using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Views.Dialogs;

public partial class BorderDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    private SKColor _color = SKColors.Black;

    public BorderDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private ImageHelpers.BorderType GetBorderType()
    {
        var combo = this.FindControl<ComboBox>("TypeComboBox");
        return combo?.SelectedIndex == 1 ? ImageHelpers.BorderType.Inside : ImageHelpers.BorderType.Outside;
    }

    private ImageHelpers.DashStyle GetDashStyle()
    {
        var combo = this.FindControl<ComboBox>("DashStyleComboBox");
        return combo?.SelectedIndex switch
        {
            1 => ImageHelpers.DashStyle.Dash,
            2 => ImageHelpers.DashStyle.Dot,
            3 => ImageHelpers.DashStyle.DashDot,
            _ => ImageHelpers.DashStyle.Solid
        };
    }

    private int GetSize() => (int)(this.FindControl<Slider>("SizeSlider")?.Value ?? 5);

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void OnColorTextChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = this.FindControl<TextBox>("ColorTextBox");
        var preview = this.FindControl<Border>("ColorPreview");
        if (textBox != null && preview != null)
        {
            try
            {
                var color = Color.Parse(textBox.Text ?? "#000000");
                preview.Background = new SolidColorBrush(color);
                _color = new SKColor(color.R, color.G, color.B, color.A);
                RaisePreview();
            }
            catch { }
        }
    }

    private void RaisePreview()
    {
        var type = GetBorderType();
        var size = GetSize();
        var dashStyle = GetDashStyle();
        
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyBorder(img, type, size, dashStyle, _color),
            "Border applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        var type = GetBorderType();
        var size = GetSize();
        var dashStyle = GetDashStyle();
        
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyBorder(img, type, size, dashStyle, _color),
            "Border applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
