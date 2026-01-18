using ShareX.Editor.ImageEffects.Adjustments;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ShareX.Editor.ImageEffects;
using SkiaSharp;
using System;

namespace ShareX.Editor.Views.Dialogs
{
    public partial class GammaDialog : UserControl, IEffectDialog
    {
        public event EventHandler<EffectEventArgs>? ApplyRequested;
        public event EventHandler<EffectEventArgs>? PreviewRequested;
        public event EventHandler? CancelRequested;

        private bool _suppressPreview = false;

        public GammaDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_suppressPreview) return;
            RequestPreview();
        }

        private void RequestPreview()
        {
             var slider = this.FindControl<Slider>("AmountSlider");
             float amount = (float)(slider?.Value ?? 1.0);

             if (amount <= 0) amount = 0.1f;

             var effect = new GammaImageEffect { Amount = amount };
             PreviewRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), $"Gamma: {amount:0.0}"));
        }

        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            var slider = this.FindControl<Slider>("AmountSlider");
            float amount = (float)(slider?.Value ?? 1.0);
            
            if (amount <= 0) amount = 0.1f;
            
            var effect = new GammaImageEffect { Amount = amount };
            ApplyRequested?.Invoke(this, new EffectEventArgs(img => effect.Apply(img), $"Applied gamma correction {amount:0.0}", effect));
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

