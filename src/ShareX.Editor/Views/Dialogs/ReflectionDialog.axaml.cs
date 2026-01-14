using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.Views.Dialogs;

public partial class ReflectionDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    public ReflectionDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private int GetPercentage() => (int)(this.FindControl<Slider>("PercentageSlider")?.Value ?? 50);
    private int GetMaxAlpha() => (int)(this.FindControl<Slider>("MaxAlphaSlider")?.Value ?? 200);
    private int GetMinAlpha() => (int)(this.FindControl<Slider>("MinAlphaSlider")?.Value ?? 0);
    private int GetOffset() => (int)(this.FindControl<Slider>("OffsetSlider")?.Value ?? 0);
    private bool GetSkew() => this.FindControl<CheckBox>("SkewCheckBox")?.IsChecked ?? false;
    private int GetSkewSize() => (int)(this.FindControl<Slider>("SkewSizeSlider")?.Value ?? 25);

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void RaisePreview()
    {
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyReflection(img, GetPercentage(), GetMaxAlpha(), GetMinAlpha(), GetOffset(), GetSkew(), GetSkewSize()),
            "Reflection applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyReflection(img, GetPercentage(), GetMaxAlpha(), GetMinAlpha(), GetOffset(), GetSkew(), GetSkewSize()),
            "Reflection applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
