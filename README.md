# PoseBridge

Camera-based body tracking for Unity 2D. A browser page detects body landmarks with MediaPipe, and a local Python bridge sends them to Unity.

The repository includes the Unity project, scenes, prefabs, scripts, camera page, bridge, macOS launcher, and a demonstration video. No separate Unity package is required to open this project.

## Requirements

- Unity Hub and Unity **6000.3.6f1** (the version recorded in the project).
- Python **3.10 or newer**, available as `python3`.
- A camera accessible to the browser: built-in camera, USB webcam, or iPhone via macOS Continuity Camera.
- Internet access when loading the camera page and MediaPipe resources.

The double-click launcher is for **macOS** and opens **Safari**. Unity and the bridge run on the same computer. This setup targets the Unity Editor or a desktop application, not Unity WebGL.

## Repository layout

| Path | Contents |
| --- | --- |
| `PoseProject/` | Unity project: Assets, Packages, and ProjectSettings |
| `PoseProject/Assets/Scenes/01.unity` | Scene containing the pose receiver, stick figure, and demo objects |
| `PoseProject/Assets/Scenes/SampleScene.unity` | Basic scene with camera and lighting |
| `PoseProject/Assets/Scripts/` | Tracking, stick figure, and demo gameplay scripts |
| `web/` | Camera selection, preview, and MediaPipe tracking |
| `bridge.py` | Local HTTP server and WebSocket-to-UDP bridge |
| `requirements.txt` | Python dependencies |
| `Start-PoseBridge.command` | macOS launcher |
| `video/mediaPipe1.mp4` | Demonstration video |

## First-time setup on macOS

Download and extract a published release, or clone this repository. Keep the project folders and launcher together.

Open Terminal in the repository folder. You can type `cd ` (with a space), drag the folder into Terminal, and press Enter.

Check Python:

```bash
python3 --version
```

Create a local Python environment and install dependencies:

```bash
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
chmod +x Start-PoseBridge.command
```

Create this environment on each computer. A copied `.venv` from another computer is not a portable installation.

In Unity Hub, choose **Add project from disk** (or **Open**) and select the **PoseProject** folder inside the repository. Open it with Unity 6000.3.6f1 and allow the initial import to finish.

In Unity's Project window, open **Assets → Scenes → 01**. You do not need to create a new Unity project or rebuild the stick figure.

## Start a session

1. Double-click `Start-PoseBridge.command`.
2. Leave its Terminal window open. Safari opens at http://127.0.0.1:8000.
3. Click **Refresh cameras** and allow camera access.
4. Select a camera and click **Start camera**.
5. Open the `01` scene in Unity and enter **Play** mode.
6. Keep the Safari page visible beside Unity while tracking.

Only one camera is tracked at a time. Selecting another camera switches the input sent to Unity.

For a side view, place the camera beside the player while the player faces the computer.

### iPhone camera

Enable Continuity Camera on the iPhone and make it available to the Mac. Close FaceTime or other applications using the camera. If it is missing, connect it by USB and click **Refresh cameras** again.

This project's launcher uses Safari, which worked with the iPhone camera in the development setup. Availability still depends on the Mac, iPhone, permissions, and operating system setup.

## Stop a session

1. Exit Play mode in Unity.
2. Click **Stop** on the camera page.
3. Press **Control+C** in the launcher Terminal window to stop the bridge.

Start the next session by double-clicking the launcher again. The Python setup is only needed once per local installation.

## Tracking scripts

| Script | Role |
| --- | --- |
| `PoseReceiver.cs` | Receives all 33 pose landmarks over UDP |
| `MoveWithJoint.cs` | Moves a Unity object using a selected landmark |
| `StickFigureManager.cs` | Connects assigned joints with lines and calculates the neck between the shoulders |

The project also includes scripts for the demo gameplay.

When adapting the figure, assign each joint's `MoveWithJoint` component to the pose receiver, then assign the joint transforms once in `StickFigureManager`. Keep the same movement area and center settings across the joints.

The neck is calculated from the midpoint of the shoulders. A single line connects it to the nose; no extra detected landmark is needed.

The camera preview is mirrored. Unity's **Mirror X** setting controls mirroring of the Unity coordinates independently.

## Connection and troubleshooting

| Connection | Address |
| --- | --- |
| Camera page | `http://127.0.0.1:8000` |
| Browser → bridge | `ws://127.0.0.1:8765` |
| Bridge → Unity | UDP `127.0.0.1:5052` |

- **Unity does not move:** confirm Play mode is active in the `01` scene, the camera page is tracking, and the pose receiver uses port `5052`.
- **Receiving is off:** check that the bridge is running and the page reports a bridge connection.
- **Receiving is on but Tracking is off:** move into the camera frame and check lighting and landmark visibility.
- **A joint stops moving:** a landmark below the receiver's visibility threshold retains its last accepted position.
- **Port already in use:** stop the previous bridge with Control+C before launching another copy.
- **Python environment missing:** run the first-time setup from the repository folder.
- **Terminal mentions Chrome:** this is an older message in the bridge; the launcher opens Safari.

Save scene changes outside Play mode so that they persist.

## Development and versioned downloads

Continue development on `main`. A published release should use a version tag such as `v1.0` to identify a specific snapshot. Later pushes to `main` do not change that snapshot.

For a project page that stays fixed while development continues, publish GitHub Pages from a separate `gh-pages` branch. Link its download button to a specific release, not to the current `main` archive or a moving `latest` link. Keep the page's video in that branch or reference a pinned version.

Local Python environments, Unity-generated caches, and editor-specific files should not be included in a release. Keep Unity's `Assets` (including `.meta` files), `Packages`, and `ProjectSettings`.

If creating a standalone Unity build later, first configure its build scenes: the current build settings list `SampleScene`, while the pose demonstration is in `01`.

