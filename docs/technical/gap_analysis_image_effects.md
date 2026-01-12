# Gap Analysis: Image Effects

**Version**: 1.2  
**Date**: 2026-01-12  
**Scope**: `src\ShareX.Editor\ImageEffects` (Baseline) vs `EditorView/EffectsPanel` (Implementation)

---

## 🚀 Executive Summary

The `ImagesEffects` library provides a suite of algorithms for image manipulation. These are exposed to the user primarily through the **Effects Panel**.

**Overall Status**: ⚠️ **Incomplete UI**.
The backend logic for effects is properly loaded via reflection. However, the **UI is missing the input controls** for configuring effect parameters (sliders, color pickers, checkboxes).
- **Parameterless Effects**: Are **Complete** (e.g., Black & White, Inverse), as they require no user input.
- **Parameterized Effects**: Are **Pending UI**, as the application currently displays a placeholder message ("Effect parameters will be configured here") instead of actual controls.

---

## 📋 Feature Summary Table

| Category | Effect | EditorView Status | Notes |
| :--- | :--- | :--- | :--- |
| **Adjustments** | Alpha | 🔴 **Pending UI** | Needs Opacity slider. |
| **Adjustments** | BlackWhite | ✅ **Complete** | Parameterless. |
| **Adjustments** | Brightness | 🔴 **Pending UI** | Needs Brightness level. |
| **Adjustments** | Colorize | 🔴 **Pending UI** | Needs Color picker. |
| **Adjustments** | Contrast | 🔴 **Pending UI** | Needs Contrast level. |
| **Adjustments** | Gamma | 🔴 **Pending UI** | Needs Gamma level. |
| **Adjustments** | Grayscale | ✅ **Complete** | Parameterless. |
| **Adjustments** | Hue | 🔴 **Pending UI** | Needs Hue slider. |
| **Adjustments** | Inverse | ✅ **Complete** | Parameterless. |
| **Adjustments** | MatrixColor | 🔴 **Pending UI** | Needs Matrix input. |
| **Adjustments** | Polaroid | 🔴 **Pending UI** | Needs Margin/Rotation params. |
| **Adjustments** | Saturation | 🔴 **Pending UI** | Needs Saturation level. |
| **Adjustments** | Sepia | ✅ **Complete** | Parameterless (Fixed). |
| **Drawings** | DrawBackground | 🔴 **Pending UI** | Needs Color/Padding. |
| **Drawings** | DrawBackgroundImage | 🔴 **Pending UI** | Needs Path/Opacity. |
| **Drawings** | DrawBorder | 🔴 **Pending UI** | Needs Size/Color. |
| **Drawings** | DrawCheckerboard | 🔴 **Pending UI** | Needs Size/Colors. |
| **Drawings** | DrawImage | 🔴 **Pending UI** | Needs Image path. |
| **Drawings** | DrawText | 🔴 **Pending UI** | Needs Text/Font properties. |
| **Filters** | Blur | 🔴 **Pending UI** | Needs Radius. |
| **Filters** | ColorDepth | 🔴 **Pending UI** | Needs Depth/Bits. |
| **Filters** | EdgeDetect | ✅ **Complete** | Parameterless (usually). |
| **Filters** | Emboss | ✅ **Complete** | Parameterless (usually). |
| **Filters** | GaussianBlur | 🔴 **Pending UI** | Needs Radius. |
| **Filters** | Glow | 🔴 **Pending UI** | Needs Radius/Color. |
| **Filters** | MatrixConvolution | 🔴 **Pending UI** | Needs Kernel/Divisor. |
| **Filters** | MeanRemoval | ✅ **Complete** | Parameterless. |
| **Filters** | Outline | 🔴 **Pending UI** | Needs Thickness/Color. |
| **Filters** | Pixelate | 🔴 **Pending UI** | Needs Pixel Size. |
| **Filters** | Reflection | 🔴 **Pending UI** | Needs Opacity/Offset. |
| **Filters** | RGBSplit | 🔴 **Pending UI** | Needs Offset. |
| **Filters** | Shadow | 🔴 **Pending UI** | Needs Opacity/Size/Offset. |
| **Filters** | Sharpen | ✅ **Complete** | Parameterless (Fixed kernel). |
| **Filters** | Slice | 🔴 **Pending UI** | Needs Slice params. |
| **Manipulations** | AutoCrop | 🔴 **Pending UI** | Needs Color/Tolerance. |
| **Manipulations** | Canvas | 🔴 **Pending UI** | Needs Size/Color. |
| **Manipulations** | Crop | 🔴 **Pending UI** | Needs Rect coords. |
| **Manipulations** | Flip | 🔴 **Pending UI** | Needs Orient. (Horiz/Vert). |
| **Manipulations** | ForceProportions | 🔴 **Pending UI** | Needs Ratio. |
| **Manipulations** | Resize | 🔴 **Pending UI** | **Broken Implementation** + Needs Size params. |
| **Manipulations** | Rotate | 🔴 **Pending UI** | Needs Angle. |
| **Manipulations** | RoundedCorners | 🔴 **Pending UI** | Needs Radius. |
| **Manipulations** | Scale | 🔴 **Pending UI** | Needs Scale factor. |
| **Manipulations** | Skew | 🔴 **Pending UI** | Needs X/Y skew. |

---

## 🔍 Major Gaps

### 1. Missing UI Templates for Parameters
**Critical Finding**: The `GenericEffectParameterTemplate` in `EffectParameterTemplates.axaml` is a placeholder text block.
**Impact**: Users cannot configure *any* effect that requires parameters (e.g., changing Blur radius, picking a Color for Colorize). This renders the majority of effects functionally useless in the UI despite the backend logic working.

### 2. Broken Implementation: Resize
The `Resize` effect is doubly non-functional: it not only lacks UI inputs but its backend `Apply()` method is a stub that does nothing.

---

## 💡 Recommendations

### Short Term
1.  **Implement Generic Parameter UI**: Create a robust `DataTemplate` (or set of templates) that uses reflection or a PropertyGrid-like control to auto-generate UI for `int` (Slider/NumericUpDown), `bool` (CheckBox), `float`, `string`, and `SKColor` (ColorPicker).
2.  **Fix Resize Logic**: Hook up the `Resize` logic to the helper methods.

### Long Term
3.  **Specialized Templates**: Create manual, polished templates for complex effects (e.g., Matrix Convolution, DrawText) that auto-generation cannot handle gracefully.
