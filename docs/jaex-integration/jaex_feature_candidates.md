# jaex feature candidates

Generated from:
- Base marker: `3c43e83` (`docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt`)
- Reference branch: `jaex/main`
- Destination branch: `origin/develop`
- Comparison commands: `git log --oneline 3c43e83..jaex/main`, `git range-diff 3c43e83...jaex/main`, `git diff --name-only 3c43e83...jaex/main`

Skip-merged check:
- Verified every SHA below is missing from `origin/develop` via `git branch -r --contains <sha>`.
- Verified none are present in `docs/jaex-integration/completed/features_log.md`.

## JX-018 Crop workflow and preview preservation
- Feature ID: JX-018
- Feature name: Crop workflow and preview preservation
- User-visible outcome: Improves crop behavior/history updates and preserves annotations while preview refreshes.
- Primary areas touched: `EditorCore`, `EditorHistory`, `MainViewModel`, `EditorView.axaml.cs`, `EditorWindow.axaml`
- Commits:
  - `4e1fc9c` Improve crop handling and history management in editor
  - `3108634` Preserve annotations when updating preview
- Risk rating: Medium
- UI impact statement: Touches `.axaml` and `.axaml.cs` (`EditorWindow.axaml`, `EditorView.axaml.cs`). No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Cherry pick with conflicts

## JX-019 Window bootstrap refactor (EditorWindow as main)
- Feature ID: JX-019
- Feature name: Window bootstrap refactor (EditorWindow as main)
- User-visible outcome: Refactors startup flow to launch directly into `EditorWindow` and removes legacy app bootstrap files.
- Primary areas touched: loader `App.axaml.cs` / `MainWindow.*`, editor `EditorWindow.*`, app bootstrap files
- Commits:
  - `1bbbe31` Refactor to use EditorWindow as main window and clean up
  - `6b4b2f4` Remove App.axaml and App.axaml.cs from ShareX.Editor
- Risk rating: High
- UI impact statement: Touches `.axaml` startup/window files. No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Manual re-implementation

## JX-020 Tool naming and number/step behavior changes
- Feature ID: JX-020
- Feature name: Tool naming and number/step behavior changes
- User-visible outcome: Renames tool labels and changes number/step behavior, including a follow-up removal of the intermediate step tool state.
- Primary areas touched: `EditorTool`, `EditorOptions`, `EditorViewModel`, `MainViewModel`, `EditorInputController`, `EditorView.axaml`, `EditorView.axaml.cs`
- Commits:
  - `d2ce180` Rename Pen to Freehand and Number to Step in EditorTool
  - `1a416fe` Rename Highlighter tool to Highlight
  - `b3ad48b` Remove Step tool and improve Number annotation options
- Risk rating: High
- UI impact statement: Touches `.axaml` and `.axaml.cs` in editor tool UI. No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Manual re-implementation

## JX-021 MainViewModel sync loop guard
- Feature ID: JX-021
- Feature name: MainViewModel sync loop guard
- User-visible outcome: Prevents potential infinite synchronization loops in view-model state syncing.
- Primary areas touched: `MainViewModel`
- Commits:
  - `0e381bc` Prevent infinite loops during core sync in MainViewModel
- Risk rating: Low
- UI impact statement: No `.axaml` files touched. No menu/command edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Clean cherry pick

## JX-022 EditorOptions lifetime refactor (remove singleton)
- Feature ID: JX-022
- Feature name: EditorOptions lifetime refactor (remove singleton)
- User-visible outcome: Refactors editor options management away from singleton usage to instance-driven flow.
- Primary areas touched: `EditorOptions`, `EditorViewModel`, `MainViewModel`, loader `App.axaml`, `EditorWindow.axaml.cs`
- Commits:
  - `689f910` Refactor EditorOptions to remove singleton pattern
- Risk rating: Medium
- UI impact statement: Touches `.axaml` (loader app) and `.axaml.cs` code-behind. No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Cherry pick with conflicts

## JX-023 Toolbar button width normalization
- Feature ID: JX-023
- Feature name: Toolbar button width normalization
- User-visible outcome: Standardizes editor toolbar button widths for consistent layout.
- Primary areas touched: `EditorView.axaml`
- Commits:
  - `1218665` Set explicit width for editor toolbar buttons
- Risk rating: Low
- UI impact statement: Touches `.axaml` in editor toolbar only. No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Cherry pick with conflicts

## JX-024 Save/pin command path refactor
- Feature ID: JX-024
- Feature name: Save/pin command path refactor
- User-visible outcome: Refactors save and pin logic between editor view models and view bindings.
- Primary areas touched: `EditorViewModel`, `MainViewModel`, `EditorView.axaml`, `EditorView.axaml.cs`
- Commits:
  - `fb027d7` Refactor save and pin logic in editor view models
- Risk rating: Medium
- UI impact statement: Touches `.axaml` and `.axaml.cs` in editor view. No `EffectsMenuDropdown.axaml` menu edits detected; `Import Preset...` and `Export Preset...` remain unaffected.
- Recommended import method: Cherry pick with conflicts

## JX-025 Repository cleanup and rename-only churn
- Feature ID: JX-025
- Feature name: Repository cleanup and rename-only churn
- User-visible outcome: Removes local docs/settings noise and performs large project path rename from `ShareX.Editor` to `ShareX.ImageEditor`.
- Primary areas touched: docs cleanup, local tool settings, solution/project path rename, broad file path relocation
- Commits:
  - `a775aa1` Remove unused using directives and minor code cleanup
  - `1e442dc` Delete settings.local.json
  - `28ae339` Remove analysis and technical documentation files
  - `a1904b8` Rename ShareX.Editor to ShareX.ImageEditor
- Risk rating: High
- UI impact statement: Includes broad file moves and non-functional cleanup; no direct `EffectsMenuDropdown.axaml` menu modifications identified in source commits, but rename scope is very large. `Import Preset...` and `Export Preset...` should be treated as protected.
- Recommended import method: Reject as cherry-pick candidate; manually port only narrowly needed logic if any

| Feature ID | Name | Risk | UI touched | Approved (Y/N) | Notes |
| --- | --- | --- | --- | --- | --- |
| JX-018 | Crop workflow and preview preservation | Medium | Yes (.axaml/.axaml.cs) | Y | Approved with strict constraints: manual logic-only integration, no deletions |
| JX-019 | Window bootstrap refactor (EditorWindow as main) | High | Yes (.axaml/.axaml.cs) | Y | Approved with strict constraints: no file deletions; preserve current infra |
| JX-020 | Tool naming and number/step behavior changes | High | Yes (.axaml/.axaml.cs) | Y | Approved as enhancement-only; preserve existing behavior and protected UI |
| JX-021 | MainViewModel sync loop guard | Low | No | Y | Approved with manual implementation |
| JX-022 | EditorOptions lifetime refactor (remove singleton) | Medium | Yes (.axaml/.axaml.cs) | Y | Approved with manual implementation on current architecture |
| JX-023 | Toolbar button width normalization | Low | Yes (.axaml) | Y | Approved with protected UI/manual mapping |
| JX-024 | Save/pin command path refactor | Medium | Yes (.axaml/.axaml.cs) | Y | Approved as enhancement-only; keep current behavior intact |
| JX-025 | Repository cleanup and rename-only churn | High | Broad rename/churn | Y | Approved only for narrowly useful logic; no cleanup deletes/rename churn |
