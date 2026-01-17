# ShareX.Editor Fix Batches - Phase 3 Implementation Plan

**Date**: 2026-01-17
**Status**: Ready for Implementation
**Total Issues**: 27 remaining (31 total - 4 blockers fixed)
**Estimated Time**: 8-12 hours total

---

## Overview

This document organizes the 27 remaining issues (16 High, 11 Medium severity) into 6 implementation batches. Each batch groups related issues by fix complexity and category for efficient execution.

**All Blocker Issues (4/4) Already Fixed** ✅
- ISSUE-001: State sync validation ✅
- ISSUE-002: Memory leak - effect annotations ✅
- ISSUE-003: Canvas memento optimization ✅
- ISSUE-026: Use-after-free - CancelRotateCustomAngle ✅

---

## Batch 1: Quick Wins - Memory Management & Cleanup (Priority: HIGH)

**Time Estimate**: 1.5 hours
**Complexity**: Low
**Risk**: Low

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-024 | Missing disposal before bitmap reassignment | High | ~5 locations |
| ISSUE-030 | Clear() doesn't dispose undo/redo stacks | Medium | 1 method |
| ISSUE-019 | Dead code - PushUndo/ClearRedoStack | Medium | ~10 lines |

### Implementation Steps

#### 1.1 Fix ISSUE-024: Add Disposal Before Reassignment
**File**: `ViewModels/MainViewModel.cs`

**Locations to fix**:
```csharp
// Line 1313 - StartEffectPreview
_preEffectImage?.Dispose();
_preEffectImage = _currentSourceImage.Copy();

// Line 1466 - OpenRotateCustomAngleDialog
_rotateCustomAngleOriginalBitmap?.Dispose();
_rotateCustomAngleOriginalBitmap = current.Copy();

// Line 1036 - UpdatePreview (already has disposal, verify others)
_originalSourceImage?.Dispose();
_originalSourceImage = image.Copy();
```

**Validation**:
- Open effect dialog → cancel → open again → verify no leak via memory profiler
- Open rotate dialog → cancel → open again → verify no leak

---

#### 1.2 Fix ISSUE-030: Add Disposal to Clear() Command
**File**: `ViewModels/MainViewModel.cs` lines 811-823

**Current code**:
```csharp
[RelayCommand]
private void Clear()
{
    PreviewImage = null;
    _currentSourceImage = null;
    _originalSourceImage = null;
    ImageDimensions = "No image";
    StatusText = "Ready";
    ResetNumberCounter();
    ClearAnnotationsRequested?.Invoke(this, EventArgs.Empty);
}
```

**Fixed code**:
```csharp
[RelayCommand]
private void Clear()
{
    PreviewImage = null;

    _currentSourceImage?.Dispose();
    _currentSourceImage = null;

    _originalSourceImage?.Dispose();
    _originalSourceImage = null;

    // Dispose all bitmaps in undo/redo stacks
    while (_imageUndoStack.Count > 0)
    {
        _imageUndoStack.Pop()?.Dispose();
    }
    while (_imageRedoStack.Count > 0)
    {
        _imageRedoStack.Pop()?.Dispose();
    }

    ImageDimensions = "No image";
    StatusText = "Ready";
    ResetNumberCounter();
    ClearAnnotationsRequested?.Invoke(this, EventArgs.Empty);
}
```

**Validation**:
- Load image → crop 5 times → Clear → verify memory released

---

#### 1.3 Fix ISSUE-019: Remove Dead Code
**Files**:
- `Views/EditorView.axaml.cs` lines 185-193
- `Views/Controllers/EditorInputController.cs` lines 111, 292, 644

**Steps**:
1. Remove `PushUndo(Control shape)` method from EditorView
2. Remove `ClearRedoStack()` method from EditorView
3. Remove all call sites: `_view.PushUndo()` and `_view.ClearRedoStack()`

**Validation**:
- Build succeeds
- Undo/redo still works (handled by EditorCore)

