# JX-005 Summary

Feature: Effect annotation robustness.

User-visible outcome:
- Fixes blur, magnify, pixelate, and spotlight behavior near edges/out-of-bounds.

Primary areas touched:
- src/ShareX.Editor/Annotations (BlurAnnotation.cs, MagnifyAnnotation.cs, PixelateAnnotation.cs)
- src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- src/ShareX.Editor/ViewModels/MainViewModel.cs (EffectStrength for spotlight mapping)

Commits (from jaex):
- 9512e64 Map EffectStrength to DarkenOpacity for Spotlight tool
- 28927d6 Improve blur annotation edge handling
- b8594c6 Fix effect annotations to handle out-of-bounds regions
- cbc5988 Improve magnify annotation region handling

UI impact:
- No menu definitions touched.
- Import Preset... and Export Preset... remain present and wired.
