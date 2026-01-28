# JX-011 Summary

Feature: Picker dropdown active styling and default font size.

User-visible outcome:
- Highlights selected options in font size/width pickers and increases default font size to 30.

Primary areas touched:
- src/ShareX.Editor/Controls/FontSizePickerDropdown.axaml
- src/ShareX.Editor/Controls/FontSizePickerDropdown.axaml.cs
- src/ShareX.Editor/Controls/WidthPickerDropdown.axaml
- src/ShareX.Editor/Controls/WidthPickerDropdown.axaml.cs
- src/ShareX.Editor/ViewModels/MainViewModel.cs

Commits (from jaex):
- 6454b90 Add active state styling to picker dropdowns
- 44040ff Increase default font size to 30

UI impact:
- FontSizePickerDropdown.axaml and WidthPickerDropdown.axaml updated.
- Import Preset... and Export Preset... remain present and wired.
