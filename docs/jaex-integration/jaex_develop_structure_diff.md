# jaex vs develop structural diff (src)

## Scope
- jaex source: C:\Users\Public\source\repos\ShareX Team\ShareX.Editor\src
- develop source: C:\Users\liveu\source\repos\ShareX Team\ShareX.Editor\src
- Excludes bin/ and obj/ output folders

## Folder-level differences
- develop-only folder: ShareX.Editor.Verification (verification harness project)
- develop-only files in existing folders:
  - ShareX.Editor/Helpers/ImageEffectPresetSerializer.cs
  - ShareX.Editor/Helpers/LegacyImageEffectImporter.cs
  - ShareX.Editor/Helpers/LegacyImageEffectExporter.cs
  - ShareX.Editor/Views/Dialogs/MessageDialog.axaml
  - ShareX.Editor/Views/Dialogs/MessageDialog.axaml.cs
- jaex-only folders or files: none detected

## Key architectural files and responsibilities

### Menu construction
- develop: EffectsMenuDropdown.axaml includes Import Preset and Export Preset items and icons; EditorView.axaml wires preset events to the view.
- jaex: EffectsMenuDropdown.axaml does not include preset items; EditorView.axaml does not wire preset events.
- develop: EditorView.axaml attaches KeyDown handler on the view.
- jaex: EditorView.axaml does not attach KeyDown; EditorView.axaml.cs uses TopLevel key handling.

### Command registration
- develop: EditorView.axaml.cs routes most menu actions to MainViewModel relay commands (rotate/flip, invert, black and white, polaroid) and effect preview methods on MainViewModel.
- jaex: EditorView.axaml.cs calls EditorCore directly for rotate/flip and effect operations (SetPreviewEffect, ClearPreviewEffect, AddEffect).

### Editor state management
- develop: EditorCore owns image effects pipeline, preview effect, effects cache, and effect list mutation. EditorCore also owns rotate/flip operations.
- jaex: EditorCore focuses on annotations and source image only; effect pipeline and rotate/flip are handled in MainViewModel.

### Undo/redo implementation
- develop: EditorHistory and EditorMemento store effects in history; MainViewModel includes image and effects stacks and prioritizes core undo/redo before image-level undo/redo.
- jaex: EditorHistory and EditorMemento do not track effects; MainViewModel uses image undo/redo stacks before delegating to annotation undo/redo.

### View and ViewModel boundaries
- develop: EditorView wires to MainViewModel commands/events on load and uses view-level key handling; snapshot uses RenderTargetBitmap of the UI container.
- jaex: EditorView uses attach/detach on DataContext changes and TopLevel key handling; snapshot uses EditorCore.GetSnapshot for rendering.

## Areas with heavy divergence risk during merge
- src/ShareX.Editor/EditorCore.cs (effects pipeline, rotate/flip ownership)
- src/ShareX.Editor/EditorHistory.cs and src/ShareX.Editor/EditorMemento.cs (effects captured in mementos)
- src/ShareX.Editor/ViewModels/MainViewModel.cs (undo priority, effects stacks, preset import/export logic)
- src/ShareX.Editor/Views/EditorView.axaml and src/ShareX.Editor/Views/EditorView.axaml.cs (menu wiring, key handling, snapshot logic)
- src/ShareX.Editor/Controls/EffectsMenuDropdown.axaml and src/ShareX.Editor/Controls/EffectsMenuDropdown.axaml.cs (preset menu items and events)
- src/ShareX.Editor/Helpers/*Preset*.cs and src/ShareX.Editor/Views/Dialogs/MessageDialog.axaml* (develop-only features)
- src/ShareX.Editor.Verification (project-level divergence)
