using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.Views.Dialogs;

public partial class SliceDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    public SliceDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private int GetMinHeight() => (int)(this.FindControl<Slider>("MinHeightSlider")?.Value ?? 5);
    private int GetMaxHeight() => (int)(this.FindControl<Slider>("MaxHeightSlider")?.Value ?? 20);
    private int GetMinShift() => (int)(this.FindControl<Slider>("MinShiftSlider")?.Value ?? -10);
    private int GetMaxShift() => (int)(this.FindControl<Slider>("MaxShiftSlider")?.Value ?? 10);

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void RaisePreview()
    {
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplySlice(img, GetMinHeight(), GetMaxHeight(), GetMinShift(), GetMaxShift()),
            "Slice applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplySlice(img, GetMinHeight(), GetMaxHeight(), GetMinShift(), GetMaxShift()),
            "Slice applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
