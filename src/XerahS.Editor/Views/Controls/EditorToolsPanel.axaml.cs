#nullable enable
#region License Information (GPL v3)

/*
    ShareX.Ava - The Avalonia UI implementation of ShareX
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Avalonia.Controls;
using ShareX.Editor.Controls;

namespace ShareX.Editor.Views.Controls
{
    /// <summary>
    /// Editor-specific tools panel containing Crop, CutOut, Background, and Effects.
    /// This panel is excluded from the Region Capture Overlay and is only used in the Image Editor.
    /// Designed to be injected into AnnotationToolbar's ExtensionContent slot.
    /// </summary>
    public partial class EditorToolsPanel : UserControl
    {
        // Forward events from EffectsMenuDropdown
        public event EventHandler? ResizeImageRequested;
        public event EventHandler? ResizeCanvasRequested;
        public event EventHandler? CropImageRequested;
        public event EventHandler? AutoCropImageRequested;
        public event EventHandler? Rotate90CWRequested;
        public event EventHandler? Rotate90CCWRequested;
        public event EventHandler? Rotate180Requested;
        public event EventHandler? RotateCustomAngleRequested;
        public event EventHandler? FlipHorizontalRequested;
        public event EventHandler? FlipVerticalRequested;

        public event EventHandler? BrightnessRequested;
        public event EventHandler? ContrastRequested;
        public event EventHandler? HueRequested;
        public event EventHandler? SaturationRequested;
        public event EventHandler? GammaRequested;
        public event EventHandler? AlphaRequested;
        public event EventHandler? ColorizeRequested;
        public event EventHandler? SelectiveColorRequested;
        public event EventHandler? ReplaceColorRequested;
        public event EventHandler? GrayscaleRequested;
        public event EventHandler? InvertRequested;
        public event EventHandler? BlackAndWhiteRequested;
        public event EventHandler? SepiaRequested;
        public event EventHandler? PolaroidRequested;

        public event EventHandler? BorderRequested;
        public event EventHandler? OutlineRequested;
        public event EventHandler? ShadowRequested;
        public event EventHandler? GlowRequested;
        public event EventHandler? ReflectionRequested;
        public event EventHandler? TornEdgeRequested;
        public event EventHandler? SliceRequested;
        public event EventHandler? RoundedCornersRequested;
        public event EventHandler? SkewRequested;
        public event EventHandler? Rotate3DRequested;
        public event EventHandler? BlurRequested;
        public event EventHandler? PixelateRequested;
        public event EventHandler? SharpenRequested;
        public event EventHandler? ImportPresetRequested;
        public event EventHandler? ExportPresetRequested;

        public EditorToolsPanel()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            WireUpEffectsMenuEvents();
        }

        private void WireUpEffectsMenuEvents()
        {
            var effectsMenu = this.FindControl<EffectsMenuDropdown>("EffectsMenuDropdown");
            if (effectsMenu == null) return;

            // Wire up all events from EffectsMenuDropdown
            effectsMenu.ResizeImageRequested += (s, args) => ResizeImageRequested?.Invoke(this, args);
            effectsMenu.ResizeCanvasRequested += (s, args) => ResizeCanvasRequested?.Invoke(this, args);
            effectsMenu.CropImageRequested += (s, args) => CropImageRequested?.Invoke(this, args);
            effectsMenu.AutoCropImageRequested += (s, args) => AutoCropImageRequested?.Invoke(this, args);
            effectsMenu.Rotate90CWRequested += (s, args) => Rotate90CWRequested?.Invoke(this, args);
            effectsMenu.Rotate90CCWRequested += (s, args) => Rotate90CCWRequested?.Invoke(this, args);
            effectsMenu.Rotate180Requested += (s, args) => Rotate180Requested?.Invoke(this, args);
            effectsMenu.RotateCustomAngleRequested += (s, args) => RotateCustomAngleRequested?.Invoke(this, args);
            effectsMenu.FlipHorizontalRequested += (s, args) => FlipHorizontalRequested?.Invoke(this, args);
            effectsMenu.FlipVerticalRequested += (s, args) => FlipVerticalRequested?.Invoke(this, args);

            effectsMenu.BrightnessRequested += (s, args) => BrightnessRequested?.Invoke(this, args);
            effectsMenu.ContrastRequested += (s, args) => ContrastRequested?.Invoke(this, args);
            effectsMenu.HueRequested += (s, args) => HueRequested?.Invoke(this, args);
            effectsMenu.SaturationRequested += (s, args) => SaturationRequested?.Invoke(this, args);
            effectsMenu.GammaRequested += (s, args) => GammaRequested?.Invoke(this, args);
            effectsMenu.AlphaRequested += (s, args) => AlphaRequested?.Invoke(this, args);
            effectsMenu.ColorizeRequested += (s, args) => ColorizeRequested?.Invoke(this, args);
            effectsMenu.SelectiveColorRequested += (s, args) => SelectiveColorRequested?.Invoke(this, args);
            effectsMenu.ReplaceColorRequested += (s, args) => ReplaceColorRequested?.Invoke(this, args);
            effectsMenu.GrayscaleRequested += (s, args) => GrayscaleRequested?.Invoke(this, args);
            effectsMenu.InvertRequested += (s, args) => InvertRequested?.Invoke(this, args);
            effectsMenu.BlackAndWhiteRequested += (s, args) => BlackAndWhiteRequested?.Invoke(this, args);
            effectsMenu.SepiaRequested += (s, args) => SepiaRequested?.Invoke(this, args);
            effectsMenu.PolaroidRequested += (s, args) => PolaroidRequested?.Invoke(this, args);

            effectsMenu.BorderRequested += (s, args) => BorderRequested?.Invoke(this, args);
            effectsMenu.OutlineRequested += (s, args) => OutlineRequested?.Invoke(this, args);
            effectsMenu.ShadowRequested += (s, args) => ShadowRequested?.Invoke(this, args);
            effectsMenu.GlowRequested += (s, args) => GlowRequested?.Invoke(this, args);
            effectsMenu.ReflectionRequested += (s, args) => ReflectionRequested?.Invoke(this, args);
            effectsMenu.TornEdgeRequested += (s, args) => TornEdgeRequested?.Invoke(this, args);
            effectsMenu.SliceRequested += (s, args) => SliceRequested?.Invoke(this, args);
            effectsMenu.RoundedCornersRequested += (s, args) => RoundedCornersRequested?.Invoke(this, args);
            effectsMenu.SkewRequested += (s, args) => SkewRequested?.Invoke(this, args);
            effectsMenu.Rotate3DRequested += (s, args) => Rotate3DRequested?.Invoke(this, args);
            effectsMenu.BlurRequested += (s, args) => BlurRequested?.Invoke(this, args);
            effectsMenu.PixelateRequested += (s, args) => PixelateRequested?.Invoke(this, args);
            effectsMenu.SharpenRequested += (s, args) => SharpenRequested?.Invoke(this, args);
            effectsMenu.ImportPresetRequested += (s, args) => ImportPresetRequested?.Invoke(this, args);
            effectsMenu.ExportPresetRequested += (s, args) => ExportPresetRequested?.Invoke(this, args);
        }
    }
}
