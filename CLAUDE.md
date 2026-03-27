# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity **2022.3.62f3** horror game "The Campus Is Wrong" (AnomalyGames). Render pipeline: **URP 14.0.12**.

All scripts are in `Assets/Scripts/` and written in **C# with Russian comments/headers**.

## Architecture

### Player hierarchy (scene)
```
Player (root)
├── CharacterController          ← physics / movement
├── PlayerController.cs          ← движение, mouse look, аниматор
├── CharacterAnimator.cs         ← head bob, camera tilt
├── CameraHolder                 ← CharacterAnimator двигает localPosition/localRotation.z
│   └── Camera                  ← PlayerController двигает localRotation.x (pitch)
└── CharacterMesh                ← Animator (Humanoid rig, applyRootMotion = false)
```

### Script responsibilities
| Script | Update timing | Controls |
|---|---|---|
| `PlayerController` | `Update` only | CharacterController.Move, Animator Speed parameter |
| `CameraController` | `LateUpdate` only | Mouse look (pitch on cameraTransform X, yaw on Player root Y), head bob (cameraHolder localPosition), tilt (cameraHolder localRotation Z only), Camera.nearClipPlane |

**Critical axis rule:** `HandleMouseLook` sets `cameraTransform.localRotation` (X pitch). `HandleCameraTilt` sets `cameraHolder.localEulerAngles` with only Z changed — X is never touched there. These must stay on separate transforms (cameraHolder is parent of cameraTransform).

### Animator
- Controller: `Assets/PlayerAnimator.controller`
- Single float parameter: `Speed` (0.0 = Idle, 0.5 = Walk, 1.0 = Run, 0.25 = Crouch)
- Animations: `Assets/Models/Characters/{Idle,Walk,Running}.anim`
- Model: `Assets/Models/Characters/Главная героиня.fbx` (Humanoid rig)
- `applyRootMotion` is forced to `false` in `PlayerController.Start()`

### First-person body visibility
- `nearClipPlane = 0.15` on the Camera — голова (за near clip) не рендерится, тело видно
- Pitch clamp: `-60° .. +80°` — нельзя задрать камеру за спину модели
- **Не скрывать голову через масштаб кости** — голова должна существовать в сцене

### Key packages
- `com.unity.inputsystem` 1.14.2 — установлен, но скрипты пока используют legacy `Input.GetAxis`
- `com.unity.cinemachine` 2.10.6 — установлен, не используется в текущих скриптах
- `com.unity.ai.navigation` 1.1.7 — для будущего AI
- `com.unity.postprocessing` 3.4.0 — постпроцессинг

## Game systems

### Scenes
- `MainMenu` — один GameObject с `MainMenuController.cs`; строит UI процедурно в Awake
- `SampleScene` — игровая сцена; один GameObject с `GameManager` + `AnomalyManager` + `RoomGenerator`

### Anomaly pipeline (порядок вызовов важен)
```
GameManager.Start()
  → RoomGenerator.Generate()      // строит комнату, добавляет AnomalyObject на объекты
  → AnomalyManager.SpawnAnomaly() // FindObjectsOfType<AnomalyObject>, выбирает случайный
                                  // применяет WrongColor / Floating / ExtraObject
```
- `AnomalyObject.cs` — чистый маркер, только `bool IsAnomalous`
- ExtraObject-тип переносит `IsAnomalous = true` на новосозданный куб, не на базовый объект
- Raycast (GameManager.Update) смотрит только на объекты с `AnomalyObject` в пределах `interactionDistance`

### HUD
- Singleton `HUD.Instance` — весь UI строится в Awake процедурно
- `ShowPrompt(bool)` — подсказка "[E] — Отметить аномалию"
- `ShowWin()` / `ShowLose()` — оверлей с кнопками Заново / Главное меню

### Settings persistence
- Чувствительность мыши хранится в `PlayerPrefs.GetFloat("MouseSensitivity", 100f)`
- Записывается `MainMenuController`, читается `CameraController.Start()`
- Диапазон: 20–300, дефолт 100

### RoomGenerator
- Создаёт комнату 10×3×12m из `GameObject.CreatePrimitive(Cube)`
- Все интерактивные объекты получают `AnomalyObject` через `Anomalize(go)`
- В конце `Generate()` ищет тег `"Player"` и выставляет стартовую позицию
- URP-шейдер: `"Universal Render Pipeline/Lit"` (фолбэк на `"Standard"`)
