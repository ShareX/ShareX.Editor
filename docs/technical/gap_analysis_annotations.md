# Gap Analysis: Annotations

**Version**: 1.0  
**Date**: 2026-01-12  
**Scope**: `src\ShareX.Editor\Annotations` (Baseline) vs `EditorView` (Implementation)

---

## 🚀 Executive Summary

The `ShareX.Editor` annotations system uses a **Hybrid Rendering** model:
- **Raster Layer**: The `EditorCore` renders the background image and bitmap-based effects (Blur, Pixelate) via SkiaSharp.
- **Vector Layer**: The `EditorView` (Avalonia) renders interactive shapes (Rectangles, Arrows, Text) as native Avalonia Controls on top of the canvas.

**Overall Status**: ✅ **Healthy**.
Most core tools are fully implemented and correctly mapped. A minor architectural divergence exists for "immediate" operations like Crop and CutOut, which bypass the Core's annotation list in favor of direct ViewModel manipulation because they don't persist as objects on the canvas.

---

## 📊 Feature Inventory & Status

### 🛠️ Core Tools (Shapes & Drawing)

| Feature | Baseline Class | Status | Implementation Details |
| :--- | :--- | :--- | :--- |
| **Rectangle** | `RectangleAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.Shapes.Rectangle`. |
| **Ellipse** | `EllipseAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.Shapes.Ellipse`. |
| **Line** | `LineAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.Shapes.Line`. |
| **Arrow** | `ArrowAnnotation` | ✅ **Complete** | Uses `Avalonia.Controls.Shapes.Path` with custom geometry geometry logic from `ArrowAnnotation`. |
| **Freehand** | `FreehandAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.Shapes.Polyline`. |
| **Text** | `TextAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.TextBox`. Editing handled by standard Avalonia input. |
| **Speech Balloon**| `SpeechBalloonAnnotation`| ✅ **Complete** | Mapped to custom `SpeechBalloonControl`. |
| **Number** | `NumberAnnotation` | ✅ **Complete** | Mapped to `Grid` containing `Ellipse` + `TextBlock`. Auto-increments. |
| **Image (Sticker)**| `ImageAnnotation` | ✅ **Complete** | Mapped to `Avalonia.Controls.Image`. |
| **Smart Eraser** | `SmartEraserAnnotation` | ✅ **Complete** | Logic in `EditorInputController` samples rendered canvas color to simulate "erasing". |

### ✨ Effect Tools

| Feature | Baseline Class | Status | Implementation Details |
| :--- | :--- | :--- | :--- |
| **Blur** | `BlurAnnotation` | ✅ **Complete** | Raster effect rendered by Core. View draws selection border. |
| **Pixelate** | `PixelateAnnotation` | ✅ **Complete** | Raster effect rendered by Core. View draws selection border. |
| **Highlighter** | `HighlightAnnotation` | ✅ **Complete** | Rendered as translucent rectangle (Alpha blending). |
| **Magnify** | `MagnifyAnnotation` | ✅ **Complete** | Raster effect. |
| **Spotlight** | `SpotlightAnnotation` | ✅ **Complete** | Mapped to custom `SpotlightControl`. Darkens area outside selection. |

### ✂️ Image Operations

| Feature | Baseline Class | Status | Implementation Details |
| :--- | :--- | :--- | :--- |
| **Crop** | `CropAnnotation` | ⚠️ **Diverged** | `EditorCore` logic is shadowed. View uses local `CropOverlay` and calls `ViewModel.CropImage` directly. |
| **Cut Out** | `CutOutAnnotation` | ⚠️ **Diverged** | `EditorCore` logic is shadowed. View uses local `CutOutOverlay` and calls `ViewModel.CutOutImage` directly. |

---

## 🔍 Identified Gaps & Findings

### 1. Diverged Logic: Crop & CutOut
**Finding**: The baseline `EditorCore` contains logic for `CropAnnotation` and `CutOutAnnotation`, intending for them to be added to the annotation list and executed later.
**Reality**: The current `EditorView` treats these as immediate, modal operations. The `EditorInputController` intercepts these tools, draws its own temporary overlay (not using the Annotation class), and executes the command immediately on mouse release.
**Impact**: The code in `EditorCore` related to these annotations is effectively dead/unreachable in the current application flow.

### 2. Smart Eraser History
**Finding**: `SmartEraser` strokes are complex objects (points + sampled color).
**Verification**: Confirmed that `EditorInputController` correctly adds these to the `EditorCore` history on release, ensuring Undo/Redo works even though they are complex shapes.

---

## 💡 Recommendations

1.  **Cleanup Dead Code**: Consider removing `CropAnnotation` and `CutOutAnnotation` logic from `EditorCore`'s `OnPointer...` methods if the immediate-mode interaction in `EditorInputController` is the desired final behavior. This reduces confusion about where the logic lives.
2.  **Verify Undo for Operations**: Since Crop/CutOut bypass the standard annotation history add, verify that `ViewModel.CropImage` and `CutOutImage` correctly push a state to the Undo stack (History).