---

### Batch 1 Testing Checklist
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Memory profiler shows no leaks when opening/closing dialogs
- [ ] Clear command releases all undo/redo memory
- [ ] Undo/redo functionality unchanged

### Commit Message Template
```
[ShareX.Editor] Batch 1: Memory management cleanup and dead code removal

- ISSUE-024: Add disposal before bitmap reassignment (5 locations)
- ISSUE-030: Clear() now disposes undo/redo stacks
- ISSUE-019: Remove dead PushUndo/ClearRedoStack methods

Prevents memory leaks in effect/rotate dialogs and Clear command

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Batch 2: Null Safety - SKBitmap.Copy() Checks (Priority: HIGH)

**Time Estimate**: 2-3 hours
**Complexity**: Medium (20+ locations)
**Risk**: Low

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-025 | No null check after SKBitmap.Copy() | High | ~20 locations |
| ISSUE-027 | Unclear UpdatePreview ownership semantics | High | Documentation only |
| ISSUE-004 | Missing null checks in EditorInputController | High | Multiple locations |
| ISSUE-012 | Missing null check in HandleTextTool closure | High | 1 location |

### Implementation Steps

#### 2.1 Fix ISSUE-025: Add Null Checks After Copy()
**File**: `ViewModels/MainViewModel.cs`

**Affected methods** (20 locations):
- Undo() - lines 733, 737
- Redo() - lines 758
- UpdatePreview() - line 1036
- CropImage() - line 1067
- CutOutImage() - line 1092
- Rotate90Clockwise/CounterClockwise/180 - lines 1128, 1142, 1156
- FlipHorizontal/Vertical - lines 1170, 1184
- AutoCropImage() - line 1205
- ResizeImage() - line 1226
- ResizeCanvas() - line 1245
- ApplyOneShotEffect() - line 1287
- StartEffectPreview() - line 1313
- ApplyEffect() - lines 1349, 1422
- OpenRotateCustomAngleDialog() - line 1466
- CommitRotateCustomAngle() - line 1502

**Pattern to apply**:
```csharp
// Before:
_imageUndoStack.Push(_currentSourceImage.Copy());

// After:
var copy = _currentSourceImage.Copy();
if (copy == null)
{
    StatusText = "Error: Out of memory - unable to save undo state";
    return; // or continue without undo
}
_imageUndoStack.Push(copy);
```

**Validation**:
- Test all image operations under low memory conditions
- Verify graceful degradation (operations continue, undo disabled)

---

#### 2.2 Fix ISSUE-027: Document UpdatePreview Ownership
**File**: `ViewModels/MainViewModel.cs` line 1028

**Add documentation**:
```csharp
/// <summary>
/// Updates the preview image. TAKES OWNERSHIP of the bitmap parameter.
/// The bitmap will be stored in _currentSourceImage and must NOT be disposed by the caller.
/// A copy will be made for _originalSourceImage backup.
/// </summary>
/// <param name="image">Image bitmap (ownership transferred to ViewModel)</param>
/// <param name="clearAnnotations">Whether to clear all annotations</param>
public void UpdatePreview(SkiaSharp.SKBitmap image, bool clearAnnotations = true)
```

**Also review all call sites** (20+ locations) to ensure correct ownership transfer.

**Validation**:
- Review all UpdatePreview() call sites
- Ensure no dispose after calling UpdatePreview (except where bitmap is explicitly discarded)

---

#### 2.3 Fix ISSUE-004: EditorInputController Null Checks
**File**: `Views/Controllers/EditorInputController.cs`

**Locations to fix**:
```csharp
// Pattern: Check vm after retrieval
var vm = ViewModel;
if (vm == null) return;

