// Classic worker: MediaPipe loads its WASM runtime with importScripts().
// Dynamic import keeps the library as ESM without making this a module worker.

let pose;
self.onmessage = async ({ data }) => {
  if (data.type === "init") {
    try {
      const { FilesetResolver, HandLandmarker } = await import(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.21/vision_bundle.mjs"
      );
      const vision = await FilesetResolver.forVisionTasks(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.21/wasm"
      );
      pose = await HandLandmarker.createFromOptions(vision, {
        baseOptions: {
          modelAssetPath: "https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task",
          delegate: "CPU"
        },
        runningMode: "VIDEO", numHands: 1,
        minHandDetectionConfidence: 0.5,
        minHandPresenceConfidence: 0.5,
        minTrackingConfidence: 0.5
      });
      self.postMessage({ type: "ready" });
    } catch (error) {
      self.postMessage({ type: "error", message: String(error) });
    }
  } else if (data.type === "frame") {
    try {
      const result = pose.detectForVideo(data.bitmap, data.timestamp);
      self.postMessage({
        type: "result", timestamp: data.timestamp,
        joints: (result.landmarks[0] || []).map(p => ({
          x: p.x, y: p.y, z: p.z,
          visibility: 1, presence: 1
        }))
      });
    } catch (error) {
      self.postMessage({ type: "error", message: String(error) });
    } finally { data.bitmap.close(); }
  }
};
