using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.Views.Dialogs;

public partial class TornEdgeDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    public TornEdgeDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private int GetDepth() => (int)(this.FindControl<Slider>("DepthSlider")?.Value ?? 20);
    private int GetRange() => (int)(this.FindControl<Slider>("RangeSlider")?.Value ?? 20);
    private bool GetTop() => this.FindControl<CheckBox>("TopCheckBox")?.IsChecked ?? true;
    private bool GetRight() => this.FindControl<CheckBox>("RightCheckBox")?.IsChecked ?? false;
    private bool GetBottom() => this.FindControl<CheckBox>("BottomCheckBox")?.IsChecked ?? true;
    private bool GetLeft() => this.FindControl<CheckBox>("LeftCheckBox")?.IsChecked ?? false;
    private bool GetCurved() => this.FindControl<CheckBox>("CurvedCheckBox")?.IsChecked ?? false;

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void RaisePreview()
    {
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyTornEdge(img, GetDepth(), GetRange(), GetTop(), GetRight(), GetBottom(), GetLeft(), GetCurved()),
            "Torn edge applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyTornEdge(img, GetDepth(), GetRange(), GetTop(), GetRight(), GetBottom(), GetLeft(), GetCurved()),
            "Torn edge applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
