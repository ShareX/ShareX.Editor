# JX-008 Summary

Feature: Downgrade Avalonia packages to 11.3.9.

User-visible outcome:
- Reverts Avalonia package versions to 11.3.9 for stability/compatibility.

Primary areas touched:
- src/ShareX.Editor/ShareX.Editor.csproj
- src/ShareX.Editor.Loader/ShareX.Editor.Loader.csproj

Commits (from jaex):
- 430ce80 Downgrade Avalonia packages to version 11.3.9

UI impact:
- No menu definitions touched.
- Import Preset... and Export Preset... remain present and wired.
