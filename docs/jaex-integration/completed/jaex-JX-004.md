# JX-004 Summary

Feature: Annotation interaction and state fixes.

User-visible outcome:
- Improves highlight/shape drawing behavior.
- Disables hit testing during line/arrow creation.
- Keeps HasAnnotations state in sync after removal.

Primary areas touched:
- src/ShareX.Editor/Annotations/HighlightAnnotation.cs
- src/ShareX.Editor/Views/Controllers/EditorInputController.cs

Commits (from jaex):
- 92eb4a1 Improve highlight and shape drawing behavior
- b643a54 Disable hit testing for new line and arrow shapes
- 90a0ef2 Update HasAnnotations after removing annotation

UI impact:
- No menu definitions touched.
- Import Preset... and Export Preset... remain present and wired.
