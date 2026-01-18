using ShareX.Editor.ImageEffects.Manipulations;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;
using ShareX.Editor.ImageEffects;
using System;

namespace ShareX.Editor.Views.Dialogs
{
    public partial class RoundedCornersDialog : UserControl, IEffectDialog
    {
        public event EventHandler<EffectEventArgs>? ApplyRequested;
        public event EventHandler<EffectEventArgs>? PreviewRequested;
        public event EventHandler? CancelRequested;

        public RoundedCornersDialog()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachedToVisualTree += (s, e) => RequestPreview();
        }

        private void OnValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            RequestPreview();
        }

        private void RequestPreview()
        {
            int radius = (int)(this.FindControl<Slider>("RadiusSlider")?.Value ?? 20);
            var effect = new RoundedCornersImageEffect { CornerRadius = radius };
            PreviewRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Rounded Corners"));
        }

        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            int radius = (int)(this.FindControl<Slider>("RadiusSlider")?.Value ?? 20);
            var effect = new RoundedCornersImageEffect { CornerRadius = radius };
            ApplyRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Applied Rounded Corners", effect));
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

