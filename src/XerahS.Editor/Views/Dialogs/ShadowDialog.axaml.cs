using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using ShareX.Editor.ImageEffects.Filters;
using SkiaSharp;

namespace ShareX.Editor.Views.Dialogs;

public partial class ShadowDialog : UserControl, IEffectDialog
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ShadowDialog, Color>(nameof(SelectedColor));

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    private SKColor _color = SKColors.Black;
    private bool _isLoaded = false;

    // Control references
    private Slider? _opacitySlider;
    private Slider? _sizeSlider;
    private Slider? _darknessSlider;
    private Slider? _offsetXSlider;
    private Slider? _offsetYSlider;
    private CheckBox? _autoResizeCheckBox;
    private ColorPicker? _colorPicker;


    public ShadowDialog()
    {
        InitializeComponent();
        
        // Find controls after XAML is loaded
        _opacitySlider = this.FindControl<Slider>("OpacitySlider");
        _sizeSlider = this.FindControl<Slider>("SizeSlider");
        _darknessSlider = this.FindControl<Slider>("DarknessSlider");
        _offsetXSlider = this.FindControl<Slider>("OffsetXSlider");
        _offsetYSlider = this.FindControl<Slider>("OffsetYSlider");
        _autoResizeCheckBox = this.FindControl<CheckBox>("AutoResizeCheckBox");
        _colorPicker = this.FindControl<ColorPicker>("ColorPickerControl");

        
        Loaded += OnLoaded;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        SelectedColor = new Color(_color.Alpha, _color.Red, _color.Green, _color.Blue);
        RaisePreview();
    }

    private float GetOpacity() => (float)(_opacitySlider?.Value ?? 80);
    private int GetSize() => (int)(_sizeSlider?.Value ?? 20);
    private float GetDarkness() => (float)(_darknessSlider?.Value ?? 50) / 100f;
    private int GetOffsetX() => (int)(_offsetXSlider?.Value ?? 5);
    private int GetOffsetY() => (int)(_offsetYSlider?.Value ?? 5);
    private bool GetAutoResize() => _autoResizeCheckBox?.IsChecked ?? true;

    private void OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isLoaded) RaisePreview();
    }

    private void OnCheckChanged(object? sender, RoutedEventArgs e)
    {
        if (_isLoaded) RaisePreview();
    }

    private void OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        _color = new SKColor(e.NewColor.R, e.NewColor.G, e.NewColor.B, e.NewColor.A);
        if (_isLoaded) RaisePreview();
    }


    private void RaisePreview()
    {
        var effect = new ShadowImageEffect(GetOpacity(), GetSize(), GetDarkness(), _color, GetOffsetX(), GetOffsetY(), GetAutoResize());
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => effect.Apply(img),
            "Shadow applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        var effect = new ShadowImageEffect(GetOpacity(), GetSize(), GetDarkness(), _color, GetOffsetX(), GetOffsetY(), GetAutoResize());
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => effect.Apply(img),
            "Shadow applied",
            effect));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
