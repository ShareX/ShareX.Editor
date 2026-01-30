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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShareX.Editor.Controls;

namespace ShareX.Editor.Views.Controls
{
    /// <summary>
    /// Reusable annotation toolbar control containing drawing tools, property controls, and actions.
    /// Can be used in both the Image Editor and Region Capture Overlay.
    /// Provides an ExtensionContent slot for injecting additional tools (e.g., EditorToolsPanel).
    /// </summary>
    public partial class AnnotationToolbar : UserControl
    {
        /// <summary>
        /// Styled property for the extension content slot.
        /// Allows injection of additional controls (e.g., EditorToolsPanel) into the toolbar.
        /// </summary>
        public static readonly StyledProperty<object?> ExtensionContentProperty =
            AvaloniaProperty.Register<AnnotationToolbar, object?>(nameof(ExtensionContent));

        /// <summary>
        /// Gets or sets the extension content to be displayed in the toolbar.
        /// Used to inject EditorToolsPanel when in Editor mode.
        /// </summary>
        public object? ExtensionContent
        {
            get => GetValue(ExtensionContentProperty);
            set => SetValue(ExtensionContentProperty, value);
        }

        /// <summary>
        /// Event raised when the border color is changed.
        /// </summary>
        public event EventHandler<IBrush>? ColorChanged;

        /// <summary>
        /// Event raised when the fill color is changed.
        /// </summary>
        public event EventHandler<IBrush>? FillColorChanged;

        /// <summary>
        /// Event raised when the stroke width is changed.
        /// </summary>
        public event EventHandler<int>? WidthChanged;

        /// <summary>
        /// Event raised when the font size is changed.
        /// </summary>
        public event EventHandler<float>? FontSizeChanged;

        /// <summary>
        /// Event raised when the effect strength is changed.
        /// </summary>
        public event EventHandler<float>? StrengthChanged;

        /// <summary>
        /// Event raised when the shadow toggle button is clicked.
        /// </summary>
        public event EventHandler? ShadowButtonClick;

        public AnnotationToolbar()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            WireUpEventHandlers();
            UpdateToolbarScrollPadding();
        }

        private void WireUpEventHandlers()
        {
            var colorPicker = this.FindControl<ColorPickerDropdown>("ColorPickerDropdown");
            if (colorPicker != null)
            {
                colorPicker.ColorChanged += (s, color) => ColorChanged?.Invoke(s, color);
            }

            var fillColorPicker = this.FindControl<ColorPickerDropdown>("FillColorPickerDropdown");
            if (fillColorPicker != null)
            {
                fillColorPicker.ColorChanged += (s, color) => FillColorChanged?.Invoke(s, color);
            }

            var widthPicker = this.FindControl<WidthPickerDropdown>("WidthPickerDropdown");
            if (widthPicker != null)
            {
                widthPicker.WidthChanged += (s, width) => WidthChanged?.Invoke(s, width);
            }

            var fontSizePicker = this.FindControl<FontSizePickerDropdown>("FontSizePickerDropdown");
            if (fontSizePicker != null)
            {
                fontSizePicker.FontSizeChanged += (s, size) => FontSizeChanged?.Invoke(s, size);
            }

            var strengthSlider = this.FindControl<StrengthSlider>("StrengthSlider");
            if (strengthSlider != null)
            {
                strengthSlider.StrengthChanged += (s, strength) => StrengthChanged?.Invoke(s, strength);
            }

            var scrollViewer = this.FindControl<ScrollViewer>("ToolbarScrollViewer");
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += OnToolbarScrollChanged;
            }
        }

        private void OnToolbarScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateToolbarScrollPadding();
        }

        private void UpdateToolbarScrollPadding()
        {
            var scrollViewer = this.FindControl<ScrollViewer>("ToolbarScrollViewer");
            if (scrollViewer == null) return;

            var hasHorizontalOverflow = scrollViewer.Extent.Width - scrollViewer.Viewport.Width > 0.5;
            scrollViewer.Padding = hasHorizontalOverflow ? new Thickness(0, 0, 0, 8) : new Thickness(0);
        }

        private void OnShadowButtonClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            ShadowButtonClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