// Use throughout method instead of assuming vm is non-null
var strokeWidth = vm?.StrokeWidth ?? 4;
var color = vm?.SelectedColor ?? "#FF0000";
```

**Validation**:
- Trigger all tool creation paths
- Verify no crashes if ViewModel is null (edge case during unload)

---

#### 2.4 Fix ISSUE-012: HandleTextTool Closure Null Check
**File**: `Views/Controllers/EditorInputController.cs` line 708

**Fix closure**:
```csharp
OnCreationLostFocus = (s, args) =>
{
    // ... text validation ...

    if (_view?.EditorCore != null)
    {
        _view.EditorCore.AddAnnotation(annotation);
    }
};
```

**Validation**:
- Start typing text → close window mid-edit → verify no crash

---

### Batch 2 Testing Checklist
- [ ] Build succeeds
- [ ] All image operations work under low memory
- [ ] No null reference exceptions in any tool
- [ ] UpdatePreview ownership clear to all developers

### Commit Message Template
```
[ShareX.Editor] Batch 2: Null safety improvements

- ISSUE-025: Add null checks after SKBitmap.Copy() (20 locations)
- ISSUE-027: Document UpdatePreview ownership contract
- ISSUE-004: Add null checks in EditorInputController
- ISSUE-012: Fix HandleTextTool closure null safety

Prevents crashes under low memory and edge cases

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Batch 3: Duplication Refactoring (Priority: MEDIUM)

**Time Estimate**: 1.5 hours
**Complexity**: Low-Medium
**Risk**: Low

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-005 | Arrow geometry creation duplication | High | 3 locations |
| ISSUE-006 | Magic number "3" for arrow head size | Medium | 4 locations |
| ISSUE-013 | Polyline point translation duplication | Medium | 2 annotation types |
| ISSUE-028 | Duplicate ApplyEffect method overloads | Medium | 2 methods |

### Implementation Steps

#### 3.1 Fix ISSUE-005 & ISSUE-006: Arrow Geometry Refactoring
**Files**:
- `Views/Controllers/EditorInputController.cs` line 416
- `Views/Controllers/EditorSelectionController.cs` lines 325, 460
- `Annotations/ArrowAnnotation.cs` (add constant)

**Step 1**: Add constant to ArrowAnnotation
```csharp
namespace ShareX.Editor.Annotations;

public class ArrowAnnotation : Annotation
{
    /// <summary>
    /// Arrow head width is proportional to stroke width for visual balance
    /// </summary>
    public const double ArrowHeadWidthMultiplier = 3.0;

    // ... existing code ...
}
```

**Step 2**: Extract helper method in EditorSelectionController
```csharp
private void UpdateArrowGeometry(Path arrowPath, Point start, Point end, double strokeWidth)
{
    double arrowHeadWidth = strokeWidth * ArrowAnnotation.ArrowHeadWidthMultiplier;
    arrowPath.Data = new ArrowAnnotation().CreateArrowGeometry(start, end, arrowHeadWidth);
    _shapeEndpoints[arrowPath] = (start, end);
}
```

**Step 3**: Replace all 3 call sites with helper

**Validation**:
- Draw arrow → resize → move → verify geometry updates correctly
- Check arrow head size visually matches before/after

---

#### 3.2 Fix ISSUE-013: Polyline Point Translation
**Files**:
- Create `Annotations/IPointBasedAnnotation.cs` (new interface)
- Modify `Annotations/FreehandAnnotation.cs`
- Modify `Annotations/SmartEraserAnnotation.cs`
- Modify `Views/Controllers/EditorSelectionController.cs` lines 492-523

**Step 1**: Create interface
```csharp
namespace ShareX.Editor.Annotations;

/// <summary>
/// Interface for annotations that store a collection of points (freehand, polyline, etc.)
/// </summary>
public interface IPointBasedAnnotation
{
    List<SKPoint> Points { get; }
}
```

**Step 2**: Implement in annotations
```csharp
public class FreehandAnnotation : Annotation, IPointBasedAnnotation
{
    public List<SKPoint> Points { get; set; } = new();
    // ...
}

public class SmartEraserAnnotation : Annotation, IPointBasedAnnotation
{
    public List<SKPoint> Points { get; set; } = new();
    // ...
}
```

