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

public partial class BorderDialog : UserControl, IEffectDialog
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<BorderDialog, Color>(nameof(SelectedColor));

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
    private ComboBox? _typeComboBox;
    private ComboBox? _dashStyleComboBox;
    private Slider? _sizeSlider;
    private ColorPicker? _colorPicker;


    public BorderDialog()
    {
        InitializeComponent();
        
        // Find controls after XAML is loaded
        _typeComboBox = this.FindControl<ComboBox>("TypeComboBox");
        _dashStyleComboBox = this.FindControl<ComboBox>("DashStyleComboBox");
        _sizeSlider = this.FindControl<Slider>("SizeSlider");
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

    private ImageHelpers.BorderType GetBorderType()
    {
        return _typeComboBox?.SelectedIndex == 1 ? ImageHelpers.BorderType.Inside : ImageHelpers.BorderType.Outside;
    }

    private ImageHelpers.DashStyle GetDashStyle()
    {
        return _dashStyleComboBox?.SelectedIndex switch
        {
            1 => ImageHelpers.DashStyle.Dash,
            2 => ImageHelpers.DashStyle.Dot,
            3 => ImageHelpers.DashStyle.DashDot,
            _ => ImageHelpers.DashStyle.Solid
        };
    }

    private int GetSize() => (int)(_sizeSlider?.Value ?? 5);

    private void OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isLoaded) RaisePreview();
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        var type = GetBorderType();
        var size = GetSize();
        var dashStyle = GetDashStyle();

        var effect = new BorderImageEffect(type, size, dashStyle, _color);
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => effect.Apply(img),
            "Border applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        var type = GetBorderType();
        var size = GetSize();
        var dashStyle = GetDashStyle();

        var effect = new BorderImageEffect(type, size, dashStyle, _color);
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => effect.Apply(img),
            "Border applied",
            effect));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
