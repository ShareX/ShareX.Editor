# develop isolation plan for protected behavior

## Goals
- Isolate preset import/export logic behind a dedicated service and a single menu wiring adapter.
- Isolate undo/redo history logic behind a narrow service API with no inline stack manipulation in MainViewModel or views.
- Keep changes minimal and behavior identical.

## Preset import/export isolation

### New classes and interfaces
- src/ShareX.Editor/Services/IEffectsPresetService.cs
  - API: Task SavePresetAsync(IReadOnlyList<ImageEffectBase> effects, string? presetName)
  - API: Task<IReadOnlyList<ImageEffectBase>?> LoadPresetAsync()
- src/ShareX.Editor/Services/EffectsPresetService.cs
  - Implements preset serialization, legacy import/export, and error reporting using existing Helpers.
- src/ShareX.Editor/Services/IEffectsPresetDialogService.cs
  - API: Task<string?> GetSavePathAsync()
  - API: Task<string?> GetLoadPathAsync()
- src/ShareX.Editor/Views/Adapters/EffectsPresetMenuAdapter.cs
  - Wires the EffectsMenuDropdown preset items to the view model commands through a single adapter.

### Menu wiring rule
- Only EffectsPresetMenuAdapter should attach to ImportPresetRequested and ExportPresetRequested.
- EditorView should not directly handle preset menu events after the adapter is introduced.

### Files to touch
- src/ShareX.Editor/Controls/EffectsMenuDropdown.axaml
- src/ShareX.Editor/Controls/EffectsMenuDropdown.axaml.cs
- src/ShareX.Editor/ViewModels/MainViewModel.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- src/ShareX.Editor/Helpers/ImageEffectPresetSerializer.cs
- src/ShareX.Editor/Helpers/LegacyImageEffectImporter.cs
- src/ShareX.Editor/Helpers/LegacyImageEffectExporter.cs
- src/ShareX.Editor/Views/Dialogs/MessageDialog.axaml
- src/ShareX.Editor/Views/Dialogs/MessageDialog.axaml.cs

## Undo/redo isolation

### New classes and interfaces
- src/ShareX.Editor/Services/IUndoRedoService.cs
  - API: bool CanUndo, bool CanRedo
  - API: void UpdateCoreState(bool canUndo, bool canRedo)
  - API: void PushImageSnapshot(SKBitmap image, IReadOnlyList<ImageEffectBase> effects)
  - API: void Undo()
  - API: void Redo()
  - API: event Action<bool, bool> StateChanged
- src/ShareX.Editor/Services/EditorUndoRedoService.cs
  - Owns image undo/redo stacks and effects stacks
  - Handles core-priority undo/redo behavior and restores effects in lockstep

### Integration approach
- MainViewModel delegates all undo/redo stack manipulation to IUndoRedoService.
- EditorView only triggers UndoRequested/RedoRequested through the service or view model, not by direct stack access.
- EditorCore remains the source for annotation history; the service only coordinates between core and image stacks.

### Files to touch
- src/ShareX.Editor/ViewModels/MainViewModel.cs
- src/ShareX.Editor/Views/EditorView.axaml.cs
- src/ShareX.Editor/Views/Controls/EditorCanvas.cs (if it triggers undo directly)
- src/ShareX.Editor/Services/IUndoRedoService.cs
- src/ShareX.Editor/Services/EditorUndoRedoService.cs

## Files explicitly not to touch
- src/ShareX.Editor/EditorCore.cs (no behavior changes)
- src/ShareX.Editor/EditorHistory.cs (no behavior changes)
- src/ShareX.Editor/EditorMemento.cs (no behavior changes)
- src/ShareX.Editor/ImageEffects/** (no behavior changes)
- src/ShareX.Editor/Views/EditorView.axaml (layout unchanged except event wiring if required)
