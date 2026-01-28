using ShareX.Editor.ImageEffects.Filters;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;
using ShareX.Editor.ImageEffects;
using System;

namespace ShareX.Editor.Views.Dialogs
{
    public partial class SharpenDialog : UserControl, IEffectDialog
    {
        public event EventHandler<EffectEventArgs>? ApplyRequested;
        public event EventHandler<EffectEventArgs>? PreviewRequested;
        public event EventHandler? CancelRequested;

        public SharpenDialog()
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
            int strength = (int)(this.FindControl<Slider>("StrengthSlider")?.Value ?? 50);
            var effect = new SharpenImageEffect { Strength = strength };
            PreviewRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Sharpen"));
        }

        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            int strength = (int)(this.FindControl<Slider>("StrengthSlider")?.Value ?? 50);
            var effect = new SharpenImageEffect { Strength = strength };
            ApplyRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Applied Sharpen", effect));
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