**Step 3**: Simplify translation logic
```csharp
if (polyline.Tag is IPointBasedAnnotation pointBased)
{
    for (int i = 0; i < pointBased.Points.Count; i++)
    {
        var oldPt = pointBased.Points[i];
        pointBased.Points[i] = new SKPoint(oldPt.X + (float)deltaX, oldPt.Y + (float)deltaY);
    }
    polyline.Points = pointBased.Points.Select(p => new Point(p.X, p.Y)).ToList();
}
```

**Validation**:
- Draw freehand → move → verify points translated
- Draw smart eraser → move → verify points translated

---

#### 3.3 Fix ISSUE-028: Extract Duplicate ApplyEffect Logic
**File**: `ViewModels/MainViewModel.cs` lines 1339-1442

**Extract common logic**:
```csharp
private void CommitEffectAndCleanup(SKBitmap result, string statusMessage)
{
    if (_currentSourceImage == null) return;

    _imageUndoStack.Push(_currentSourceImage.Copy());
    _imageRedoStack.Clear();

    UpdatePreview(result, clearAnnotations: true);
    UpdateUndoRedoProperties();
    StatusText = statusMessage;

    _preEffectImage?.Dispose();
    _preEffectImage = null;

    _isPreviewingEffect = false;
    OnPropertyChanged(nameof(AreBackgroundEffectsActive));
    UpdateCanvasProperties();
    ApplySmartPaddingCrop();
}
```

**Update both overloads** to call this helper.

**Validation**:
- Apply brightness → verify effect commits correctly
- Apply custom rotate → verify effect commits correctly

---

### Batch 3 Testing Checklist
- [ ] Build succeeds
- [ ] Arrow geometry creation works (draw, resize, move)
- [ ] Polyline translation works (freehand, smart eraser)
- [ ] Effect application works (brightness, rotate)
- [ ] No visual regressions

### Commit Message Template
```
[ShareX.Editor] Batch 3: Refactor duplication

- ISSUE-005/006: Extract arrow geometry helper + add constant
- ISSUE-013: Create IPointBasedAnnotation interface
- ISSUE-028: Extract CommitEffectAndCleanup helper

Reduces code duplication, improves maintainability

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Batch 4: Memory Management - Advanced (Priority: MEDIUM)

**Time Estimate**: 2-3 hours
**Complexity**: Medium-High
**Risk**: Medium

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-031 | Undo/Redo leaks old currentSourceImage | Medium | UpdatePreview method |
| ISSUE-029 | PreviewEffect bitmap disposal (verify) | Medium | 1 location + verification |
| ISSUE-023 | Missing disposal - _currentSourceImage fields | Medium | Multiple locations |

### Implementation Steps

#### 4.1 Fix ISSUE-031: UpdatePreview Disposal
**File**: `ViewModels/MainViewModel.cs` lines 1028-1052

**Modify UpdatePreview to dispose old image**:
```csharp
public void UpdatePreview(SkiaSharp.SKBitmap image, bool clearAnnotations = true)
{
    // Dispose old image before replacing (if different object)
    if (_currentSourceImage != null && _currentSourceImage != image)
    {
        _currentSourceImage.Dispose();
    }
    _currentSourceImage = image;

    // Update original backup
    if (!_isApplyingSmartPadding)
    {
        _originalSourceImage?.Dispose();
        _originalSourceImage = image.Copy();
    }

    // ... rest of method ...
}
```

**Validation**:
- Load image → crop → undo → redo → verify no leaks
- Memory profiler during undo/redo cycles

---

#### 4.2 Fix ISSUE-029: Verify BitmapConversionHelpers.ToAvaloniBitmap
**File**: `Helpers/BitmapConversionHelpers.cs` (read-only review)

**Steps**:
1. Read BitmapConversionHelpers.ToAvaloniBitmap implementation
2. Determine if it creates a copy or wraps the SKBitmap
3. If it creates a copy: Disposal at line 1407 is correct
4. If it wraps: Remove disposal or defer until after rendering

**File**: `ViewModels/MainViewModel.cs` line 1407

**Validation**:
- Apply brightness with rapid slider movement
- Check for visual corruption (indicates use-after-free)
- Memory profiler to verify bitmaps released

---

#### 4.3 Fix ISSUE-023: Dispose _currentSourceImage Fields
**File**: `ViewModels/MainViewModel.cs` - review all assignments

**Locations where _currentSourceImage is reassigned**:
- Line 814: Clear() - add disposal ✓ (fixed in Batch 1)
- Line 1031: UpdatePreview() - add disposal ✓ (fixed in 4.1)
- Line 480, 506: ApplySmartPaddingCrop() - verify disposal

**Review and add missing disposals as needed.**

**Validation**:
- Various image operations → verify no leaks

---

### Batch 4 Testing Checklist
- [ ] Build succeeds
- [ ] Memory profiler shows no leaks during undo/redo
- [ ] Effect preview doesn't cause visual corruption
- [ ] Image field assignments don't leak

### Commit Message Template
```
[ShareX.Editor] Batch 4: Advanced memory management fixes

