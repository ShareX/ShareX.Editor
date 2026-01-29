#nullable enable
#region License Information (GPL v3)

/*
    ShareX.Ava - The Avalonia UI implementation of ShareX
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout; // Added for HorizontalAlignment/VerticalAlignment
using Avalonia.Media;
using ShareX.Editor.Annotations;
using ShareX.Editor.Helpers;
using ShareX.Editor.ViewModels;
using ShareX.Editor.Controls;
using ShareX.Editor.Views.Controllers;
using SkiaSharp;
using System.ComponentModel;
using System.Linq; // Added for Enumerable.Select
using ShareX.Editor.Views.Dialogs;
using ShareX.Editor.ImageEffects;
using ShareX.Editor.ImageEffects.Adjustments;

namespace ShareX.Editor.Views
{
    public partial class EditorView : UserControl
    {
        private readonly EditorZoomController _zoomController;
        private readonly EditorSelectionController _selectionController;
        private readonly EditorInputController _inputController;
        private MainViewModel? _boundViewModel;
        private bool _suppressPreviewImageReload;

        internal EditorCore EditorCore => _editorCore;
        // SIP0018: Hybrid Rendering
        private SKCanvasControl? _canvasControl;
        private readonly EditorCore _editorCore;
        private TopLevel? _topLevel;

        public EditorView()
        {
            InitializeComponent();

            _editorCore = new EditorCore();

            _zoomController = new EditorZoomController(this);
            _selectionController = new EditorSelectionController(this);
            _inputController = new EditorInputController(this, _selectionController, _zoomController);

            // Subscribe to selection controller events
            _selectionController.RequestUpdateEffect += OnRequestUpdateEffect;
            _selectionController.SelectionChanged += OnSelectionChanged;

            // SIP0018: Subscribe to Core events
            _editorCore.InvalidateRequested += () => Avalonia.Threading.Dispatcher.UIThread.Post(RenderCore);
            _editorCore.ImageChanged += () => Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                if (_canvasControl != null)
                {
                    int w = (int)_editorCore.CanvasSize.Width;
                    int h = (int)_editorCore.CanvasSize.Height;
                    _canvasControl.Initialize(w, h);
                    RenderCore();
                    if (DataContext is MainViewModel vm)
                    {
                        vm.ImageWidth = w;
                        vm.ImageHeight = h;
                        vm.WindowTitle = $"ShareX - Image Editor - {w}x{h}";
                        SyncViewModelPreviewFromCore(clearAnnotations: false);
                        UpdateViewModelHistoryState(vm);
                    }
                }
            });
            _editorCore.AnnotationsRestored += () => Avalonia.Threading.Dispatcher.UIThread.Post(OnAnnotationsRestored);
            _editorCore.HistoryChanged += () => Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                if (DataContext is MainViewModel vm)
                {
                    UpdateViewModelHistoryState(vm);
                    vm.RecalculateNumberCounter(_editorCore.Annotations);
                }
            });

            // Capture wheel events in tunneling phase so ScrollViewer doesn't scroll when using Ctrl+wheel zoom.
            AddHandler(PointerWheelChangedEvent, OnPreviewPointerWheelChanged, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            DetachViewModel(_boundViewModel);
            AttachViewModel(DataContext as MainViewModel);
        }
        
        private void OnSelectionChanged(bool hasSelection)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.HasSelectedAnnotation = hasSelection;
            }
        }
        
        private void UpdateViewModelHistoryState(MainViewModel vm)
        {
            vm.UpdateCoreHistoryState(_editorCore.CanUndo, _editorCore.CanRedo);
        }

        private void SyncViewModelPreviewFromCore(bool clearAnnotations)
        {
            if (DataContext is not MainViewModel vm) return;
            if (_editorCore.SourceImage == null) return;

            var copy = _editorCore.SourceImage.Copy();
            if (copy == null) return;

            _suppressPreviewImageReload = true;
            try
            {
                vm.UpdatePreview(copy, clearAnnotations);
            }
            finally
            {
                _suppressPreviewImageReload = false;
            }
        }
        

        private void UpdateViewModelMetadata(MainViewModel vm)
        {
            // Initial sync of metadata if needed
            UpdateViewModelHistoryState(vm);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _topLevel = TopLevel.GetTopLevel(this);
            if (_topLevel != null)
            {
                _topLevel.AddHandler(KeyDownEvent, OnTopLevelKeyDown, RoutingStrategies.Tunnel);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (_topLevel != null)
            {
                _topLevel.RemoveHandler(KeyDownEvent, OnTopLevelKeyDown);
                _topLevel = null;
            }
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            UpdateToolbarScrollPadding();
            UpdateSidebarScrollPadding();

            AttachViewModel(DataContext as MainViewModel);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            DetachViewModel(_boundViewModel);

            _selectionController.RequestUpdateEffect -= OnRequestUpdateEffect;
        }

        private void AttachViewModel(MainViewModel? vm)
        {
            if (vm == null || ReferenceEquals(_boundViewModel, vm))
            {
                return;
            }

            _boundViewModel = vm;
            vm.UndoRequested += (s, args) => PerformUndo();
            vm.RedoRequested += (s, args) => PerformRedo();
            vm.DeleteRequested += (s, args) => PerformDelete();
            vm.ClearAnnotationsRequested += (s, args) => ClearAllAnnotations();
            vm.SnapshotRequested += () =>
            {
                var skBitmap = GetSnapshot();
                var snapshot = skBitmap != null ? BitmapConversionHelpers.ToAvaloniBitmap(skBitmap) : null;
                return Task.FromResult<Avalonia.Media.Imaging.Bitmap?>(snapshot);
            };

            vm.PropertyChanged += OnViewModelPropertyChanged;

            _zoomController.InitLastZoom(vm.Zoom);

            vm.CopyRequested += OnCopyRequested;
            vm.SaveAsRequested += OnSaveAsRequested;
            vm.SavePresetRequested += OnSavePresetRequested;
            vm.LoadPresetRequested += OnLoadPresetRequested;
            vm.ShowErrorDialog += OnShowErrorDialog;
            vm.DeselectRequested += OnDeselectRequested;
            vm.OpenImageRequested += OnOpenImageRequested;
            vm.AddImageAnnotationRequested += OnAddImageAnnotationRequested;

            if (vm.PreviewImage != null)
            {
                LoadImageFromViewModel(vm);
            }
        }

        private void DetachViewModel(MainViewModel? vm)
        {
            if (vm == null)
            {
                _boundViewModel = null;
                return;
            }

            vm.PropertyChanged -= OnViewModelPropertyChanged;
            vm.CopyRequested -= OnCopyRequested;
            vm.SaveAsRequested -= OnSaveAsRequested;
            vm.SavePresetRequested -= OnSavePresetRequested;
            vm.LoadPresetRequested -= OnLoadPresetRequested;
            vm.ShowErrorDialog -= OnShowErrorDialog;
            vm.DeselectRequested -= OnDeselectRequested;
            vm.OpenImageRequested -= OnOpenImageRequested;
            vm.AddImageAnnotationRequested -= OnAddImageAnnotationRequested;
            _boundViewModel = null;
        }
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is MainViewModel vm)
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedColor))
                {
                    ApplySelectedColor(vm.SelectedColor);
                }
                else if (e.PropertyName == nameof(MainViewModel.StrokeWidth))
                {
                    ApplySelectedStrokeWidth(vm.StrokeWidth);
                }
                else if (e.PropertyName == nameof(MainViewModel.PreviewImage))
                {
                    if (_suppressPreviewImageReload)
                    {
                        return;
                    }
                    _zoomController.ResetScrollViewerOffset();
                    // During smart padding, use UpdateSourceImage to preserve history and annotations
                    if (vm.IsSmartPaddingInProgress)
                    {
                        UpdateSourceImageFromViewModel(vm);
                    }
                    else
                    {
                        LoadImageFromViewModel(vm);
                    }
                }
                else if (e.PropertyName == nameof(MainViewModel.Zoom))
                {
                    _zoomController.HandleZoomPropertyChanged(vm);
                }
                else if (e.PropertyName == nameof(MainViewModel.ActiveTool))
                {
                    _selectionController.ClearSelection();
                    UpdateCursorForTool(); // ISSUE-018 fix: Update cursor feedback for active tool
                }
            }
        }

        /// <summary>
        /// ISSUE-018 fix: Updates the canvas cursor based on the active tool
        /// </summary>
        private void UpdateCursorForTool()
        {
            var canvas = this.FindControl<Canvas>("AnnotationCanvas");
            if (canvas == null || DataContext is not MainViewModel vm) return;

            canvas.Cursor = vm.ActiveTool switch
            {
                EditorTool.Select => new Cursor(StandardCursorType.Arrow),
                EditorTool.Crop or EditorTool.CutOut => new Cursor(StandardCursorType.Cross),
                _ => new Cursor(StandardCursorType.Cross) // Drawing tools (Rectangle, Ellipse, Pen, etc.)
            };
        }

        // --- Public/Internal Methods for Controllers ---

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _canvasControl = this.FindControl<SKCanvasControl>("CanvasControl");
        }

        private void LoadImageFromViewModel(MainViewModel vm)
        {
            if (vm.PreviewImage == null || _canvasControl == null) return;

            // One-time conversion from Avalonia Bitmap to SKBitmap for the Core
            // In a full refactor, VM would hold SKBitmap source of truth
            using var skBitmap = BitmapConversionHelpers.ToSKBitmap(vm.PreviewImage);
            if (skBitmap != null)
            {
                // We must copy because ToSKBitmap might return a disposable wrapper or we need ownership
                _editorCore.LoadImage(skBitmap.Copy());
                
                _canvasControl.Initialize(skBitmap.Width, skBitmap.Height);
                RenderCore();
            }
        }

        /// <summary>
        /// Updates the source image in EditorCore without clearing history or annotations.
        /// Used during smart padding operations to preserve editing state.
        /// </summary>
        private void UpdateSourceImageFromViewModel(MainViewModel vm)
        {
            if (vm.PreviewImage == null || _canvasControl == null) return;

            using var skBitmap = BitmapConversionHelpers.ToSKBitmap(vm.PreviewImage);
            if (skBitmap != null)
            {
                _editorCore.UpdateSourceImage(skBitmap.Copy());
                _canvasControl.Initialize(skBitmap.Width, skBitmap.Height);
                RenderCore();
            }
        }

        private void RenderCore()
        {
            if (_canvasControl == null) return;
            // Hybrid rendering: Render only background + raster effects from Core
            // Vector annotations are handled by Avalonia Canvas
            _canvasControl.Draw(canvas => _editorCore.Render(canvas, false));
        }

        /// <summary>
        /// Sample pixel color from the rendered canvas (including annotations) at the specified canvas coordinates
        /// </summary>
        internal async System.Threading.Tasks.Task<string?> GetPixelColorFromRenderedCanvas(Point canvasPoint)
        {
            if (DataContext is not MainViewModel vm || vm.PreviewImage == null) return null;

            try
            {
                var container = this.FindControl<Grid>("CanvasContainer");
                if (container == null || container.Width <= 0 || container.Height <= 0) return null;

                var rtb = new global::Avalonia.Media.Imaging.RenderTargetBitmap(
                    new PixelSize((int)container.Width, (int)container.Height),
                    new Vector(96, 96));

                rtb.Render(container);

                using var skBitmap = BitmapConversionHelpers.ToSKBitmap(rtb);

                int x = (int)Math.Round(canvasPoint.X);
                int y = (int)Math.Round(canvasPoint.Y);

                if (x < 0 || y < 0 || x >= skBitmap.Width || y >= skBitmap.Height)
                    return null;

                var skColor = skBitmap.GetPixel(x, y);
                return $"#{skColor.Red:X2}{skColor.Green:X2}{skColor.Blue:X2}";
            }
            catch
            {
                return null;
            }
        }

        // --- Event Handlers Delegated to Controllers ---

        private void OnPreviewPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            _zoomController.OnPreviewPointerWheelChanged(sender, e);
        }

        private void OnScrollViewerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _zoomController.OnScrollViewerPointerPressed(sender, e);
        }

        private void OnScrollViewerPointerMoved(object? sender, PointerEventArgs e)
        {
            _zoomController.OnScrollViewerPointerMoved(sender, e);
        }

        private void OnScrollViewerPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _zoomController.OnScrollViewerPointerReleased(sender, e);
        }

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _inputController.OnCanvasPointerPressed(sender, e);
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            _inputController.OnCanvasPointerMoved(sender, e);
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _inputController.OnCanvasPointerReleased(sender, e);
        }

        private void OnTopLevelKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Source is TextBox) return;

            if (DataContext is MainViewModel vm)
            {
                if (e.Key == Key.Delete)
                {
                    vm.DeleteSelectedCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    if (e.Key == Key.Z)
                    {
                        PerformUndo();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Y)
                    {
                        PerformRedo();
                        e.Handled = true;
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.None)
                {
                    // Tool shortcuts
                    switch (e.Key)
                    {
                        case Key.V: vm.SelectToolCommand.Execute(EditorTool.Select); e.Handled = true; break;
                        case Key.R: vm.SelectToolCommand.Execute(EditorTool.Rectangle); e.Handled = true; break;
                        case Key.E: vm.SelectToolCommand.Execute(EditorTool.Ellipse); e.Handled = true; break;
                        case Key.A: vm.SelectToolCommand.Execute(EditorTool.Arrow); e.Handled = true; break;
                        case Key.L: vm.SelectToolCommand.Execute(EditorTool.Line); e.Handled = true; break;
                        case Key.T: vm.SelectToolCommand.Execute(EditorTool.Text); e.Handled = true; break;
                        case Key.S: vm.SelectToolCommand.Execute(EditorTool.Spotlight); e.Handled = true; break;
                        case Key.B: vm.SelectToolCommand.Execute(EditorTool.Blur); e.Handled = true; break;
                        case Key.P: vm.SelectToolCommand.Execute(EditorTool.Pixelate); e.Handled = true; break;
                        case Key.I: vm.SelectToolCommand.Execute(EditorTool.Image); e.Handled = true; break;
                        case Key.F: vm.SelectToolCommand.Execute(EditorTool.Pen); e.Handled = true; break; // Freehand
                        case Key.H: vm.SelectToolCommand.Execute(EditorTool.Highlighter); e.Handled = true; break;
                        case Key.M: vm.SelectToolCommand.Execute(EditorTool.Magnify); e.Handled = true; break;
                        case Key.C: vm.SelectToolCommand.Execute(EditorTool.Crop); e.Handled = true; break;
                    }
                }
            }
        }

        // --- Private Helpers (Undo/Redo, Delete, etc that involve view state) ---

        private void PerformUndo()
        {
            if (_editorCore.CanUndo)
            {
                _editorCore.Undo();
                // AnnotationsRestored event will handle UI sync
            }
        }

        private void PerformRedo()
        {
            if (_editorCore.CanRedo)
            {
                _editorCore.Redo();
            }
        }

        private void OnDeselectRequested(object? sender, EventArgs e)
        {
            _selectionController.ClearSelection();
        }

        private void OnAnnotationsRestored()
        {
            // Fully rebuild annotation layer from Core state
            // 1. Clear current UI annotations
            var canvas = this.FindControl<Canvas>("AnnotationCanvas");
            if (canvas == null) return;

            // Dispose old annotations ONLY if they are no longer in the Core
            // This prevents disposing the bitmap of an annotation that persists across a refresh
            foreach (var child in canvas.Children)
            {
                if (child is Control control && control.Tag is Annotation annotation)
                {
                    if (!_editorCore.Annotations.Where(a => a.Id == annotation.Id).Any())
                    {
                        (annotation as IDisposable)?.Dispose();
                    }
                }
            }

            canvas.Children.Clear();
            _selectionController.ClearSelection();

            // 2. Re-create UI for all vector annotations in Core
            Control? selectedControl = null;
            foreach (var annotation in _editorCore.Annotations)
            {
                // Only create UI for vector annotations (Hybrid model)
                Control? shape = CreateControlForAnnotation(annotation);
                if (shape != null)
                {
                    canvas.Children.Add(shape);

                    // Track the control mapping for the selected annotation
                    if (_editorCore.SelectedAnnotation != null && annotation.Id == _editorCore.SelectedAnnotation.Id)
                    {
                        selectedControl = shape;
                    }
                }
            }

            // Restore selection in the controller
            if (selectedControl != null)
            {
                _selectionController.SetSelectedShape(selectedControl);
            }

            RenderCore();

            // 3. Validate state synchronization (ISSUE-001 mitigation)
            ValidateAnnotationSync();
            
            // Update HasAnnotations state
            UpdateHasAnnotationsState();
        }

        private Control? CreateControlForAnnotation(Annotation annotation)
        {
             // Factory for restoring vector visuals
             if (annotation is RectangleAnnotation rect) {
                var bounds = rect.GetBounds();
                var r = rect.CreateVisual();
                Canvas.SetLeft(r, bounds.Left);
                Canvas.SetTop(r, bounds.Top);
                r.Width = bounds.Width;
                r.Height = bounds.Height;
                return r;
             }
             else if (annotation is EllipseAnnotation ellipse) {
                var bounds = ellipse.GetBounds();
                var e = ellipse.CreateVisual();
                Canvas.SetLeft(e, bounds.Left);
                Canvas.SetTop(e, bounds.Top);
                e.Width = bounds.Width;
                e.Height = bounds.Height;
                return e;
             }
             else if (annotation is LineAnnotation line) {
                return line.CreateVisual();
             }
             else if (annotation is ArrowAnnotation arrow) {
                var path = (global::Avalonia.Controls.Shapes.Path)arrow.CreateVisual();
                // ISSUE-005/006 fix: Use constant for arrow head width
                path.Data = arrow.CreateArrowGeometry(new Point(arrow.StartPoint.X, arrow.StartPoint.Y), new Point(arrow.EndPoint.X, arrow.EndPoint.Y), arrow.StrokeWidth * ArrowAnnotation.ArrowHeadWidthMultiplier);
                return path;
             }
             else if (annotation is TextAnnotation text) {
                var bounds = text.GetBounds();
                var tb = (TextBox)text.CreateVisual();
                tb.IsHitTestVisible = false;
                tb.LostFocus += (s, e) => { if (s is TextBox t) t.IsHitTestVisible = false; };
                Canvas.SetLeft(tb, bounds.Left);
                Canvas.SetTop(tb, bounds.Top);
                tb.Width = bounds.Width;
                tb.Height = bounds.Height;
                return tb;

             }
             else if (annotation is SpotlightAnnotation spotlight) {
                var s = spotlight.CreateVisual();
                Canvas.SetLeft(s, 0);
                Canvas.SetTop(s, 0);
                s.Width = spotlight.CanvasSize.Width;
                s.Height = spotlight.CanvasSize.Height;
                return s;
             }
             // Effect annotations (Blur, Pixelate, Magnify, Highlight)
            else if (annotation is BaseEffectAnnotation effect) {
                 Control? factorControl = null;
                 
                 // Use CreateVisual() which properly sets up the Fill for each type
                 if (annotation is BlurAnnotation blur) factorControl = blur.CreateVisual();
                 else if (annotation is PixelateAnnotation pix) factorControl = pix.CreateVisual();
                 else if (annotation is MagnifyAnnotation mag) factorControl = mag.CreateVisual();
                 else if (annotation is HighlightAnnotation high) factorControl = high.CreateVisual();
                 
                 if (factorControl != null) {
                    Canvas.SetLeft(factorControl, effect.GetBounds().Left);
                    Canvas.SetTop(factorControl, effect.GetBounds().Top);
                    factorControl.Width = effect.GetBounds().Width;
                    factorControl.Height = effect.GetBounds().Height;

                    // Trigger visual update for all effect annotations
                    OnRequestUpdateEffect(factorControl);
                    return factorControl;
                }
             }
             else if (annotation is SpeechBalloonAnnotation balloon) {
                var bounds = balloon.GetBounds();
                var b = balloon.CreateVisual();
                Canvas.SetLeft(b, bounds.Left);
                Canvas.SetTop(b, bounds.Top);
                b.Width = bounds.Width;
                b.Height = bounds.Height;
                return b;
             }
             else if (annotation is NumberAnnotation number) {
                var bounds = number.GetBounds();
                var grid = number.CreateVisual();
                Canvas.SetLeft(grid, bounds.Left);
                Canvas.SetTop(grid, bounds.Top);
                grid.Width = bounds.Width;
                grid.Height = bounds.Height;
                return grid;
             }
             else if (annotation is ImageAnnotation imgAnn) {
                 var bounds = imgAnn.GetBounds();
                 var img = imgAnn.CreateVisual();
                 Canvas.SetLeft(img, bounds.Left);
                 Canvas.SetTop(img, bounds.Top);
                 if (bounds.Width > 0) img.Width = bounds.Width;
                 if (bounds.Height > 0) img.Height = bounds.Height;
                 return img;
             }
             else if (annotation is FreehandAnnotation freehand) {
                 return freehand.CreateVisual();
             }
             else if (annotation is SmartEraserAnnotation eraser) {
                 return eraser.CreateVisual();
             }
             
             return null; 
        }

        private Color SKColorToAvalonia(SKColor color)
        {
            return Color.FromUInt32((uint)color);
        }

        private void PerformDelete()
        {
            var selected = _selectionController.SelectedShape;
            if (selected != null)
            {
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                if (canvas != null && canvas.Children.Contains(selected))
                {
                    // Sync with EditorCore - this creates the undo history entry
                    if (selected.Tag is Annotation annotation)
                    {
                        // Select the annotation in core so DeleteSelected knows what to remove
                        _editorCore.Select(annotation);
                        _editorCore.DeleteSelected();
                    }
                    
                    // Dispose annotation resources before removing from view
                    (selected.Tag as IDisposable)?.Dispose();

                    canvas.Children.Remove(selected);

                    _selectionController.ClearSelection();
                    
                    // Update HasAnnotations state
                    UpdateHasAnnotationsState();
                }
            }
        }

        private void ClearAllAnnotations()
        {
            var canvas = this.FindControl<Canvas>("AnnotationCanvas");
            if (canvas != null)
            {
                canvas.Children.Clear();
                _selectionController.ClearSelection();
                _editorCore.ClearAll();
                RenderCore();
                
                // Update HasAnnotations state
                if (DataContext is MainViewModel vm)
                {
                    vm.HasAnnotations = false;
                }
            }
        }
        
        /// <summary>
        /// Updates the ViewModel's HasAnnotations property based on current annotation count.
        /// </summary>
        private void UpdateHasAnnotationsState()
        {
            if (DataContext is MainViewModel vm)
            {
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                int coreAnnotationCount = _editorCore.Annotations.Count;
                int canvasChildCount = canvas?.Children.Count ?? 0;
                vm.HasAnnotations = coreAnnotationCount > 0 || canvasChildCount > 0;
            }
        }

        public SkiaSharp.SKBitmap? GetSnapshot()
        {
            // Use the EditorCore's GetSnapshot which properly excludes selection handles
            return _editorCore.GetSnapshot();
        }

        // This is called by SelectionController/InputController via event when an effect logic needs update
        // We replicate the UpdateEffectVisual logic here or expose it
        private void OnRequestUpdateEffect(Control shape)
        {
            if (shape == null || shape.Tag is not BaseEffectAnnotation annotation) return;
            if (DataContext is not MainViewModel vm || vm.PreviewImage == null) return;

            // Logic to update effect bitmap
            try
            {
                double left = Canvas.GetLeft(shape);
                double top = Canvas.GetTop(shape);
                // Use explicit Width/Height first, fallback to Bounds, then annotation bounds
                double width = shape.Width;
                double height = shape.Height;
                if (double.IsNaN(width) || width <= 0) width = shape.Bounds.Width;
                if (double.IsNaN(height) || height <= 0) height = shape.Bounds.Height;
                // Final fallback to annotation's own bounds
                if (width <= 0 || height <= 0)
                {
                    var bounds = annotation.GetBounds();
                    width = bounds.Width;
                    height = bounds.Height;
                }
                if (width <= 0 || height <= 0) return;

                // Map to SKPoint
                annotation.StartPoint = new SKPoint((float)left, (float)top);
                annotation.EndPoint = new SKPoint((float)(left + width), (float)(top + height));

                // We don't have the cached bitmap here, create fresh or pass from controller?
                // Original logic cached it. InputController caches it.
                // This handler is for "OnPointerReleased" from SelectionController (dragging an existing effect).
                // SelectionController doesn't have the cached bitmap.
                using var skBitmap = BitmapConversionHelpers.ToSKBitmap(vm.PreviewImage);
                annotation.UpdateEffect(skBitmap);

                if (annotation.EffectBitmap != null && shape is Shape shapeControl)
                {
                    var avaloniaBitmap = BitmapConversionHelpers.ToAvaloniBitmap(annotation.EffectBitmap);
                    shapeControl.Fill = new ImageBrush(avaloniaBitmap)
                    {
                        Stretch = Stretch.None,
                        SourceRect = new RelativeRect(0, 0, width, height, RelativeUnit.Absolute)
                    };
                }
            }
            catch { }
        }

        private void OnColorChanged(object? sender, IBrush color)
        {
            if (DataContext is MainViewModel vm && color is SolidColorBrush solidBrush)
            {
                var hexColor = $"#{solidBrush.Color.A:X2}{solidBrush.Color.R:X2}{solidBrush.Color.G:X2}{solidBrush.Color.B:X2}";
                vm.SetColorCommand.Execute(hexColor);
            }
        }

        private void OnFillColorChanged(object? sender, IBrush color)
        {
            if (DataContext is MainViewModel vm && color is SolidColorBrush solidBrush)
            {
                var hexColor = $"#{solidBrush.Color.A:X2}{solidBrush.Color.R:X2}{solidBrush.Color.G:X2}{solidBrush.Color.B:X2}";
                vm.FillColor = hexColor;
                
                // Apply to selected annotation if any
                var selected = _selectionController.SelectedShape;
                if (selected?.Tag is Annotation annotation)
                {
                    annotation.FillColor = hexColor;
                    
                    // Update the UI control's Fill property
                    if (selected is Shape shape)
                    {
                        shape.Fill = hexColor == "#00000000" ? Brushes.Transparent : solidBrush;
                    }
                    else if (selected is Grid grid)
                    {
                        // For NumberAnnotation, update the Ellipse fill
                        foreach (var child in grid.Children)
                        {
                            if (child is Avalonia.Controls.Shapes.Ellipse ellipse)
                            {
                                ellipse.Fill = hexColor == "#00000000" ? Brushes.Transparent : solidBrush;
                            }
                        }
                    }
                }
            }
        }

        private void OnFontSizeChanged(object? sender, float fontSize)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.FontSize = fontSize;
                
                // Apply to selected annotation if any
                var selected = _selectionController.SelectedShape;
                if (selected?.Tag is TextAnnotation textAnn)
                {
                    textAnn.FontSize = fontSize;
                    if (selected is TextBox textBox)
                    {
                        textBox.FontSize = fontSize;
                    }
                }
                else if (selected?.Tag is NumberAnnotation numAnn)
                {
                    numAnn.FontSize = fontSize;
                    
                    // Update the visual - resize grid and update text
                    if (selected is Grid grid)
                    {
                        var radius = AnnotationGeometryHelper.CalculateNumberRadius(fontSize);
                        grid.Width = radius * 2;
                        grid.Height = radius * 2;
                        
                        foreach (var child in grid.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.FontSize = fontSize * 0.6; // Match CreateVisual scaling
                            }
                        }
                    }
                }
            }
        }

        private void OnStrengthChanged(object? sender, float strength)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.EffectStrength = strength;
                
                // Apply to selected annotation if any
                var selected = _selectionController.SelectedShape;
                if (selected?.Tag is BaseEffectAnnotation effectAnn)
                {
                    effectAnn.Amount = strength;
                    // Regenerate effect
                    OnRequestUpdateEffect(selected);
                }
            }
        }

        private void OnShadowButtonClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Toggle state
                vm.ShadowEnabled = !vm.ShadowEnabled;
                var isEnabled = vm.ShadowEnabled;
                
                // Apply to selected annotation if any
                var selected = _selectionController.SelectedShape;
                if (selected?.Tag is Annotation annotation)
                {
                    annotation.ShadowEnabled = isEnabled;
                    
                    // Update the UI control's Effect property
                    if (selected is Control control)
                    {
                        if (isEnabled)
                        {
                            control.Effect = new Avalonia.Media.DropShadowEffect
                            {
                                OffsetX = 3,
                                OffsetY = 3,
                                BlurRadius = 4,
                                Color = Avalonia.Media.Color.FromArgb(128, 0, 0, 0)
                            };
                        }
                        else
                        {
                            control.Effect = null;
                        }
                    }
                }
            }
        }
        public void PerformCrop()
        {
            var cropOverlay = this.FindControl<global::Avalonia.Controls.Shapes.Rectangle>("CropOverlay");
            if (cropOverlay != null && cropOverlay.IsVisible)
            {
                var rect = new SkiaSharp.SKRect(
                    (float)Canvas.GetLeft(cropOverlay),
                    (float)Canvas.GetTop(cropOverlay),
                    (float)(Canvas.GetLeft(cropOverlay) + cropOverlay.Width),
                    (float)(Canvas.GetTop(cropOverlay) + cropOverlay.Height));

                if (rect.Width > 0 && rect.Height > 0)
                {
                    var scaling = 1.0;
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null) scaling = topLevel.RenderScaling;

                    var physX = (int)(rect.Left * scaling);
                    var physY = (int)(rect.Top * scaling);
                    var physW = (int)(rect.Width * scaling);
                    var physH = (int)(rect.Height * scaling);

                    _editorCore.PerformCrop(physX, physY, physW, physH);
                }
                cropOverlay.IsVisible = false;
            }
        }

        private void OnWidthChanged(object? sender, int width)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SetStrokeWidthCommand.Execute(width);
            }
        }

        private void OnZoomChanged(object? sender, double zoom)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Zoom = zoom;
            }
        }

        private void OnSidebarScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateSidebarScrollPadding();
        }

        private void UpdateToolbarScrollPadding()
        {
            var toolbarScrollViewer = this.FindControl<ScrollViewer>("ToolbarScrollViewer");
            if (toolbarScrollViewer == null) return;

            var hasHorizontalOverflow = toolbarScrollViewer.Extent.Width - toolbarScrollViewer.Viewport.Width > 0.5;
            toolbarScrollViewer.Padding = hasHorizontalOverflow ? new Thickness(0, 0, 0, 8) : new Thickness(0);
        }

        private void UpdateSidebarScrollPadding()
        {
            var sidebarScrollViewer = this.FindControl<ScrollViewer>("SidebarScrollViewer");
            if (sidebarScrollViewer == null) return;

            var hasVerticalOverflow = sidebarScrollViewer.Extent.Height - sidebarScrollViewer.Viewport.Height > 0.5;
            sidebarScrollViewer.Padding = hasVerticalOverflow ? new Thickness(0, 0, 8, 0) : new Thickness(0);
        }

        private void OnToolbarScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateToolbarScrollPadding();
        }

        // --- Restored from ref\3babd33_EditorView.axaml.cs lines 767-829 ---

        private void ApplySelectedColor(string colorHex)
        {
            var selected = _selectionController.SelectedShape;
            if (selected == null) return;

            // Ensure the annotation model property is updated so changes persist and effects render correctly
            if (selected.Tag is Annotation annotation)
            {
                annotation.StrokeColor = colorHex;
            }

            var brush = new SolidColorBrush(Color.Parse(colorHex));

            switch (selected)
            {
                case Shape shape:
                    if (shape.Tag is HighlightAnnotation)
                    {
                        // For highlighter, we must regenerate the effect bitmap with the new color
                        OnRequestUpdateEffect(shape);
                        break;
                    }

                    shape.Stroke = brush;

                    if (shape is global::Avalonia.Controls.Shapes.Path path)
                    {
                        path.Fill = brush;
                    }
                    else if (selected is SpeechBalloonControl balloon)
                    {
                         balloon.InvalidateVisual();
                    }
                    break;
                case TextBox textBox:
                    textBox.Foreground = brush;
                    break;
                case Grid grid:
                    foreach (var child in grid.Children)
                    {
                        if (child is Ellipse ellipse)
                        {
                            ellipse.Stroke = brush;
                        }
                    }
                    break;
                case SpeechBalloonControl balloonControl:
                    balloonControl.InvalidateVisual();
                    break;
            }
            
            // ISSUE-LIVE-UPDATE: Update active text editor if present
            _selectionController.UpdateActiveTextEditorProperties();
        }

        private void ApplySelectedStrokeWidth(int width)
        {
            var selected = _selectionController.SelectedShape;
            if (selected == null) return;

            switch (selected)
            {
                case Shape shape:
                    shape.StrokeThickness = width;
                    break;
                case TextBox textBox:
                    textBox.FontSize = Math.Max(12, width * 4);
                    textBox.BorderThickness = new Thickness(Math.Max(1, width / 2));
                    break;
                case Grid grid:
                    foreach (var child in grid.Children)
                    {
                        if (child is Ellipse ellipse)
                        {
                            ellipse.StrokeThickness = Math.Max(1, width);
                        }
                    }
                    break;
                case SpeechBalloonControl balloon:
                    if (balloon.Annotation != null)
                    {
                        balloon.Annotation.StrokeWidth = width;
                        balloon.InvalidateVisual();
                    }
                    break;
            }
        }

        private static Color ApplyHighlightAlpha(Color baseColor)
        {
            return Color.FromArgb(0x55, baseColor.R, baseColor.G, baseColor.B);
        }

        // --- Edit Menu Event Handlers ---

        private void OnResizeImageRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.PreviewImage != null)
            {
                var dialog = new ResizeImageDialog();
                dialog.Initialize((int)vm.ImageWidth, (int)vm.ImageHeight);
                
                dialog.ApplyRequested += (s, args) =>
                {
                    vm.ResizeImage(args.NewWidth, args.NewHeight, args.Quality);
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                dialog.CancelRequested += (s, args) =>
                {
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                vm.EffectsPanelContent = dialog;
                vm.IsEffectsPanelOpen = true;
            }
        }

        private void OnResizeCanvasRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.PreviewImage != null)
            {
                var dialog = new ResizeCanvasDialog();
                // Get edge color from image for "Match image edge" option
                SKColor? edgeColor = null;
                try
                {
                    using var skBitmap = BitmapConversionHelpers.ToSKBitmap(vm.PreviewImage);
                    if (skBitmap != null)
                    {
                        edgeColor = skBitmap.GetPixel(0, 0);
                    }
                }
                catch { }
                
                dialog.Initialize(edgeColor);
                
                dialog.ApplyRequested += (s, args) =>
                {
                    vm.ResizeCanvas(args.Top, args.Right, args.Bottom, args.Left, args.BackgroundColor);
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                dialog.CancelRequested += (s, args) =>
                {
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                vm.EffectsPanelContent = dialog;
                vm.IsEffectsPanelOpen = true;
            }
        }

        private void OnCropImageRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.PreviewImage != null)
            {
                var dialog = new CropImageDialog();
                dialog.Initialize((int)vm.ImageWidth, (int)vm.ImageHeight);
                
                dialog.ApplyRequested += (s, args) =>
                {
                    _editorCore.PerformCrop(args.X, args.Y, args.Width, args.Height);
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                dialog.CancelRequested += (s, args) =>
                {
                    vm.CloseEffectsPanelCommand.Execute(null);
                };
                
                vm.EffectsPanelContent = dialog;
                vm.IsEffectsPanelOpen = true;
            }
        }

        private void OnAutoCropImageRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AutoCropImageCommand.Execute(null);
            }
        }

        private void OnRotate90CWRequested(object? sender, EventArgs e) => _editorCore.PerformRotate90CW();
        private void OnRotate90CCWRequested(object? sender, EventArgs e) => _editorCore.PerformRotate90CCW();
        private void OnRotate180Requested(object? sender, EventArgs e) => _editorCore.PerformRotate180();

        private void OnRotateCustomAngleRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OpenRotateCustomAngleDialogCommand.Execute(null);
                var dialog = new RotateCustomAngleDialog();
                vm.EffectsPanelContent = dialog;
                vm.IsEffectsPanelOpen = true;
            }
        }

        private void OnFlipHorizontalRequested(object? sender, EventArgs e) => _editorCore.PerformFlipHorizontal();
        private void OnFlipVerticalRequested(object? sender, EventArgs e) => _editorCore.PerformFlipVertical();

        // --- Effects Menu Handlers ---

        // --- Effects Menu Handlers ---

        private void OnBrightnessRequested(object? sender, EventArgs e) => ShowEffectDialog(new BrightnessDialog());
        private void OnContrastRequested(object? sender, EventArgs e) => ShowEffectDialog(new ContrastDialog());
        private void OnHueRequested(object? sender, EventArgs e) => ShowEffectDialog(new HueDialog());
        private void OnSaturationRequested(object? sender, EventArgs e) => ShowEffectDialog(new SaturationDialog());
        private void OnGammaRequested(object? sender, EventArgs e) => ShowEffectDialog(new GammaDialog());
        private void OnAlphaRequested(object? sender, EventArgs e) => ShowEffectDialog(new AlphaDialog());
        private void OnColorizeRequested(object? sender, EventArgs e) => ShowEffectDialog(new ColorizeDialog());
        private void OnSelectiveColorRequested(object? sender, EventArgs e) => ShowEffectDialog(new SelectiveColorDialog());
        private void OnReplaceColorRequested(object? sender, EventArgs e) => ShowEffectDialog(new ReplaceColorDialog());
        private void OnGrayscaleRequested(object? sender, EventArgs e) => ShowEffectDialog(new GrayscaleDialog());

        private void OnInvertRequested(object? sender, EventArgs e) => _editorCore.AddEffect(new InvertImageEffect());
        private void OnBlackAndWhiteRequested(object? sender, EventArgs e) => _editorCore.AddEffect(new BlackAndWhiteImageEffect());
        private void OnSepiaRequested(object? sender, EventArgs e) => ShowEffectDialog(new SepiaDialog());
        private void OnPolaroidRequested(object? sender, EventArgs e) => _editorCore.AddEffect(new PolaroidImageEffect());

        // Filter handlers
        private void OnBorderRequested(object? sender, EventArgs e) => ShowEffectDialog(new BorderDialog());
        private void OnOutlineRequested(object? sender, EventArgs e) => ShowEffectDialog(new OutlineDialog());
        private void OnShadowRequested(object? sender, EventArgs e) => ShowEffectDialog(new ShadowDialog());
        private void OnGlowRequested(object? sender, EventArgs e) => ShowEffectDialog(new GlowDialog());
        private void OnReflectionRequested(object? sender, EventArgs e) => ShowEffectDialog(new ReflectionDialog());
        private void OnTornEdgeRequested(object? sender, EventArgs e) => ShowEffectDialog(new TornEdgeDialog());
        private void OnSliceRequested(object? sender, EventArgs e) => ShowEffectDialog(new SliceDialog());
        private void OnRoundedCornersRequested(object? sender, EventArgs e) => ShowEffectDialog(new RoundedCornersDialog());
        private void OnSkewRequested(object? sender, EventArgs e) => ShowEffectDialog(new SkewDialog());
        private void OnRotate3DRequested(object? sender, EventArgs e) => ShowEffectDialog(new Rotate3DDialog());
        private void OnBlurRequested(object? sender, EventArgs e) => ShowEffectDialog(new BlurDialog());
        private void OnPixelateRequested(object? sender, EventArgs e) => ShowEffectDialog(new PixelateDialog());
        private void OnSharpenRequested(object? sender, EventArgs e) => ShowEffectDialog(new SharpenDialog());
        private void OnImportPresetRequested(object? sender, EventArgs e) { if (DataContext is MainViewModel vm) vm.LoadPresetCommand.Execute(null); }
        private void OnExportPresetRequested(object? sender, EventArgs e) { if (DataContext is MainViewModel vm) vm.SavePresetCommand.Execute(null); }


        private void ShowEffectDialog<T>(T dialog) where T : UserControl, IEffectDialog
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            // Wire events to use EditorCore's non-destructive effects pipeline.
            // Preview: show the effect in real-time without committing to undo stack.
            // Apply: commit the effect with proper undo support, preserving annotations.
            // Cancel: remove the preview effect, restoring original state.
            dialog.PreviewRequested += (s, e) => _editorCore.SetPreviewEffect(e.EffectOperation);
            dialog.ApplyRequested += (s, e) =>
            {
                _editorCore.ClearPreviewEffect();
                if (e.EffectInstance != null)
                {
                    _editorCore.AddEffect(e.EffectInstance);
                }
                vm.CloseEffectsPanelCommand.Execute(null);
            };
            dialog.CancelRequested += (s, e) =>
            {
                _editorCore.ClearPreviewEffect();
                vm.CloseEffectsPanelCommand.Execute(null);
            };

            vm.EffectsPanelContent = dialog;
            vm.IsEffectsPanelOpen = true;
        }

        private void OnModalBackgroundPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // Only close if clicking on the background, not the dialog content
            if (e.Source == sender && DataContext is MainViewModel vm)
            {
                vm.CancelEffectPreview();
                vm.CloseModalCommand.Execute(null);
            }
        }


        private async Task<string?> OnOpenImageRequested()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider == null)
            {
                return null;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Open Image",
                AllowMultiple = false,
                FileTypeFilter = new[] { Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        private void OnAddImageAnnotationRequested(string path)
        {
            if (!File.Exists(path) || _editorCore == null) return;

            try
            {
                using var stream = File.OpenRead(path);
                var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                
                // Add logic specific to annotation creation
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                if (canvas == null) return;

                var skBitmap = BitmapConversionHelpers.ToSKBitmap(bitmap);
                if (skBitmap == null) return;

                var annotation = new ImageAnnotation();
                annotation.SetImage(skBitmap);
                annotation.ImagePath = path;

                double canvasWidth = _editorCore.CanvasSize.Width;
                double canvasHeight = _editorCore.CanvasSize.Height;
                if (canvasWidth <= 0 || canvasHeight <= 0)
                {
                    canvasWidth = bitmap.Size.Width;
                    canvasHeight = bitmap.Size.Height;
                }

                var left = (canvasWidth - bitmap.Size.Width) / 2;
                var top = (canvasHeight - bitmap.Size.Height) / 2;
                annotation.StartPoint = new SKPoint((float)left, (float)top);
                annotation.EndPoint = new SKPoint((float)(left + bitmap.Size.Width), (float)(top + bitmap.Size.Height));

                var imageControl = annotation.CreateVisual();
                Canvas.SetLeft(imageControl, left);
                Canvas.SetTop(imageControl, top);
                canvas.Children.Add(imageControl);

                _editorCore.AddAnnotation(annotation);
                _selectionController.SetSelectedShape(imageControl);
                UpdateHasAnnotationsState();
            }
            catch (Exception ex)
            {
                 _ = OnShowErrorDialog("Add Image Failed", ex.Message);
            }
        }

        private async Task OnCopyRequested(Avalonia.Media.Imaging.Bitmap bitmap)
        {
            if (ShareX.Editor.Services.EditorServices.Clipboard != null)
            {
                using var skBitmap = BitmapConversionHelpers.ToSKBitmap(bitmap);
                if (skBitmap != null)
                {
                    ShareX.Editor.Services.EditorServices.Clipboard.SetImage(skBitmap);
                }
            }
            await Task.CompletedTask;
        }

        private async Task<string?> OnSaveAsRequested()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider == null) return null;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Save Image",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } },
                    new Avalonia.Platform.Storage.FilePickerFileType("JPEG Image") { Patterns = new[] { "*.jpg", "*.jpeg" } },
                    new Avalonia.Platform.Storage.FilePickerFileType("Bitmap") { Patterns = new[] { "*.bmp" } }
                },
                DefaultExtension = "png"
            });

            return file?.Path.LocalPath;
        }

        private async Task<string?> OnSavePresetRequested()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider == null) return null;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Save Image Effects Preset",
                DefaultExtension = "xsie",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("XerahS Image Effects") { Patterns = new[] { "*.xsie" } },
                    new Avalonia.Platform.Storage.FilePickerFileType("ShareX Image Effects") { Patterns = new[] { "*.sxie" } }
                }
            });

            return file?.Path.LocalPath;
        }

        private async Task<string?> OnLoadPresetRequested()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider == null) return null;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Load Image Effects Preset",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Image Effects Preset") { Patterns = new[] { "*.xsie", "*.sxie" } }
                }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        private Task OnShowErrorDialog(string title, string message)
        {
            if (DataContext is not MainViewModel vm)
            {
                return Task.CompletedTask;
            }

            vm.ModalContent = new MessageDialog
            {
                Title = title,
                Message = message
            };
            vm.IsModalOpen = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates that UI annotation state is synchronized with EditorCore state.
        /// ISSUE-001 mitigation: Detect annotation count mismatches in dual-state architecture.
        /// </summary>
        private void ValidateAnnotationSync()
        {
            var canvas = this.FindControl<Canvas>("AnnotationCanvas");
            if (canvas == null) return;

            // Count UI annotations (exclude non-annotation controls like CropOverlay)
            int uiAnnotationCount = 0;
            foreach (var child in canvas.Children)
            {
                if (child is Control control && control.Tag is Annotation &&
                    control.Name != "CropOverlay" && control.Name != "CutOutOverlay")
                {
                    uiAnnotationCount++;
                }
            }

            int coreAnnotationCount = _editorCore.Annotations.Count;

            if (uiAnnotationCount != coreAnnotationCount)
            {
                var message = $"[SYNC WARNING] Annotation count mismatch: UI={uiAnnotationCount}, Core={coreAnnotationCount}";
                System.Diagnostics.Debug.WriteLine(message);
            }
        }
    }
}
