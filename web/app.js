const video = document.querySelector("#video");
const canvas = document.querySelector("#canvas");
const ctx = canvas.getContext("2d");
const modelStatus = document.querySelector("#model");
const connection = document.querySelector("#connection");
const values = document.querySelector("#values");
const startButton = document.querySelector("#start");
const stopButton = document.querySelector("#stop");
const cameraSelect = document.querySelector("#camera");
const refreshButton = document.querySelector("#refresh");
const cameraStatus = document.querySelector("#cameras");
const links = [[11,12],[11,13],[13,15],[12,14],[14,16],[11,23],[12,24],[23,24],[23,25],[25,27],[27,29],[29,31],[27,31],[24,26],[26,28],[28,30],[30,32],[28,32]];
let socket, worker, stream, running = false, busy = false;
let generation = 0, lastSent = 0, lastVideoTime = -1;
let starting = false, listGeneration = 0;
let animationId = null;

async function refreshCameras(requestPermission = false, preferredId = cameraSelect.value) {
  const requestId = ++listGeneration;
  let permissionStream;
  try {
    if (requestPermission && !stream) {
      permissionStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    }
    const devices = await navigator.mediaDevices.enumerateDevices();
    if (requestId !== listGeneration) return;
    const cameras = devices.filter(d => d.kind === "videoinput" && d.deviceId);
    cameraSelect.replaceChildren(new Option("Default camera", ""));
    cameras.forEach((d, i) => cameraSelect.add(new Option(d.label || `Camera ${i + 1}`, d.deviceId)));
    if (cameras.some(d => d.deviceId === preferredId)) cameraSelect.value = preferredId;
    cameraStatus.textContent = cameras.length
      ? `${cameras.length} camera(s) available. Select the camera you want to track.`
      : "Click Start camera or Refresh cameras and allow access.";
  } catch (error) {
    cameraStatus.textContent = `Could not list cameras: ${error.message}. Check camera permissions.`;
  } finally {
    permissionStream?.getTracks().forEach(t => t.stop());
  }
}

function connect() {
  socket = new WebSocket("ws://127.0.0.1:8765");
  socket.onopen = () => { connection.textContent = "Bridge connected — Unity can receive tracking data."; };
  socket.onclose = () => {
    connection.textContent = "Bridge disconnected. Keep bridge.py running. Reconnecting…";
    setTimeout(connect, 1500);
  };
  socket.onerror = () => socket.close();
}
connect();

function send(joints, timestamp = performance.now()) {
  if (socket?.readyState === WebSocket.OPEN && socket.bufferedAmount < 16000) {
    socket.send(JSON.stringify({ tracked: joints.length === 33, timestamp, joints }));
  }
}

function draw(joints) {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.strokeStyle = "#73edbb"; ctx.lineWidth = 3;
  for (const [a,b] of links) {
    if (!joints[a] || !joints[b] || joints[a].visibility < .5 || joints[b].visibility < .5) continue;
    ctx.beginPath(); ctx.moveTo(joints[a].x * canvas.width, joints[a].y * canvas.height);
    ctx.lineTo(joints[b].x * canvas.width, joints[b].y * canvas.height); ctx.stroke();
  }
  ctx.fillStyle = "#ff7676";
  for (const p of joints) {
    if (p.visibility < .5) continue;
    ctx.beginPath(); ctx.arc(p.x * canvas.width, p.y * canvas.height, 4, 0, Math.PI * 2); ctx.fill();
  }
  const p = joints[16];
  values.textContent = p
    ? `RightWrist (Unity mirror coordinates):\nx: ${(1-p.x).toFixed(3)}\ny: ${(1-p.y).toFixed(3)}\nvisibility: ${p.visibility.toFixed(3)}`
    : "No body detected";
}

function stop() {
  running = false; starting = false; generation++; busy = false;
  if (animationId !== null) cancelAnimationFrame(animationId);
  animationId = null;
  worker?.terminate(); worker = null;
  stream?.getTracks().forEach(t => t.stop()); stream = null;
  video.srcObject = null;
  send([]); draw([]);
  startButton.disabled = false; stopButton.disabled = true;
  cameraSelect.disabled = false; refreshButton.disabled = false;
}

