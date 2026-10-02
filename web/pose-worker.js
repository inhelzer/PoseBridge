// Classic worker: MediaPipe loads its WASM runtime with importScripts().
// Dynamic import keeps the library as ESM without making this a module worker.

let pose;
self.onmessage = async ({ data }) => {
  if (data.type === "init") {
    try {
      const { FilesetResolver, PoseLandmarker } = await import(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.21/vision_bundle.mjs"
      );
      const vision = await FilesetResolver.forVisionTasks(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.21/wasm"
      );
      pose = await PoseLandmarker.createFromOptions(vision, {
        baseOptions: {
          modelAssetPath: "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/1/pose_landmarker_lite.task",
          delegate: "CPU"
        },
        runningMode: "VIDEO", numPoses: 1,
        minPoseDetectionConfidence: 0.5,
        minPosePresenceConfidence: 0.5,
        minTrackingConfidence: 0.5,
        outputSegmentationMasks: false
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
          visibility: p.visibility ?? 0, presence: p.presence ?? 0
        }))
      });
    } catch (error) {
      self.postMessage({ type: "error", message: String(error) });
    } finally { data.bitmap.close(); }
  }
};
