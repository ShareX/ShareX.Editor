# Gap Analysis: Image Effects

**Version**: 1.1  
**Date**: 2026-01-12  
**Scope**: `src\ShareX.Editor\ImageEffects` (Baseline) vs `EditorView/EffectsPanel` (Implementation)

---

## 🚀 Executive Summary

The `ImagesEffects` library provides a suite of algorithms for image manipulation. These are exposed to the user primarily through the **Effects Panel**.

**Overall Status**: ⚠️ **Partial**.
While the mechanism to expose effects is robust (reflection-based auto-discovery), distinct functional gaps exist. Most notably, the **Resize** effect is implemented as a stub and does not function. Additionally, redundancy exists between effects implemented in this library and "Tools" that perform identical actions (Blur, Pixelate).

---

## 📋 Feature Summary Table

The following table lists all effects found in the `ImageEffects` library and their implementation status in the Editor View (accessible via the Effects Panel).

| Category | Effect | EditorView |
| :--- | :--- | :--- |
| **Adjustments** | Alpha | Complete |
| **Adjustments** | BlackWhite | Complete |
| **Adjustments** | Brightness | Complete |
| **Adjustments** | Colorize | Complete |
| **Adjustments** | Contrast | Complete |
| **Adjustments** | Gamma | Complete |
| **Adjustments** | Grayscale | Complete |
| **Adjustments** | Hue | Complete |
| **Adjustments** | Inverse | Complete |
| **Adjustments** | MatrixColor | Complete |
| **Adjustments** | Polaroid | Complete |
| **Adjustments** | Saturation | Complete |
| **Adjustments** | Sepia | Complete |
| **Drawings** | DrawBackground | Complete |
| **Drawings** | DrawBackgroundImage | Complete |
| **Drawings** | DrawBorder | Complete |
| **Drawings** | DrawCheckerboard | Complete |
| **Drawings** | DrawImage | Complete |
| **Drawings** | DrawText | Complete |
| **Filters** | Blur | Complete |
| **Filters** | ColorDepth | Complete |
| **Filters** | EdgeDetect | Complete |
| **Filters** | Emboss | Complete |
| **Filters** | GaussianBlur | Complete |
| **Filters** | Glow | Complete |
| **Filters** | MatrixConvolution | Complete |
| **Filters** | MeanRemoval | Complete |
| **Filters** | Outline | Complete |
| **Filters** | Pixelate | Complete |
| **Filters** | Reflection | Complete |
| **Filters** | RGBSplit | Complete |
| **Filters** | Shadow | Complete |
| **Filters** | Sharpen | Complete |
| **Filters** | Slice | Complete |
| **Manipulations** | AutoCrop | Complete |
| **Manipulations** | Canvas | Complete |
| **Manipulations** | Crop | Complete |
| **Manipulations** | Flip | Complete |
| **Manipulations** | ForceProportions | Complete |
| **Manipulations** | Resize | **Pending** |
| **Manipulations** | Rotate | Complete |
| **Manipulations** | RoundedCorners | Complete |
| **Manipulations** | Scale | Complete |
| **Manipulations** | Skew | Complete |

---

## 🔍 Identified Gaps & Findings

### 1. 🛑 Broken Feature: Resize
**Finding**: The class `shareX.Editor.ImageEffects.Manipulations.Resize` exists and is visible in the UI.
**Issue**: The `Apply()` method contains a `TODO` comment and returns the original image without modification.
**Impact**: User selecting "Resize" in the effects panel will see no change.

### 2. 🔄 Logic Duplication
**Finding**: Core algorithms for **Blur** and **Pixelate** are implemented twice:
1.  In `ImageEffectsProcessing.cs` (used by Global Effects).
2.  Inside `BlurAnnotation.cs` and `PixelateAnnotation.cs` (used by Tools).
**Impact**: Maintenance burden. Improvements to the blur algorithm in one place will not automatically reflect in the other.

### 3. Feature Overlap
**Finding**: `Crop`, `Rotate`, and `Flip` exist as both **ImageEffects** (in the list) and **Toolbar Commands**.
**Impact**: This might confuse users. "Crop" as an effect is typically a hard-coded parameter crop, whereas the Toolbar Crop is interactive.

---

## 💡 Recommendations

### Short Term (Fixes)
1.  **Implement Resize**: Connect `Resize.Apply()` to the existing `ImageEffectsProcessing.ResizeImage` helper method. This is a quick fix.

### Medium Term (Refactor)
2.  **Consolidate Algorithms**: Refactor `BlurAnnotation` and `PixelateAnnotation` to call `ImageEffectsProcessing.ApplyBlur` and `Pixelate`. This ensures a single source of truth for these algorithms.
3.  **Curate Effects List**: Review if `Crop` and `DrawText` should be hidden from the "Effects" list, as they are better served by their interactive Tool counterparts.
