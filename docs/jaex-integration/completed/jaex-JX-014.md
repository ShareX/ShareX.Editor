# JX-014 Speech balloon stroke width + transparent fill handling

PR: https://github.com/ShareX/ShareX.Editor/pull/29

Summary:
- Ensures speech balloon uses stroke width and correctly handles transparent fill color during tool use.
- Captures UI snapshot and readiness marker for the integration branch.

Jaex source SHAs:
- cccfede (Handle transparent fill color for speech balloon tool)
- b0f4cf2 (Add stroke width support for SpeechBalloonControl)

Files touched (high level):
- src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-014.md
- integrate/jaex-JX-014.md

Tests:
- Not run (summary artifact only)

UI snapshots:
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-014.md
