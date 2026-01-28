using ShareX.Editor.ImageEffects.Adjustments;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.Helpers;
using ShareX.Editor.ImageEffects;
using System;

namespace ShareX.Editor.Views.Dialogs
{
    public partial class SepiaDialog : UserControl, IEffectDialog
    {
        public event EventHandler<EffectEventArgs>? ApplyRequested;
        public event EventHandler<EffectEventArgs>? PreviewRequested;
        public event EventHandler? CancelRequested;

        public SepiaDialog()
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
            float strength = (float)(this.FindControl<Slider>("StrengthSlider")?.Value ?? 100);
            var effect = new SepiaImageEffect { Strength = strength };
            PreviewRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Sepia"));
        }

        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            float strength = (float)(this.FindControl<Slider>("StrengthSlider")?.Value ?? 100);
            var effect = new SepiaImageEffect { Strength = strength };
            ApplyRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), "Applied Sepia", effect));
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