async function tick(now) {
  if (!running) return;
  // At most 20 detections/second, one frame in flight. Never build a queue.
  if (!busy && now - lastSent >= 50 && video.readyState >= 2 && video.currentTime !== lastVideoTime) {
    busy = true; lastSent = now; lastVideoTime = video.currentTime;
    const currentGeneration = generation;
    try {
      const bitmap = await createImageBitmap(video);
      if (!running || generation !== currentGeneration) { bitmap.close(); return; }
      worker.postMessage({ type: "frame", bitmap, timestamp: now }, [bitmap]);
    } catch (error) {
      if (generation === currentGeneration) { stop(); modelStatus.textContent = `Camera error: ${error}`; }
      return;
    }
  }
  if (running) animationId = requestAnimationFrame(tick);
}

async function start() {
  if (starting) return;
  stop();
  const currentGeneration = generation;
  const selectedId = cameraSelect.value;
  starting = true;
  startButton.disabled = true;
  stopButton.disabled = false;
  cameraSelect.disabled = true; refreshButton.disabled = true;
  modelStatus.textContent = "Loading camera and pose model…";
  try {
    const newStream = await navigator.mediaDevices.getUserMedia({
      video: {
        ...(selectedId ? { deviceId: { exact: selectedId } } : {}),
        width: { ideal: 640 }, height: { ideal: 480 }, frameRate: { ideal: 30 }
      }, audio: false
    });
    if (generation !== currentGeneration) { newStream.getTracks().forEach(t => t.stop()); return; }
    stream = newStream;
    const track = stream.getVideoTracks()[0];
    track.onended = () => {
      if (generation !== currentGeneration) return;
      stop(); modelStatus.textContent = "Camera disconnected. Reconnect it, refresh cameras, and start again.";
      void refreshCameras();
    };
    video.srcObject = stream;
    await video.play();
    if (generation !== currentGeneration) return;
    canvas.width = video.videoWidth; canvas.height = video.videoHeight;
    await refreshCameras(false, track.getSettings().deviceId || selectedId);
    if (generation !== currentGeneration) return;
    worker = new Worker("./pose-worker.js?v=3");
    worker.onmessage = ({ data }) => {
      if (generation !== currentGeneration) return;
      if (data.type === "ready") {
        starting = false;
        cameraSelect.disabled = false; refreshButton.disabled = false;
        running = true; busy = false; lastVideoTime = -1; lastSent = 0;
        stopButton.disabled = false;
        modelStatus.textContent = `Tracking: ${track.label || "selected camera"}. Up to 20 detections per second.`;
        animationId = requestAnimationFrame(tick);
      } else if (data.type === "result") {
        busy = false; draw(data.joints); send(data.joints, data.timestamp);
      } else if (data.type === "error") {
        stop(); modelStatus.textContent = `Tracking error: ${data.message}`;
      }
    };
    worker.onerror = event => {
      if (generation !== currentGeneration) return;
      stop(); modelStatus.textContent = `Model loading failed: ${event.message}. Check your internet connection.`;
    };
    worker.postMessage({ type: "init" });
  } catch (error) {
    if (generation !== currentGeneration) return;
    stop(); modelStatus.textContent = `Could not start camera: ${error.message}. Check permissions, close other camera apps, or refresh and select another camera.`;
  }
}
startButton.onclick = start;
stopButton.onclick = () => { stop(); modelStatus.textContent = "Camera stopped."; };
cameraSelect.onchange = () => { if (running) void start(); };
refreshButton.onclick = async () => {
  refreshButton.disabled = true; startButton.disabled = true; cameraSelect.disabled = true;
  await refreshCameras(true);
  if (!starting) {
    refreshButton.disabled = false; cameraSelect.disabled = false; startButton.disabled = running;
  }
};
navigator.mediaDevices?.addEventListener("devicechange", () => { void refreshCameras(); });
void refreshCameras();
window.addEventListener("pagehide", stop);
