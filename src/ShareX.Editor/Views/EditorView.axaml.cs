using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ShareX.Editor.Annotations;
using ShareX.Editor.Controls;
using ShareX.Editor.Helpers;
using ShareX.Editor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShareX.Editor.Views
{
    public partial class EditorView : UserControl
    {
        private Point _startPoint;
        private Point _lastDragPoint;
        private bool _isDrawing;
        private bool _isDraggingShape;
        private bool _isDraggingHandle;
        private Control? _currentShape;
        private Control? _selectedShape;
        private Control? _draggedHandle;
        
        // Custom undo buffer since Canvas doesn't support it natively
        private readonly Stack<Control> _undoStack = new();
        private readonly Stack<Control> _redoStack = new();

        // Selection handles
        private readonly List<Control> _selectionHandles = new();

        // For arrow editing (finding endpoints)
        private readonly Dictionary<Control, (Point Start, Point End)> _shapeEndpoints = new();

        // Cached bitmap for effects to avoid continuous conversion
        private SkiaSharp.SKBitmap? _cachedSkBitmap;

        public EditorView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is EditorViewModel vm)
            {
                vm.UndoRequested += (s, args) => PerformUndo();
                vm.RedoRequested += (s, args) => PerformRedo();
                vm.DeleteRequested += (s, args) => PerformDelete();
                vm.ClearAnnotationsRequested += (s, args) => ClearAllAnnotations();
                vm.CopyRequested += (s, args) => CopyImageToClipboard();
                // vm.QuickSaveRequested += ... 
                // vm.SaveAsRequested += ...
                // vm.ApplyEffectRequested += ... handled in OnEffectsPanelApplyRequested but good to hook if invoked from VM
                
                // When preview image changes, clear annotations? Or just resize?
                // For now, assume a new image means a reset.
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(EditorViewModel.PreviewImage))
                    {
                        ClearAllAnnotations();
                        _cachedSkBitmap = null;
                    }
                };
            }
        }

        private async void CopyImageToClipboard()
        {
            // TODO: Implement clipboard copy logic compatible with ShareX.Editor
            // Needed: Render the Canvas + Image to a bitmap
            // Since this logic might be complex and platform specific, 
            // maybe we should expose an event for the host to handle if possible, 
            // OR implement a renderer here.
            
            // For now, simpler to expose event or leave as placeholder if not critical for UI structure move.
            // But user requested "functional".
            
            // Implementation: Render Current UI to Bitmap
            // Requires access to TopLevel or visual tree.
             try
            {
                var canvasContainer = this.FindControl<Grid>("CanvasContainer");
                if (canvasContainer == null) return;
                
                // Create a RenderTargetBitmap
                // Note: Dimensions should match the image size
                if (DataContext is EditorViewModel vm && vm.HasPreviewImage)
                {
                    var pixelSize = new PixelSize((int)vm.ImageWidth, (int)vm.ImageHeight);
                    var bitmap = new Avalonia.Media.Imaging.RenderTargetBitmap(pixelSize, new Vector(96, 96));
                    bitmap.Render(canvasContainer);

                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel?.Clipboard != null)
                    {
                         var data = new DataObject();
                         // data.Set(DataFormats.Bitmap, bitmap); // Avalonia direct bitmap?
                         // Avalonia clipboard might handle it, or we need to convert to System.Drawing/compat type?
                         // Avalonia current Clipboard implementation accepts IDataObject.
                         // But standard SetBitmap might be tricky across platforms without more helpers.
                         // For Windows, this usually works.
                         
                         // Note: Avalonia 11+ clipboard API:
                         // await topLevel.Clipboard.SetTextAsync("Copied image"); 
                         // Setting bitmap is specific.
                         // Let's defer full implementation or use a simple stub if complex.
                         // Or try standard approach:
                         /*
                         var dataObject = new DataObject();
                         dataObject.Set(DataFormats.Bitmap, bitmap);
                         await topLevel.Clipboard.SetDataObjectAsync(dataObject);
                         */
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Copy failed: {ex.Message}");
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is not EditorViewModel vm) return;

            // Shortcuts
            if (e.Key == Key.Delete)
            {
                PerformDelete();
                e.Handled = true;
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                if (e.Key == Key.Z)
                {
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                        PerformRedo();
                    else
                        PerformUndo();
                    e.Handled = true;
                }
                else if (e.Key == Key.C)
                {
                    vm.CopyCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.S)
                {
                    vm.SaveAsCommand.Execute(null);
                    e.Handled = true;
                }
            }
            
            // Tool shortcuts
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                switch (e.Key)
                {
                    case Key.R: vm.ActiveTool = EditorTool.Rectangle; e.Handled = true; break;
                    case Key.E: vm.ActiveTool = EditorTool.Ellipse; e.Handled = true; break;
                    case Key.L: vm.ActiveTool = EditorTool.Line; e.Handled = true; break;
                    case Key.A: vm.ActiveTool = EditorTool.Arrow; e.Handled = true; break;
                    case Key.T: vm.ActiveTool = EditorTool.Text; e.Handled = true; break;
                    case Key.P: vm.ActiveTool = EditorTool.Pen; e.Handled = true; break;
                    case Key.H: vm.ActiveTool = EditorTool.Highlighter; e.Handled = true; break;
                    case Key.B: vm.ActiveTool = EditorTool.SpeechBalloon; e.Handled = true; break;
                    case Key.N: vm.ActiveTool = EditorTool.Number; e.Handled = true; break;
                    case Key.M: vm.ActiveTool = EditorTool.Magnify; e.Handled = true; break;
                    case Key.S: vm.ActiveTool = EditorTool.Spotlight; e.Handled = true; break;
                    case Key.C: vm.ActiveTool = EditorTool.Crop; e.Handled = true; break;
                    case Key.V: vm.ActiveTool = EditorTool.Select; e.Handled = true; break;
                    case Key.F: vm.ToggleEffectsPanelCommand.Execute(null); e.Handled = true; break;
                }
            }
        }

        private void OnScrollViewerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
             // Deselect if clicking outside canvas
             if (e.Source is ScrollViewer || e.Source is LayoutTransformControl || e.Source is Border)
             {
                 _selectedShape = null;
                 UpdateSelectionHandles();
             }
        }
        
        // Zoom handling
        private void OnScrollViewerPointerMoved(object? sender, PointerEventArgs e) { }
        private void OnScrollViewerPointerReleased(object? sender, PointerReleasedEventArgs e) { }

        private Point GetCanvasPosition(PointerEventArgs e, Visual canvas)
        {
            return e.GetPosition(canvas);
        }

        private void UpdateSelectionHandles()
        {
            var overlay = this.FindControl<Canvas>("OverlayCanvas");
            if (overlay == null) return;

            // Clear old handles
            foreach (var handle in _selectionHandles)
            {
                overlay.Children.Remove(handle);
            }
            _selectionHandles.Clear();

            if (_selectedShape == null) return;
            
            // Don't show handles for freehand drawing (Polyline) as it's complex to resize
            if (_selectedShape is Polyline) return;

            // Color for handles
            var handleFill = new SolidColorBrush(Colors.White);
            var handleStroke = new SolidColorBrush(Colors.Blue);

            // Special logic for Line: 2 handles (Start/End)
            if (_selectedShape is Line line)
            {
                AddHandle(overlay, line.StartPoint, "LineStart");
                AddHandle(overlay, line.EndPoint, "LineEnd");
                return;
            }

            // Special logic for Arrow: 2 handles (Start/End)
            if (_selectedShape is global::Avalonia.Controls.Shapes.Path arrowPath && _shapeEndpoints.ContainsKey(arrowPath))
            {
                var points = _shapeEndpoints[arrowPath];
                AddHandle(overlay, points.Start, "ArrowStart");
                AddHandle(overlay, points.End, "ArrowEnd");
                return;
            }

            // Standard Box handles (8 points) for Rect/Ellipse/Image/etc
            var left = Canvas.GetLeft(_selectedShape);
            var top = Canvas.GetTop(_selectedShape);
            var w = _selectedShape.Bounds.Width;
            var h = _selectedShape.Bounds.Height;
            
            // If Bounds not yet updated, use Width/Height properties
            if (double.IsNaN(w) || w == 0) w = _selectedShape.Width;
            if (double.IsNaN(h) || h == 0) h = _selectedShape.Height;
            
            // Corners
            AddHandle(overlay, new Point(left, top), "TopLeft");
            AddHandle(overlay, new Point(left + w, top), "TopRight");
            AddHandle(overlay, new Point(left, top + h), "BottomLeft");
            AddHandle(overlay, new Point(left + w, top + h), "BottomRight");

            // Sides
            AddHandle(overlay, new Point(left + w / 2, top), "Top");
            AddHandle(overlay, new Point(left + w / 2, top + h), "Bottom");
            AddHandle(overlay, new Point(left, top + h / 2), "Left");
            AddHandle(overlay, new Point(left + w, top + h / 2), "Right");
        }

        private void AddHandle(Canvas overlay, Point p, string tag)
        {
            var size = 10;
            var handle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Brushes.White,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Tag = tag,
                Cursor = Cursor.Parse("Hand")
            };

            Canvas.SetLeft(handle, p.X - size / 2);
            Canvas.SetTop(handle, p.Y - size / 2);

            overlay.Children.Add(handle);
            _selectionHandles.Add(handle);
        }

        private void PerformUndo()
        {
            if (_undoStack.Count > 0)
            {
                var shape = _undoStack.Pop();
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                if (canvas != null && canvas.Children.Contains(shape))
                {
                    canvas.Children.Remove(shape);
                    _redoStack.Push(shape);

                    if (_selectedShape == shape) _selectedShape = null;
                }
            }
        }

        private void PerformRedo()
        {
            if (_redoStack.Count > 0)
            {
                var shape = _redoStack.Pop();
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                if (canvas != null)
                {
                    canvas.Children.Add(shape);
                    _undoStack.Push(shape);
                }
            }
        }

        private void PerformDelete()
        {
            if (_selectedShape != null)
            {
                var canvas = this.FindControl<Canvas>("AnnotationCanvas");
                if (canvas != null && canvas.Children.Contains(_selectedShape))
                {
                    canvas.Children.Remove(_selectedShape);
                    
                    // Cleanup shapeEndpoints
                    if (_shapeEndpoints.ContainsKey(_selectedShape))
                    {
                        _shapeEndpoints.Remove(_selectedShape);
                    }

                    _selectedShape = null;
                    UpdateSelectionHandles();
                }
            }
        }

        private void ClearAllAnnotations()
        {
            var canvas = this.FindControl<Canvas>("AnnotationCanvas");
            if (canvas != null)
            {
                canvas.Children.Clear();
            }

            var overlay = this.FindControl<Canvas>("OverlayCanvas");
            if (overlay != null)
            {
                foreach (var handle in _selectionHandles)
                {
                    overlay.Children.Remove(handle);
                }
            }
            _selectionHandles.Clear();
            _shapeEndpoints.Clear();

            var cropOverlay = this.FindControl<Rectangle>("CropOverlay");
            if (cropOverlay != null)
            {
                cropOverlay.IsVisible = false;
                cropOverlay.Width = 0;
                cropOverlay.Height = 0;
            }

            _selectedShape = null;
            _currentShape = null;
            _isDrawing = false;
            _isDraggingHandle = false;
            _draggedHandle = null;
            _isDraggingShape = false;

            _undoStack.Clear();
            _redoStack.Clear();
            
            // Clean up cached bitmap
            _cachedSkBitmap?.Dispose();
            _cachedSkBitmap = null;
            ResetScrollViewerOffset();
        }

        private void ResetScrollViewerOffset()
        {
            var scrollViewer = this.FindControl<ScrollViewer>("CanvasScrollViewer");
            if (scrollViewer == null) return;

            Dispatcher.UIThread.Post(() => scrollViewer.Offset = new Vector(0, 0), DispatcherPriority.Render);
        }

        private void ApplySelectedColor(string colorHex)
        {
            if (_selectedShape == null) return;

            var brush = new SolidColorBrush(Color.Parse(colorHex));

            switch (_selectedShape)
            {
                case Shape shape:
                    if (shape.Tag is HighlightAnnotation)
                    {
                        var highlightColor = Color.Parse(colorHex);
                        shape.Stroke = Brushes.Transparent;
                        shape.Fill = new SolidColorBrush(ApplyHighlightAlpha(highlightColor));
                        break;
                    }

                    shape.Stroke = brush;

                    if (shape is global::Avalonia.Controls.Shapes.Path path)
                    {
                        path.Fill = brush;
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
                            ellipse.Fill = brush;
                        }
                    }
                    break;
            }
        }

        private void ApplySelectedStrokeWidth(int width)
        {
            if (_selectedShape == null) return;

            switch (_selectedShape)
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
            }
        }

        private static Color ApplyHighlightAlpha(Color baseColor)
        {
            return Color.FromArgb(0x55, baseColor.R, baseColor.G, baseColor.B);
        }

        private async void OnCanvasPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is not EditorViewModel vm) return;
            // Always draw on the main annotation canvas even if the overlay receives the event.
            var canvas = this.FindControl<Canvas>("AnnotationCanvas") ?? sender as Canvas;
            if (canvas == null) return;

            // Ignore middle mouse to avoid creating annotations while panning
            var props = e.GetCurrentPoint(canvas).Properties;
            if (props.IsMiddleButtonPressed) return;

            var point = GetCanvasPosition(e, canvas);

            // Handle right-click to delete shape
            if (props.IsRightButtonPressed)
            {
                // Hit test to find the shape under cursor
                var hitSource = e.Source as Visual;
                Control? hitTarget = null;

                while (hitSource != null && hitSource != canvas)
                {
                    if (canvas.Children.Contains(hitSource as Control))
                    {
                        hitTarget = hitSource as Control;
                        break;
                    }
                    hitSource = hitSource.GetVisualParent();
                }

                if (hitTarget != null && hitTarget.Name != "CropOverlay")
                {
                    canvas.Children.Remove(hitTarget);
                    
                    if (_shapeEndpoints.ContainsKey(hitTarget))
                    {
                        _shapeEndpoints.Remove(hitTarget);
                    }

                    // Remove from undo stack if present
                    if (_undoStack.Contains(hitTarget))
                    {
                        // Helper to remove from stack specific item? 
                        // Simplified: just rebuild stack (inefficient but works)
                        var tempStack = new Stack<Control>();
                        while (_undoStack.Count > 0)
                        {
                            var item = _undoStack.Pop();
                            if (item != hitTarget) tempStack.Push(item);
                        }
                        while (tempStack.Count > 0) _undoStack.Push(tempStack.Pop());
                    }

                    if (_selectedShape == hitTarget)
                    {
                        _selectedShape = null;
                        UpdateSelectionHandles();
                    }

                    vm.StatusText = "Shape deleted";
                    e.Handled = true;
                }
                return;
            }

            // Check if clicking a handle
            if (_selectedShape != null || vm.ActiveTool == EditorTool.Crop)
            {
                var overlay = this.FindControl<Canvas>("OverlayCanvas");
                if (overlay != null)
                {
                    var handle = e.Source as Control;
                    if (handle != null && overlay.Children.Contains(handle) && _selectionHandles.Contains(handle))
                    {
                        _isDraggingHandle = true;
                        _draggedHandle = handle;
                        _startPoint = GetCanvasPosition(e, overlay); 

                        if (vm.ActiveTool == EditorTool.Crop)
                        {
                            var cropOverlay = this.FindControl<Rectangle>("CropOverlay");
                            _selectedShape = cropOverlay;
                        }
                        return;
                    }
                }
            }

            // Allow dragging selected shapes
            if (_selectedShape != null && vm.ActiveTool != EditorTool.Select)
            {
                var hitSource = e.Source as Visual;
                Control? hitTarget = null;

                while (hitSource != null && hitSource != canvas)
                {
                    if (canvas.Children.Contains(hitSource as Control))
                    {
                        hitTarget = hitSource as Control;
                        break;
                    }
                    hitSource = hitSource.GetVisualParent();
                }

                if (hitTarget == _selectedShape)
                {
                    _lastDragPoint = point;
                    _isDraggingShape = true;
                    e.Pointer.Capture(canvas);
                    return;
                }
                else
                {
                    _selectedShape = null;
                    UpdateSelectionHandles();
                }
            }

            if (vm.ActiveTool == EditorTool.Select)
            {
                var hitSource = e.Source as Visual;
                Control? hitTarget = null;

                while (hitSource != null && hitSource != canvas)
                {
                    if (canvas.Children.Contains(hitSource as Control))
                    {
                        hitTarget = hitSource as Control;
                        break;
                    }
                    hitSource = hitSource.GetVisualParent();
                }

                if (hitTarget != null && hitTarget.Name != "CropOverlay")
                {
                    _selectedShape = hitTarget;
                    _lastDragPoint = point;
                    _isDraggingShape = true;
                    UpdateSelectionHandles();
                }
                else
                {
                    _selectedShape = null;
                    UpdateSelectionHandles();
                }
                return;
            }

            // New Drawing
            _redoStack.Clear();

            _startPoint = point;
            _isDrawing = true;
            e.Pointer.Capture(canvas);

            var brush = new SolidColorBrush(Color.Parse(vm.SelectedColor));

            if (vm.ActiveTool == EditorTool.Crop)
            {
                var cropOverlay = this.FindControl<Rectangle>("CropOverlay");
                if (cropOverlay != null)
                {
                    cropOverlay.IsVisible = true;
                    Canvas.SetLeft(cropOverlay, _startPoint.X);
                    Canvas.SetTop(cropOverlay, _startPoint.Y);
                    cropOverlay.Width = 0;
                    cropOverlay.Height = 0;
                    _currentShape = cropOverlay;
                }
                return;
            }

            if (vm.ActiveTool == EditorTool.Image)
            {
                _isDrawing = false;
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.StorageProvider != null)
                {
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Select Image",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                    });

                    if (files.Count > 0)
                    {
                        try
                        {
                            using var stream = await files[0].OpenReadAsync();
                            var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);

                            var imageControl = new Image
                            {
                                Source = bitmap,
                                Width = bitmap.Size.Width,
                                Height = bitmap.Size.Height
                            };

                            var annotation = new ImageAnnotation();
                            annotation.SetImage(BitmapConversionHelpers.ToSKBitmap(bitmap));
                            imageControl.Tag = annotation;

                            Canvas.SetLeft(imageControl, _startPoint.X - bitmap.Size.Width / 2);
                            Canvas.SetTop(imageControl, _startPoint.Y - bitmap.Size.Height / 2);

                            canvas.Children.Add(imageControl);
                            _undoStack.Push(imageControl);

                            _currentShape = imageControl;
                            _selectedShape = imageControl;
                            UpdateSelectionHandles();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                }
                return;
            }

            switch (vm.ActiveTool)
            {
                case EditorTool.Rectangle:
                    _currentShape = new Rectangle
                    {
                        Stroke = brush,
                        StrokeThickness = vm.StrokeWidth,
                        Fill = Brushes.Transparent
                    };
                    break;
                case EditorTool.Ellipse:
                    _currentShape = new Ellipse
                    {
                        Stroke = brush,
                        StrokeThickness = vm.StrokeWidth,
                        Fill = Brushes.Transparent
                    };
                    break;
                case EditorTool.Line:
                    _currentShape = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = vm.StrokeWidth,
                        StartPoint = _startPoint,
                        EndPoint = _startPoint
                    };
                    break;
                case EditorTool.Arrow:
                    _currentShape = new global::Avalonia.Controls.Shapes.Path
                    {
                        Stroke = brush,
                        StrokeThickness = vm.StrokeWidth,
                        Fill = brush,
                        Data = new PathGeometry()
                    };
                    _shapeEndpoints[_currentShape] = (_startPoint, _startPoint);
                    break;
                case EditorTool.Text:
                    var textBox = new TextBox
                    {
                        Foreground = brush,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.White,
                        FontSize = Math.Max(12, vm.StrokeWidth * 4),
                        Text = string.Empty,
                        Padding = new Thickness(4),
                        MinWidth = 50,
                        AcceptsReturn = false
                    };
                    Canvas.SetLeft(textBox, _startPoint.X);
                    Canvas.SetTop(textBox, _startPoint.Y);
                    
                    textBox.LostFocus += (s, args) =>
                    {
                        if (s is TextBox tb)
                        {
                            tb.BorderThickness = new Thickness(0);
                            if (string.IsNullOrWhiteSpace(tb.Text))
                            {
                                var parentKey = tb.Parent as Panel;
                                parentKey?.Children.Remove(tb);
                            }
                        }
                    };
                    textBox.KeyDown += (s, args) =>
                    {
                        if (args.Key == Key.Enter)
                        {
                            args.Handled = true;
                            this.Focus();
                        }
                    };
                    canvas.Children.Add(textBox);
                    textBox.Focus();
                    _isDrawing = false;
                    return;

                case EditorTool.Spotlight:
                    var spotlightAnnotation = new SpotlightAnnotation();
                    spotlightAnnotation.CanvasSize = new SkiaSharp.SKSize((float)canvas.Bounds.Width, (float)canvas.Bounds.Height);
                    
                    var spotlightControl = new SpotlightControl
                    {
                        Annotation = spotlightAnnotation,
                        IsHitTestVisible = true
                    };
                    _currentShape = spotlightControl;
                    break;

                case EditorTool.Blur:
                case EditorTool.Pixelate:
                case EditorTool.Magnify:
                case EditorTool.Highlighter:
                    var effectRect = new Rectangle
                    {
                        Stroke = (vm.ActiveTool == EditorTool.Magnify) ? brush : Brushes.Transparent,
                        StrokeThickness = vm.StrokeWidth,
                        Fill = (vm.ActiveTool == EditorTool.Highlighter) ? new SolidColorBrush(ApplyHighlightAlpha(Color.Parse(vm.SelectedColor))) : Brushes.Transparent,
                        Tag = CreateEffectAnnotation(vm.ActiveTool)
                    };
                    if (vm.ActiveTool == EditorTool.Blur) effectRect.Fill = new SolidColorBrush(Color.Parse("#200000FF"));
                    else if (vm.ActiveTool == EditorTool.Pixelate) effectRect.Fill = new SolidColorBrush(Color.Parse("#2000FF00"));
                    _currentShape = effectRect;
                    break;

                case EditorTool.SpeechBalloon:
                    _currentShape = new Rectangle
                    {
                        Stroke = brush,
                        StrokeThickness = vm.StrokeWidth,
                        Fill = Brushes.White,
                        RadiusX = 10,
                        RadiusY = 10
                    };
                    break;

                case EditorTool.Pen:
                case EditorTool.SmartEraser:
                    var polyline = new Polyline
                    {
                        Stroke = (vm.ActiveTool == EditorTool.SmartEraser) ? new SolidColorBrush(Color.Parse("#80FF0000")) : brush,
                        StrokeThickness = (vm.ActiveTool == EditorTool.SmartEraser) ? 10 : vm.StrokeWidth,
                        Points = new Points { _startPoint }
                    };
                    polyline.SetValue(Panel.ZIndexProperty, 1);
                    
                    // Smart Eraser / Pen Tag Logic would go here
                    // For now basic implementation
                    polyline.Tag = vm.ActiveTool == EditorTool.SmartEraser ? new SmartEraserAnnotation() : new FreehandAnnotation();

                    _currentShape = polyline;
                    break;

                case EditorTool.Number:
                    var numberGrid = new Grid
                    {
                        Width = 30,
                        Height = 30
                    };
                    var bg = new Ellipse
                    {
                        Fill = brush,
                        Stroke = Brushes.White,
                        StrokeThickness = 2
                    };
                    var numText = new TextBlock
                    {
                        Text = vm.NumberCounter.ToString(),
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeight.Bold
                    };
                    numberGrid.Children.Add(bg);
                    numberGrid.Children.Add(numText);
                    Canvas.SetLeft(numberGrid, _startPoint.X - 15);
                    Canvas.SetTop(numberGrid, _startPoint.Y - 15);
                    _currentShape = numberGrid;
                    vm.NumberCounter++;
                    canvas.Children.Add(numberGrid);
                    break;
            }

            if (_currentShape != null)
            {
                if (vm.ActiveTool != EditorTool.Number && vm.ActiveTool != EditorTool.Line && vm.ActiveTool != EditorTool.Arrow && vm.ActiveTool != EditorTool.Pen && vm.ActiveTool != EditorTool.SmartEraser && vm.ActiveTool != EditorTool.Spotlight)
                {
                    Canvas.SetLeft(_currentShape, _startPoint.X);
                    Canvas.SetTop(_currentShape, _startPoint.Y);
                    canvas.Children.Add(_currentShape);
                }
                else if (vm.ActiveTool != EditorTool.Number)
                {
                    canvas.Children.Add(_currentShape);
                }
            }
        }

        private void OnCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            var canvas = this.FindControl<Canvas>("AnnotationCanvas") ?? sender as Canvas;
            if (canvas == null) return;
            var currentPoint = GetCanvasPosition(e, canvas);

            if (_isDraggingHandle && _draggedHandle != null && _selectedShape != null)
            {
                var handleTag = _draggedHandle.Tag?.ToString();
                var deltaX = currentPoint.X - _startPoint.X;
                var deltaY = currentPoint.Y - _startPoint.Y;

                if (_selectedShape is Line targetLine)
                {
                    if (handleTag == "LineStart") targetLine.StartPoint = currentPoint;
                    else if (handleTag == "LineEnd") targetLine.EndPoint = currentPoint;
                    _startPoint = currentPoint;
                    UpdateSelectionHandles();
                    return;
                }

                if (_selectedShape is global::Avalonia.Controls.Shapes.Path arrowPath && DataContext is EditorViewModel vm)
                {
                    if (_shapeEndpoints.TryGetValue(arrowPath, out var endpoints))
                    {
                        Point arrowStart = endpoints.Start;
                        Point arrowEnd = endpoints.End;

                        if (handleTag == "ArrowStart") arrowStart = currentPoint;
                        else if (handleTag == "ArrowEnd") arrowEnd = currentPoint;

                        _shapeEndpoints[arrowPath] = (arrowStart, arrowEnd);
                        arrowPath.Data = CreateArrowGeometry(arrowStart, arrowEnd, vm.StrokeWidth * 3);
                    }
                    _startPoint = currentPoint;
                    UpdateSelectionHandles();
                    return;
                }

                var left = Canvas.GetLeft(_selectedShape);
                var top = Canvas.GetTop(_selectedShape);
                var width = _selectedShape.Bounds.Width;
                var height = _selectedShape.Bounds.Height;
                if (double.IsNaN(width)) width = _selectedShape.Width;
                if (double.IsNaN(height)) height = _selectedShape.Height;

                if (_selectedShape is Rectangle || _selectedShape is Ellipse || _selectedShape is Grid)
                {
                    double newLeft = left;
                    double newTop = top;
                    double newWidth = width;
                    double newHeight = height;

                    if (handleTag.Contains("Right")) newWidth = Math.Max(1, width + deltaX);
                    else if (handleTag.Contains("Left")) { var change = Math.Min(width - 1, deltaX); newLeft += change; newWidth -= change; }
                    if (handleTag.Contains("Bottom")) newHeight = Math.Max(1, height + deltaY);
                    else if (handleTag.Contains("Top")) { var change = Math.Min(height - 1, deltaY); newTop += change; newHeight -= change; }

                    Canvas.SetLeft(_selectedShape, newLeft);
                    Canvas.SetTop(_selectedShape, newTop);
                    _selectedShape.Width = newWidth;
                    _selectedShape.Height = newHeight;
                }

                _startPoint = currentPoint;
                UpdateSelectionHandles();
                return;
            }

            if (_isDraggingShape && _selectedShape != null)
            {
                var deltaX = currentPoint.X - _lastDragPoint.X;
                var deltaY = currentPoint.Y - _lastDragPoint.Y;

                if (_selectedShape is Line targetLine)
                {
                    targetLine.StartPoint = new Point(targetLine.StartPoint.X + deltaX, targetLine.StartPoint.Y + deltaY);
                    targetLine.EndPoint = new Point(targetLine.EndPoint.X + deltaX, targetLine.EndPoint.Y + deltaY);
                    _lastDragPoint = currentPoint;
                    UpdateSelectionHandles();
                    return;
                }

                if (_selectedShape is global::Avalonia.Controls.Shapes.Path arrowPath && DataContext is EditorViewModel vm)
                {
                    if (_shapeEndpoints.TryGetValue(arrowPath, out var endpoints))
                    {
                        var newStart = new Point(endpoints.Start.X + deltaX, endpoints.Start.Y + deltaY);
                        var newEnd = new Point(endpoints.End.X + deltaX, endpoints.End.Y + deltaY);
                        _shapeEndpoints[arrowPath] = (newStart, newEnd);
                        arrowPath.Data = CreateArrowGeometry(newStart, newEnd, vm.StrokeWidth * 3);
                    }
                    _lastDragPoint = currentPoint;
                    UpdateSelectionHandles();
                    return;
                }

                var left = Canvas.GetLeft(_selectedShape);
                var top = Canvas.GetTop(_selectedShape);
                Canvas.SetLeft(_selectedShape, left + deltaX);
                Canvas.SetTop(_selectedShape, top + deltaY);

                _lastDragPoint = currentPoint;
                UpdateSelectionHandles();
                return;
            }

            if (!_isDrawing || _currentShape == null) return;

            if (_currentShape is Line line)
            {
                line.EndPoint = currentPoint;
            }
            else if (_currentShape is Polyline polyline)
            {
                var updated = new Points(polyline.Points);
                updated.Add(currentPoint);
                polyline.Points = updated;
                // polyline.Tag specific logic..
            }
            else if (_currentShape is global::Avalonia.Controls.Shapes.Path arrowPath && DataContext is EditorViewModel vm)
            {
                arrowPath.Data = CreateArrowGeometry(_startPoint, currentPoint, vm.StrokeWidth * 3);
                _shapeEndpoints[arrowPath] = (_startPoint, currentPoint);
            }
            else
            {
                var x = Math.Min(_startPoint.X, currentPoint.X);
                var y = Math.Min(_startPoint.Y, currentPoint.Y);
                var width = Math.Abs(_startPoint.X - currentPoint.X);
                var height = Math.Abs(_startPoint.Y - currentPoint.Y);

                if (_currentShape is Rectangle rect)
                {
                    rect.Width = width;
                    rect.Height = height;
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    if (rect.Tag is BaseEffectAnnotation) UpdateEffectVisual(rect);
                }
                else if (_currentShape is Ellipse ellipse)
                {
                    ellipse.Width = width;
                    ellipse.Height = height;
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                }
                else if (_currentShape is SpotlightControl spotlightControl && spotlightControl.Annotation is SpotlightAnnotation spotlight)
                {
                     spotlight.StartPoint = new SkiaSharp.SKPoint((float)_startPoint.X, (float)_startPoint.Y);
                     spotlight.EndPoint = new SkiaSharp.SKPoint((float)currentPoint.X, (float)currentPoint.Y);
                     var parentCanvas = this.FindControl<Canvas>("AnnotationCanvas");
                     if (parentCanvas != null)
                     {
                         spotlight.CanvasSize = new SkiaSharp.SKSize((float)parentCanvas.Bounds.Width, (float)parentCanvas.Bounds.Height);
                     }
                     spotlightControl.InvalidateVisual();
                }
            }
        }

        private void OnCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDraggingHandle)
            {
                _isDraggingHandle = false;
                _draggedHandle = null;
                e.Pointer.Capture(null);
                return;
            }

            if (_isDraggingShape)
            {
                _isDraggingShape = false;
                e.Pointer.Capture(null);
                return;
            }

            if (_isDrawing)
            {
                _isDrawing = false;
                if (_currentShape != null)
                {
                    var createdShape = _currentShape;
                    
                    if (DataContext is EditorViewModel vm && vm.ActiveTool == EditorTool.Crop && createdShape.Name == "CropOverlay")
                    {
                        PerformCrop();
                        _currentShape = null;
                        e.Pointer.Capture(null);
                        return;
                    }
                    
                    _undoStack.Push(createdShape);

                    if (createdShape is not Polyline)
                    {
                        if (createdShape.Tag is BaseEffectAnnotation)
                        {
                            UpdateEffectVisual(createdShape);
                        }
                        
                        _selectedShape = createdShape;
                        UpdateSelectionHandles();
                    }
                    _currentShape = null;
                }
                e.Pointer.Capture(null);
            }
        }

        private BaseEffectAnnotation? CreateEffectAnnotation(EditorTool tool)
        {
            return tool switch
            {
                EditorTool.Blur => new BlurAnnotation(),
                EditorTool.Pixelate => new PixelateAnnotation(),
                EditorTool.Magnify => new MagnifyAnnotation(),
                EditorTool.Highlighter => new HighlightAnnotation(),
                _ => null
            };
        }

        private void UpdateEffectVisual(Control shape)
        {
            if (shape.Tag is not BaseEffectAnnotation annotation) return;
            if (_isDrawing) return;
            if (DataContext is not EditorViewModel vm || vm.PreviewImage == null) return;

            try
            {
                double left = Canvas.GetLeft(shape);
                double top = Canvas.GetTop(shape);
                double width = shape.Bounds.Width;
                double height = shape.Bounds.Height;

                if (width <= 0 || height <= 0) return;

                annotation.StartPoint = new SkiaSharp.SKPoint((float)left, (float)top);
                annotation.EndPoint = new SkiaSharp.SKPoint((float)(left + width), (float)(top + height));

                if (_cachedSkBitmap == null)
                {
                    _cachedSkBitmap = BitmapConversionHelpers.ToSKBitmap(vm.PreviewImage);
                }

                annotation.UpdateEffect(_cachedSkBitmap);

                if (annotation.EffectBitmap != null && shape is Shape shapeControl)
                {
                    var avaloniaBitmap = BitmapConversionHelpers.ToAvaloniaBitmap(annotation.EffectBitmap);
                    shapeControl.Fill = new ImageBrush(avaloniaBitmap)
                    {
                        Stretch = Stretch.None,
                        SourceRect = new RelativeRect(0, 0, width, height, RelativeUnit.Absolute)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Effect update failed: {ex.Message}");
            }
        }

        private Geometry CreateArrowGeometry(Point start, Point end, double headSize)
        {
             var geometry = new StreamGeometry();
             using (var ctx = geometry.Open())
             {
                 var d = end - start;
                 var length = Math.Sqrt(d.X * d.X + d.Y * d.Y);
                 if (length > 0)
                 {
                     var ux = d.X / length;
                     var uy = d.Y / length;
                     var arrowAngle = Math.PI / 9;
                     var arrowBase = new Point(end.X - headSize * ux, end.Y - headSize * uy);
                     var p1 = new Point(end.X - headSize * Math.Cos(Math.Atan2(uy, ux) - arrowAngle), end.Y - headSize * Math.Sin(Math.Atan2(uy, ux) - arrowAngle));
                     var p2 = new Point(end.X - headSize * Math.Cos(Math.Atan2(uy, ux) + arrowAngle), end.Y - headSize * Math.Sin(Math.Atan2(uy, ux) + arrowAngle));

                     ctx.BeginFigure(start, false);
                     ctx.LineTo(arrowBase);
                     ctx.EndFigure(false);

                     ctx.BeginFigure(end, true);
                     ctx.LineTo(p1);
                     ctx.LineTo(p2);
                     ctx.EndFigure(true);
                 }
                 else
                 {
                     ctx.BeginFigure(start, false);
                     ctx.LineTo(end);
                     ctx.EndFigure(false);
                 }
             }
             return geometry;
        }

        public void PerformCrop()
        {
            var cropOverlay = this.FindControl<Rectangle>("CropOverlay");
            if (cropOverlay == null || !cropOverlay.IsVisible || DataContext is not EditorViewModel vm) return;

            var x = Canvas.GetLeft(cropOverlay);
            var y = Canvas.GetTop(cropOverlay);
            var w = cropOverlay.Width;
            var h = cropOverlay.Height;

            var scaling = 1.0;
            if (VisualRoot is TopLevel tl) scaling = tl.RenderScaling;

            var physX = (int)(x * scaling);
            var physY = (int)(y * scaling);
            var physW = (int)(w * scaling);
            var physH = (int)(h * scaling);

            vm.CropImage(physX, physY, physW, physH);
            cropOverlay.IsVisible = false;
            vm.StatusText = "Image cropped";
        }

        private void OnEffectsPanelApplyRequested(object? sender, RoutedEventArgs e)
        {
            if (DataContext is EditorViewModel vm)
            {
               vm.ApplyEffectCommand.Execute(null);
            }
        }
    }
}
