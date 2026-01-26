using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ShareX.Editor.Controls
{
    public partial class FontSizePickerDropdown : UserControl
    {
        public static readonly StyledProperty<float> SelectedFontSizeProperty =
            AvaloniaProperty.Register<FontSizePickerDropdown, float>(
                nameof(SelectedFontSize),
                defaultValue: 24);

        public static readonly StyledProperty<IEnumerable<float>> FontSizeOptionsProperty =
            AvaloniaProperty.Register<FontSizePickerDropdown, IEnumerable<float>>(
                nameof(FontSizeOptions),
                defaultValue: GetDefaultFontSizes());

        public float SelectedFontSize
        {
            get => GetValue(SelectedFontSizeProperty);
            set => SetValue(SelectedFontSizeProperty, value);
        }

        public IEnumerable<float> FontSizeOptions
        {
            get => GetValue(FontSizeOptionsProperty);
            set => SetValue(FontSizeOptionsProperty, value);
        }

        public event EventHandler<float>? FontSizeChanged;

        public FontSizePickerDropdown()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnDropdownButtonClick(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("FontSizePopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void OnFontSizeSelected(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is float selectedSize)
            {
                SelectedFontSize = selectedSize;
                FontSizeChanged?.Invoke(this, selectedSize);

                // Close the popup
                var popup = this.FindControl<Popup>("FontSizePopup");
                if (popup != null)
                {
                    popup.IsOpen = false;
                }
            }
        }

        private static IEnumerable<float> GetDefaultFontSizes()
        {
            return new List<float> { 10, 13, 16, 20, 24, 30, 48, 72, 96, 144, 216, 288 };
        }
    }
}
