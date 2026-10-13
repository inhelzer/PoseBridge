# PoseBridge v1.1

Camera-based **body and single-hand tracking for Unity 2D**.

The project includes two ready-to-open Unity scenes and one camera page with a **Body / Hand** selector. Track one mode at a time and open the matching scene in Unity.

## Requirements

- Unity Hub and Unity **6000.3.6f1**.
- Python **3.10 or newer**, available as `python3`.
- A camera accessible to Safari: built-in camera, USB webcam, or iPhone via Continuity Camera.
- Internet access to load the MediaPipe library and models.

The launcher is for **macOS** and opens **Safari**. Unity and the Python bridge run on the same computer. This setup supports the Unity Editor / desktop, not Unity WebGL.

## Included scenes

| Browser selection | Unity scene | Tracking |
| --- | --- | --- |
| Body | `PoseProject/Assets/Scenes/body.unity` | Body landmarks and a 2D stick figure |
| Hand | `PoseProject/Assets/Scenes/hand.unity` | 21 landmarks of one hand and its 2D skeleton |

`SampleScene` is a basic scene, not the tracking demo. No separate Unity package or manual reconstruction is needed.

## First-time setup on macOS

Extract the release ZIP and keep its folders together.

Open Terminal in the extracted **PoseBridge** folder, beside `bridge.py` and `requirements.txt`. You can type `cd ` (with a space), drag the folder into Terminal, and press Enter.

Check Python, then run:

```bash
python3 --version
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
chmod +x Start-PoseBridge.command
```

Create the Python environment on each computer; do not copy another installation's `.venv`.

In Unity Hub, choose **Add project from disk** / **Open** and select the **PoseProject** folder inside the extracted folder. Open with Unity 6000.3.6f1 and allow the initial import to finish.

## Start a session

1. Double-click `Start-PoseBridge.command`.
2. Leave Terminal open. Safari opens http://127.0.0.1:8000.
3. Choose **Body** or **Hand** in the **Tracking** selector.
4. Click **Refresh cameras**, allow camera access, and select a camera.
5. Click **Start camera**.
6. In Unity, open **Assets → Scenes → body** or **hand**, matching the browser selection.
7. Enter **Play** mode. Keep Safari visible beside Unity.

For Hand mode, show **one hand**. The current version does not offer separate left/right hand selection.

Changing Body / Hand stops tracking and clears the previous input. Click **Start camera** again. Exit Play before opening the matching Unity scene.

You can select another camera while tracking. Only one camera is used at a time.

### iPhone camera

Enable Continuity Camera and make the iPhone available to the Mac. Close FaceTime or other apps using the camera. If it is missing, try USB and click **Refresh cameras** again.

To capture a body from the side while facing the computer, place the camera beside the player.

## Stop a session

Exit Play in Unity, click **Stop** on the camera page, then press **Control+C** in Terminal.

Next time, double-click the launcher again. No repeated Python installation is needed.

## Scripts

| Body | Single hand | Role |
| --- | --- | --- |
| `PoseReceiver.cs` | `HandReceiver.cs` | Receive landmarks |
| `MoveWithJoint.cs` | `MoveWithHandJoint.cs` | Move a joint object |
| `StickFigureManager.cs` | `HandFigureManager.cs` | Connect assigned joint objects with lines |

The two managers are independent. Joint objects are assigned once in their matching manager. Use the same movement size and center across the joints of a figure.

The body manager calculates a neck between the shoulders. The hand manager connects the fingers and palm. Both render in 2D.

The camera preview is mirrored. The receiver's **Mirror X** setting controls Unity's coordinates separately.

## Connections

| Connection | Address |
| --- | --- |
| Camera page | `http://127.0.0.1:8000` |
| Browser → bridge | `ws://127.0.0.1:8765` |
| Body → Unity | UDP `127.0.0.1:5052` |
| Hand → Unity | UDP `127.0.0.1:5053` |

## Troubleshooting

- **Nothing moves:** match the browser mode to the Unity scene and enter Play.
- **Receiving is off:** check that Terminal is running and the browser reports a bridge connection. Check the matching receiver's port.
- **Receiving is on but Tracking is off:** put the body or hand in frame and check the lighting.
- **Port already in use:** stop the previous bridge with Control+C.
- **Python environment missing:** run the first-time setup in the extracted project folder.
- **Body / Hand selector missing:** refresh Safari to load the new camera page.
- **A joint retains its position:** joint objects keep their last accepted position when tracking is lost. Hand lines can hide when tracking is lost.

Save Unity changes outside Play mode.

## Release and development

The release ZIP includes the Unity project, camera page, bridge, launcher, dependencies list, and this README. The demonstration video stays on the project page and is omitted from the distribution ZIP.

Continue development on `main`. The public project page is published separately from `gh-pages`. Its download button points to the **v1.1** release; later pushes to `main` do not update that download.

Before distributing, omit local environments and generated caches. Retain Unity's **Assets** (including `.meta` files), **Packages**, and **ProjectSettings**.

Project page: https://inhelzer.github.io/PoseBridge/
Repository: https://github.com/inhelzer/PoseBridge
