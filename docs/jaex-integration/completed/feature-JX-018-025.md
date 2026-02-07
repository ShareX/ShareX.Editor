# Feature Batch JX-018..JX-025 (Manual Integration)

- PR link: Pending
- Feature name: Manual integration batch for JX-018 through JX-025

## Summary
This batch manually integrates approved logic enhancements from `jaex/main` into current `develop` architecture while preserving protected UI and existing behavior. No wholesale cleanup/rename churn was applied, and no files were deleted.

## Jaex SHAs referenced
- JX-018: `4e1fc9c`, `3108634`
- JX-019: `1bbbe31`, `6b4b2f4`
- JX-020: `d2ce180`, `1a416fe`, `b3ad48b` (logic-only subset)
- JX-021: `0e381bc`
- JX-022: `689f910`
- JX-023: `1218665` (manual local mapping)
- JX-024: `fb027d7`
- JX-025: `a775aa1`, `1e442dc`, `28ae339`, `a1904b8` (no-op for cleanup/rename churn by constraint)

## Files touched (high-level)
- `docs/jaex-integration/jaex_feature_candidates.md`
- `XerahS.Editor.sln`
- `src/XerahS.Editor.Loader/App.axaml.cs`
- `src/XerahS.Editor.Loader/MainWindow.axaml`
- `src/XerahS.Editor.Loader/ShareX.Editor.Loader.csproj`
- `src/XerahS.Editor/EditorHistory.cs`
- `src/XerahS.Editor/EditorOptions.cs`
- `src/XerahS.Editor/ViewModels/EditorViewModel.cs`
- `src/XerahS.Editor/ViewModels/MainViewModel.cs`
- `src/XerahS.Editor/Views/Controls/AnnotationToolbar.axaml`
- `src/XerahS.Editor/Views/EditorView.axaml`
- `src/XerahS.Editor/Views/EditorWindow.axaml.cs`
- `integrate/ready.md`

## Tests run
- `dotnet build XerahS.Editor.sln` (pass, 0 warnings, 0 errors)
- `dotnet test XerahS.Editor.sln` (pass)

## UI snapshots
- Base: `docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md`
- Post-batch: pending capture before PR finalization
