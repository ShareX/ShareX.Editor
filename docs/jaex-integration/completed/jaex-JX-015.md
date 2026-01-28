# JX-015 Smart padding edit-state preservation

PR: https://github.com/ShareX/ShareX.Editor/pull/30

Summary:
- Preserves editing state when smart padding is applied to avoid losing active edits.
- Captures UI snapshot and readiness marker for the integration branch.

Jaex source SHAs:
- 8c71db0 (Preserve editing state during smart padding)

Files touched (high level):
- src/ShareX.Editor/EditorCore.cs
- src/ShareX.Editor/ViewModels/MainViewModel.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-015.md
- integrate/jaex-JX-015.md

Tests:
- Not run (summary artifact only)

UI snapshots:
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-015.md
