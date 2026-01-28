# JX-009 Summary

Feature: Minimum shape size validation for annotation tools.

User-visible outcome:
- Prevents accidental tiny shapes by discarding shapes smaller than a minimum size.

Primary areas touched:
- src/ShareX.Editor/Views/Controllers/EditorInputController.cs

Commits (from jaex):
- 72bf393 Add minimum shape size validation to annotation tools

UI impact:
- No menu definitions touched.
- Import Preset... and Export Preset... remain present and wired.
