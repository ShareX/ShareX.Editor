# Gap Analysis: Image Effects

**Version**: 1.3  
**Date**: 2026-01-12  
**Scope**: `src\ShareX.Editor\ImageEffects` (Baseline) vs `EditorView/EffectsPanel` (Implementation)

---

## 🚀 Executive Summary

The `ImagesEffects` library provides a suite of algorithms for image manipulation. These are exposed to the user primarily through the **Effects Panel**.

**Overall Status**: ⚠️ **Incomplete UI**.
Most backend logic is present, but the UI lacks specific controls for configuring parameters. The table below is prioritized by **Implementation Difficulty**, starting with effects that are already working or easiest to fix.

---

## 📋 Feature Summary Table (By Difficulty)

| Category | Effect | Complexity | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| **Adjustments** | BlackWhite | � **Trivial** | ✅ **Complete** | Parameterless. Already working. |
| **Adjustments** | Grayscale | 🟢 **Trivial** | ✅ **Complete** | Parameterless. Already working. |
| **Adjustments** | Inverse | � **Trivial** | ✅ **Complete** | Parameterless. Already working. |
| **Adjustments** | Sepia | � **Trivial** | ✅ **Complete** | Parameterless. Already working. |
| **Filters** | EdgeDetect | 🟢 **Trivial** | ✅ **Complete** | Usually parameterless. |
| **Filters** | Emboss | � **Trivial** | ✅ **Complete** | Usually parameterless. |
| **Filters** | MeanRemoval | � **Trivial** | ✅ **Complete** | Parameterless. |
| **Filters** | Sharpen | 🟢 **Trivial** | ✅ **Complete** | Parameterless (Fixed kernel). |
| **Adjustments** | Alpha | � **Easy** | �🔴 **Pending UI** | Needs single **Slider** (0.0 - 1.0). |
| **Adjustments** | Brightness | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Slider** (-100 - 100). |
| **Adjustments** | Contrast | � **Easy** | �🔴 **Pending UI** | Needs single **Slider**. |
| **Adjustments** | Gamma | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Slider**. |
| **Adjustments** | Hue | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Slider**. |
| **Adjustments** | Saturation | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Slider**. |
| **Filters** | Blur | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric/Slider** (Radius). |
| **Filters** | GaussianBlur | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric/Slider** (Radius). |
| **Filters** | Pixelate | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric** (Size). |
| **Manipulations** | Rotate | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric** (Angle). |
| **Manipulations** | RoundedCorners | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric** (Radius). |
| **Manipulations** | Scale | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric** (Factor). |
| **Manipulations** | ForceProportions | 🟢 **Easy** | 🔴 **Pending UI** | Needs single **Numeric** (Ratio). |
| **Adjustments** | Colorize | 🟡 **Moderate** | 🔴 **Pending UI** | Needs **Color Picker**. |
| **Adjustments** | Polaroid | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Checkbox + Margin. |
| **Drawings** | DrawBackground | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Color + Padding. |
| **Drawings** | DrawBorder | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Size + Color. |
| **Filters** | Glow | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Radius + Color. |
| **Filters** | Outline | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Size + Color. |
| **Filters** | Reflection | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Opacity + Offset (Two sliders). |
| **Filters** | RGBSplit | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Offset (Point X/Y). |
| **Filters** | Shadow | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Opacity + Size + Offset. |
| **Manipulations** | AutoCrop | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Color + Tolerance. |
| **Manipulations** | Flip | 🟡 **Moderate** | 🔴 **Pending UI** | Needs Enum/Checks (Horizontal/Vertical). |
| **Manipulations** | Skew | 🟡 **Moderate** | 🔴 **Pending UI** | Needs X/Y values. |
| **Adjustments** | MatrixColor | 🔴 **Hard** | 🔴 **Pending UI** | Needs 5x4 Matrix Grid input. |
| **Drawings** | DrawBackgroundImage | 🔴 **Hard** | 🔴 **Pending UI** | Needs File Picker + Opacity. |
| **Drawings** | DrawCheckerboard | 🔴 **Hard** | 🔴 **Pending UI** | Needs Size + Two Colors. |
| **Drawings** | DrawImage | 🔴 **Hard** | 🔴 **Pending UI** | Needs File Picker. |
| **Drawings** | DrawText | 🔴 **Hard** | 🔴 **Pending UI** | Needs String + Font Picker + Color + Styling. |
| **Filters** | ColorDepth | 🔴 **Hard** | 🔴 **Pending UI** | Needs specialized bit-depth selection. |
| **Filters** | MatrixConvolution | 🔴 **Hard** | 🔴 **Pending UI** | Needs custom Kernel Grid input. |
| **Filters** | Slice | 🔴 **Hard** | 🔴 **Pending UI** | Needs complex slice parameter config. |
| **Manipulations** | Canvas | 🔴 **Hard** | 🔴 **Pending UI** | Needs Width/Height + Color + Alignment logic. |
| **Manipulations** | Crop | 🔴 **Hard** | 🔴 **Pending UI** | Needs Rect coords (UX usually interactive, not numeric). |
| **Manipulations** | Resize | 🔴 **Hard** | 🔴 **Pending UI** | **Broken Backend** + Needs Width/Height inputs. |

---

## 🔍 Major Gaps & Recommendations

### 1. Missing UI Generators (The "Easy" Tier)
**Gap**: There is no generic UI generator for `int`, `float`, `bool`, or `SKColor`.
**Solution**: Implementing a `DynamicEffectParameterView` that uses Reflection to auto-generate:
-   `float`/`int` -> `Slider` or `NumericUpDown`
-   `bool` -> `CheckBox`
-   `SKColor` -> `ColorPicker`
**Impact**: This single feature would immediately fix the "Pending UI" status for **24 Effects** (Tiers Easy & Moderate).

### 2. Complex Inputs (The "Hard" Tier)
**Gap**: Effects like `DrawText` or `MatrixColor` require specialized controls that cannot be easily auto-generated.
**Solution**: These require custom `DataTemplate`s defined manually in XAML for each specific Effect Type.

### 3. Broken Backend
**Gap**: `Resize` effect does not work.
**Solution**: Wire up `Resize.Apply` to `ImageEffectsProcessing.ResizeImage`.

