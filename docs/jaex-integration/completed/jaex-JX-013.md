# JX-013 EditorOptions configuration + color alpha

PR: https://github.com/ShareX/ShareX.Editor/pull/28

Summary:
- Introduces EditorOptions and sync between EditorViewModel/MainViewModel, preserving alpha in color selection.
- Adds step border/fill options and fixes swapped step color assignments.
- Captures UI snapshot and readiness marker for the integration branch.

Jaex source SHAs:
- 5474a7a (Add EditorOptions class for editor configuration)
- fc50445 (Preserve alpha in color selection and add editor options)
- 134203d (Add step border and fill color options to EditorOptions)
- 449556d (Sync EditorViewModel and MainViewModel with EditorOptions)
- fb7c7c9 (Fix swapped step fill and border color assignments)

Files touched (high level):
- src/ShareX.Editor/EditorOptions.cs
- src/ShareX.Editor/ViewModels/EditorViewModel.cs
- src/ShareX.Editor/ViewModels/MainViewModel.cs
- src/ShareX.Editor/Annotations/NumberAnnotation.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-013.md
- integrate/jaex-JX-013.md

Tests:
- Not run (summary artifact only)

UI snapshots:
- docs/jaex-integration/ui-snapshots/ui_snapshot_after_JX-013.md