- ISSUE-031: UpdatePreview now disposes old currentSourceImage
- ISSUE-029: Verify ToAvaloniBitmap ownership (disposal correct)
- ISSUE-023: Add disposal for all currentSourceImage reassignments

Prevents memory leaks in undo/redo and image operations

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Batch 5: UX Improvements (Priority: MEDIUM)

**Time Estimate**: 2-3 hours
**Complexity**: Medium
**Risk**: Low-Medium

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-010 | Selection state not persisted in mementos | High | EditorHistory + EditorMemento |
| ISSUE-014 | No visual feedback for crop/cutout direction | Medium | EditorInputController |
| ISSUE-018 | No cursor feedback for drawing tools | Medium | EditorView |

### Implementation Steps

#### 5.1 Fix ISSUE-010: Persist Selection in Mementos
**Files**:
- `EditorMemento.cs` - add SelectedAnnotationId field
- `EditorHistory.cs` - save/restore selection
- `EditorCore.cs` - expose selected annotation ID

**Step 1**: Modify EditorMemento
```csharp
public class EditorMemento
{
    public List<Annotation> Annotations { get; private set; }
    public SKSize CanvasSize { get; private set; }
    public SKBitmap? Canvas { get; private set; }
    public Guid? SelectedAnnotationId { get; private set; } // NEW

    public EditorMemento(..., Guid? selectedAnnotationId = null)
    {
        // ...
        SelectedAnnotationId = selectedAnnotationId;
    }
}
```

**Step 2**: Update EditorHistory snapshot methods
```csharp
private EditorMemento GetMementoFromAnnotations(Annotation? excludeAnnotation = null)
{
    var selectedId = _editorCore.SelectedAnnotation?.Id;
    return new EditorMemento(annotations, canvasSize, null, selectedId);
}
```

**Step 3**: Restore selection in EditorCore.RestoreState
```csharp
public void RestoreState(EditorMemento memento)
{
    // ... existing restore logic ...

    // Restore selection if memento has it
    if (memento.SelectedAnnotationId.HasValue)
    {
        _selectedAnnotation = _annotations.FirstOrDefault(a => a.Id == memento.SelectedAnnotationId.Value);
    }
    else
    {
        _selectedAnnotation = null;
    }

    // ... fire events ...
}
```

**Validation**:
- Draw A → Draw B (B selected) → Undo → Verify A is re-selected
- Draw multiple → Undo/Redo → Verify selection state matches

---

#### 5.2 Fix ISSUE-014: Crop/CutOut Direction Feedback
**File**: `Views/Controllers/EditorInputController.cs` lines 324-378

