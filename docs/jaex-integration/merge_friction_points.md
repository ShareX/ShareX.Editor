# Merge friction points (develop vs jaex)

## FP-01 Preset menu items in effects menu
- develop location: src/ShareX.Editor/Controls/EffectsMenuDropdown.axaml, src/ShareX.Editor/Views/EditorView.axaml
- jaex location: preset menu items and event wiring are absent
- conflict reason: UI elements and event wiring exist only in develop, so menu layout and event handlers will conflict when merging UI files
- type: UI, structural

## FP-02 Preset import/export workflow
- develop location: src/ShareX.Editor/ViewModels/MainViewModel.cs (SavePreset/LoadPreset), src/ShareX.Editor/Helpers/ImageEffectPresetSerializer.cs, src/ShareX.Editor/Helpers/LegacyImageEffectImporter.cs, src/ShareX.Editor/Helpers/LegacyImageEffectExporter.cs, src/ShareX.Editor/Views/Dialogs/MessageDialog.axaml*
- jaex location: no preset logic or dialog classes
- conflict reason: develop adds logic and dependencies that do not exist in jaex, so merges will either drop functionality or require manual reapplication
- type: behavioral, structural

## FP-03 Effects pipeline ownership
- develop location: src/ShareX.Editor/EditorCore.cs (effects list, preview effect, effects cache, effects events)
- jaex location: EditorCore.cs has no effects pipeline
- conflict reason: large removal/addition blocks in EditorCore create high-conflict regions and change where effects are owned
- type: structural, behavioral

## FP-04 Undo/redo behavior and effects history
- develop location: src/ShareX.Editor/EditorHistory.cs, src/ShareX.Editor/EditorMemento.cs, src/ShareX.Editor/ViewModels/MainViewModel.cs
- jaex location: history and mementos omit effects; MainViewModel uses image stacks first
- conflict reason: develop stores effects in history and prioritizes core undo, while jaex does not, causing merge conflicts and behavioral divergence
- type: behavioral, structural

## FP-05 Snapshot rendering path
- develop location: src/ShareX.Editor/Views/EditorView.axaml.cs (GetSnapshot uses RenderTargetBitmap of UI container)
- jaex location: GetSnapshot delegates to EditorCore.GetSnapshot
- conflict reason: different rendering pipelines and responsibilities; merges can regress selection handling or output fidelity
- type: behavioral

## FP-06 Key handling and redo shortcut
- develop location: src/ShareX.Editor/Views/EditorView.axaml (KeyDown on view), src/ShareX.Editor/Views/EditorView.axaml.cs (Ctrl+Shift+Z redo)
- jaex location: TopLevel key handling without view-level KeyDown
- conflict reason: different event wiring and shortcut behaviors produce conflicts in view code
- type: UI, behavioral

## FP-07 Rotate/flip ownership
- develop location: src/ShareX.Editor/EditorCore.cs (PerformRotate/PerformFlip), src/ShareX.Editor/Views/EditorView.axaml.cs (delegates to VM commands)
- jaex location: rotation/flip handled in MainViewModel; EditorCore lacks rotate/flip methods
- conflict reason: mismatched ownership leads to method absence on one side and incompatible call sites
- type: structural, behavioral

## FP-08 Effects menu handlers routing
- develop location: src/ShareX.Editor/Views/EditorView.axaml.cs (invert/black and white/polaroid route to MainViewModel)
- jaex location: handlers call EditorCore directly
- conflict reason: call sites and logic paths differ, producing conflicts in EditorView and effect-preview flow
- type: behavioral, structural

## FP-09 Extra verification project
- develop location: src/ShareX.Editor.Verification
- jaex location: project absent
- conflict reason: solution and project references can conflict on merge and require manual reconciliation
- type: structural
