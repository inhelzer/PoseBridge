#!/bin/bash
# Keep this file beside bridge.py, inside your existing PoseBridge folder.
cd -- "$(dirname -- "${BASH_SOURCE[0]}")" || exit 1

if [ ! -f bridge.py ] || [ ! -d web ]; then
    echo "Place Start-PoseBridge.command inside PoseBridge, beside bridge.py."
    read -r -p "Press Enter to close..."
    exit 1
fi
if [ ! -x .venv/bin/python ]; then
    echo "The project's Python environment is missing."
    echo "Run the original one-time setup in this folder first:"
    echo "python3 -m venv .venv"
    echo ".venv/bin/python -m pip install -r requirements.txt"
    read -r -p "Press Enter to close..."
    exit 1
fi

exec .venv/bin/python -u - <<'PY'
import signal
import socket
import subprocess
import sys
import time
import urllib.request
from pathlib import Path

URL = "http://127.0.0.1:8000"
ROOT = Path.cwd()

def interrupt(signum, frame):
    raise KeyboardInterrupt

def main():
    # Do not start a duplicate bridge or interfere with another local server.
    for port in (8000, 8765):
        with socket.socket() as probe:
            try:
                probe.bind(("127.0.0.1", port))
            except OSError:
                print(f"Port {port} is already in use.")
                print("Stop the previous bridge with Control+C, then try again.")
                return 1

    print("Starting Unity Pose Input...")
    print(f"Project: {ROOT}")
    print("Keep this Terminal window open. Press Control+C to stop.")
    process = subprocess.Popen(
        [sys.executable, "-u", str(ROOT / "bridge.py")],
        cwd=ROOT,
        start_new_session=True
    )
    # Local requests must not be routed through a configured proxy.
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    try:
        deadline = time.monotonic() + 20
        while time.monotonic() < deadline:
            if process.poll() is not None:
                print("Bridge exited before the page was ready. See the error above.")
                return 1
            try:
                with opener.open(URL, timeout=1) as response:
                    ready = response.status == 200
                if ready and process.poll() is None:
                    break
            except (OSError, urllib.error.URLError):
                pass
            time.sleep(0.2)
        else:
            print("The camera page did not become ready within 20 seconds.")
            return 1

        # Open Safari: Continuity Camera works there on this Mac.
        opened = subprocess.run(
            ["/usr/bin/open", "-a", "Safari", URL],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL
        )
        if opened.returncode != 0:
            print(f"Could not open Safari. Open Safari manually and visit: {URL}")
        print("Choose a camera on the page and click Start camera.")
        return process.wait()
    finally:
        if process.poll() is None:
            process.send_signal(signal.SIGINT)
            try:
                process.wait(timeout=4)
            except subprocess.TimeoutExpired:
                process.terminate()
                try:
                    process.wait(timeout=2)
                except subprocess.TimeoutExpired:
                    process.kill()
                    process.wait()
        print("PoseBridge stopped.")

if __name__ == "__main__":
    signal.signal(signal.SIGHUP, interrupt)
    signal.signal(signal.SIGTERM, interrupt)
    try:
        code = main()
    except KeyboardInterrupt:
        code = 0
    except Exception as error:
        print(f"Could not start PoseBridge: {error}")
        code = 1
    if code:
        try:
            input("Press Enter to close...")
        except (EOFError, KeyboardInterrupt):
            pass
    sys.exit(code)
PY