**Add immediate visual feedback**:
```csharp
// Show overlay immediately with hint text
_currentShape.IsVisible = true;
_currentShape.Opacity = 0.3; // Semi-transparent until direction determined

// Add text hint (requires TextBlock in overlay)
var hintText = _view.FindControl<TextBlock>("CutOutHintText");
if (hintText != null)
{
    hintText.Text = "Drag to choose cut direction";
    hintText.IsVisible = true;
}

// Once direction determined (after threshold):
_currentShape.Opacity = 1.0;
if (hintText != null) hintText.IsVisible = false;
```

**Requires**: Add TextBlock to CropOverlay XAML

**Validation**:
- Start cutout → drag 5px → verify hint visible
- Continue past 15px → verify direction locks in

---

#### 5.3 Fix ISSUE-018: Tool Cursor Feedback
**File**: `Views/EditorView.axaml.cs`

**Subscribe to ViewModel.ActiveTool changes**:
```csharp
partial void OnViewModelPropertyChanged(string? propertyName)
{
    if (propertyName == nameof(MainViewModel.ActiveTool))
    {
        UpdateCursorForTool();
    }
}

private void UpdateCursorForTool()
{
    var canvas = this.FindControl<Canvas>("AnnotationCanvas");
    if (canvas == null || DataContext is not MainViewModel vm) return;

    canvas.Cursor = vm.ActiveTool switch
    {
        EditorTool.Select => new Cursor(StandardCursorType.Arrow),
        EditorTool.Crop or EditorTool.CutOut => new Cursor(StandardCursorType.Cross),
        _ => new Cursor(StandardCursorType.Cross) // Drawing tools
    };
}
```

**Validation**:
- Select each tool → verify cursor changes appropriately

---

### Batch 5 Testing Checklist
- [ ] Build succeeds
- [ ] Selection restored correctly after undo/redo
- [ ] CutOut shows immediate visual feedback
- [ ] Cursor changes for each tool

### Commit Message Template
```
[ShareX.Editor] Batch 5: UX improvements

- ISSUE-010: Persist selection state in undo/redo mementos
- ISSUE-014: Add immediate visual feedback for crop/cutout direction
- ISSUE-018: Add cursor feedback for drawing tools

Improves user experience and workflow consistency

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Batch 6: Performance & Design (Priority: LOW)

**Time Estimate**: 3-4 hours
**Complexity**: High
**Risk**: Medium-High

### Issues in This Batch

| ID | Title | Severity | Lines Affected |
|----|-------|----------|----------------|
| ISSUE-021 | Smart padding pixel-by-pixel scan | Medium | ApplySmartPaddingCrop method |
| ISSUE-022 | Recursive guard flag _isApplyingSmartPadding | Medium | Design refactor |
| ISSUE-007 | Threading - UI dispatch inconsistency | High | EditorCore + subscribers |
| ISSUE-008 | DPI scaling inconsistency | High | Effect annotations |

### Implementation Steps

#### 6.1 Fix ISSUE-021: Optimize Smart Padding Algorithm
**File**: `ViewModels/MainViewModel.cs` lines 455-473

**Option A**: Sample every Nth pixel (fast approximation)
```csharp
const int sampleStep = 4; // Sample every 4th pixel
for (int y = 0; y < skBitmap.Height; y += sampleStep)
{
    for (int x = 0; x < skBitmap.Width; x += sampleStep)
    {
        var pixel = skBitmap.GetPixel(x, y);
        // ... comparison ...
    }
}
```

**Option B**: Use Task.Run for async execution
```csharp
await Task.Run(() =>
{
    // Existing pixel scan logic
});
```

**Option C**: Use unsafe pointer access (fastest)
```csharp
unsafe
{
    var pixels = (uint*)skBitmap.GetPixels().ToPointer();
    int width = skBitmap.Width;
    // ... fast pointer iteration ...
}
```

**Recommended**: Option A or B (safer than C)

**Validation**:
- Load 4K image → enable smart padding → measure time
- Compare before/after performance

---

#### 6.2 Fix ISSUE-022: Refactor Smart Padding Event Chain
**File**: `ViewModels/MainViewModel.cs`

**Separate concerns**:
- UpdatePreview should NOT trigger smart padding automatically
- Smart padding should be explicitly called only when settings change

**Requires careful refactoring of property change handlers.**

**Risk**: High - could affect other features

**Validation**:
- Full regression test of all image operations
- Toggle smart padding → verify applies correctly

---

#### 6.3 Fix ISSUE-007: Document Threading Contract
**File**: `EditorCore.cs`

**Add documentation**:
```csharp
/// <summary>
/// Core editor logic (platform-agnostic).
/// THREADING: All events are fired on the calling thread and may not be the UI thread.
/// Subscribers must dispatch to UI thread if needed (e.g., Dispatcher.UIThread.Post).
/// </summary>
public class EditorCore
```

**Review all event subscribers** to ensure proper dispatching.

**Validation**:
- Trigger EditorCore events from background thread
- Verify no crashes

---

#### 6.4 Fix ISSUE-008: DPI Scaling for Effect Annotations
**File**: `Views/Controllers/EditorInputController.cs` lines 537-538

**Apply RenderScaling to effect annotation bounds**:
```csharp
var scaling = 1.0;
var topLevel = TopLevel.GetTopLevel(_view);
if (topLevel != null) scaling = topLevel.RenderScaling;

