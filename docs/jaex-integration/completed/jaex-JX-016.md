# JX-016 Selection handling for tool change

PR: https://github.com/ShareX/ShareX.Editor/pull/31

Summary:
- Adds deselect event handling to cleanly exit selection when switching tools.
- Captures UI snapshot and readiness marker for the integration branch.

Jaex source SHAs:
- 306c3fe (Add DeselectRequested event to handle tool selection)

Files touched (high level):
- src/ShareX.Editor/ViewModels/MainViewModel.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-016.md
- integrate/jaex-JX-016.md

Tests:
- Not run (summary artifact only)

UI snapshots:
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-016.md
