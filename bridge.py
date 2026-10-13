"""Serve the camera page and relay landmark JSON to Unity on localhost."""
import asyncio
import functools
import json
import socket
import threading
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from websockets.asyncio.server import serve

ROOT = Path(__file__).resolve().parent / "web"
UDP_TARGET = ("127.0.0.1", 5052)
HAND_TARGET = ("127.0.0.1", 5053)


class QuietHandler(SimpleHTTPRequestHandler):
    def log_message(self, *args):
        pass


async def main():
    handler = functools.partial(QuietHandler, directory=str(ROOT))
    http = ThreadingHTTPServer(("127.0.0.1", 8000), handler)
    thread = threading.Thread(target=http.serve_forever, daemon=True)
    thread.start()
    udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    async def relay(connection):
        print("Camera page connected", flush=True)
        try:
            async for message in connection:
                if not isinstance(message, str):
                    continue
                try:
                    packet = json.loads(message)
                    if not isinstance(packet, dict) or not isinstance(packet.get("joints"), list):
                        continue
                    kind = packet.get("kind", "body")
                    if kind not in ("body", "hand"):
                        continue
                    expected = 21 if kind == "hand" else 33
                    if len(packet["joints"]) not in (0, expected):
                        continue
                    udp.sendto(message.encode("utf-8"), HAND_TARGET if kind == "hand" else UDP_TARGET)
                except (ValueError, OSError):
                    continue
        finally:
            # Immediately invalidate the last pose when the page disconnects.
            udp.sendto(b'{"tracked":false,"joints":[]}', UDP_TARGET)
            udp.sendto(b'{"tracked":false,"joints":[]}', HAND_TARGET)
            print("Camera page disconnected", flush=True)

    try:
        async with serve(relay, "127.0.0.1", 8765,
                         origins=["http://127.0.0.1:8000", "http://localhost:8000"],
                         max_size=32000, max_queue=1, compression=None):
            print("Open http://127.0.0.1:8000 in Safari", flush=True)
            print("Unity receives body UDP on 5052 and hand UDP on 5053. Stop with Ctrl+C.", flush=True)
            await asyncio.Future()
    finally:
        http.shutdown()
        http.server_close()
        udp.close()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        pass
    except OSError as error:
        print(f"Could not start bridge: {error}. Close other copies and try again.")