annotation.StartPoint = new SKPoint((float)(x * scaling), (float)(y * scaling));
annotation.EndPoint = new SKPoint((float)((x + width) * scaling), (float)((y + height) * scaling));
```

**Validation**:
- Set Windows DPI to 150%
- Draw blur annotation
- Verify blur region matches selection box

---

### Batch 6 Testing Checklist
- [ ] Build succeeds
- [ ] Smart padding performance improved
- [ ] No event chain regressions
- [ ] Threading documented and verified
- [ ] DPI scaling correct on high-DPI displays

### Commit Message Template
```
[ShareX.Editor] Batch 6: Performance and DPI fixes

- ISSUE-021: Optimize smart padding (sample every Nth pixel)
- ISSUE-022: Document smart padding event chain
- ISSUE-007: Document EditorCore threading contract
- ISSUE-008: Fix DPI scaling for effect annotations

Improves performance and high-DPI display correctness

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Deferred / Out of Scope

These issues are marked as verification-needed or false alarms:

| ID | Title | Status |
|----|-------|--------|
| ISSUE-009 | Effect bitmap update timing | Needs verification - may be false alarm |
| ISSUE-015 | Hover outline recreation | False alarm - code is correct |
| ISSUE-016 | Hard-coded XAML control names | Low priority - cosmetic |
| ISSUE-017 | Crop region validation | Low priority - edge case |
| ISSUE-020 | Pattern matching usage | False alarm - code is correct |
| ISSUE-011 | _cutOutDirection state leak | Low priority - cleanup exists |

---

## Summary & Execution Plan

### Recommended Order
1. **Batch 1** (1.5h) - Quick wins, immediate value
2. **Batch 2** (2-3h) - Null safety, critical correctness
3. **Batch 3** (1.5h) - Duplication, improves maintainability
4. **Batch 4** (2-3h) - Memory management, prevents leaks
5. **Batch 5** (2-3h) - UX improvements, user-facing value
6. **Batch 6** (3-4h) - Performance & DPI, platform polish

**Total Estimated Time**: 12-17 hours

### Success Criteria
- [ ] All 27 issues addressed or explicitly deferred
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Memory profiler shows no leaks
- [ ] All manual tests pass
- [ ] No functional regressions

---

**Document Version**: 1.0
**Ready for Implementation**: Yes
**Next Step**: Start Batch 1
