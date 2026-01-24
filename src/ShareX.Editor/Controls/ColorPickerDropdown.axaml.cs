using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ShareX.Editor.Controls
{
    public partial class ColorPickerDropdown : UserControl
    {
        public static readonly StyledProperty<IBrush> SelectedColorProperty =
            AvaloniaProperty.Register<ColorPickerDropdown, IBrush>(
                nameof(SelectedColor),
                defaultValue: Brushes.Red);

        public static readonly StyledProperty<Color> SelectedColorValueProperty =
            AvaloniaProperty.Register<ColorPickerDropdown, Color>(
                nameof(SelectedColorValue),
                defaultValue: Colors.Red);

        public static readonly StyledProperty<IEnumerable<IBrush>> ColorPaletteProperty =
            AvaloniaProperty.Register<ColorPickerDropdown, IEnumerable<IBrush>>(
                nameof(ColorPalette),
                defaultValue: GetDefaultColorPalette());

        public IBrush SelectedColor
        {
            get => GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public Color SelectedColorValue
        {
            get => GetValue(SelectedColorValueProperty);
            set => SetValue(SelectedColorValueProperty, value);
        }

        public IEnumerable<IBrush> ColorPalette
        {
            get => GetValue(ColorPaletteProperty);
            set => SetValue(ColorPaletteProperty, value);
        }

        public event EventHandler<IBrush>? ColorChanged;

        static ColorPickerDropdown()
        {
            // Sync SelectedColor -> SelectedColorValue
            SelectedColorProperty.Changed.AddClassHandler<ColorPickerDropdown>((s, e) =>
            {
                if (e.NewValue is SolidColorBrush brush)
                {
                    s.SelectedColorValue = brush.Color;
                }
            });

            // Sync SelectedColorValue -> SelectedColor
            SelectedColorValueProperty.Changed.AddClassHandler<ColorPickerDropdown>((s, e) =>
            {
                if (e.NewValue is Color color)
                {
                    var newBrush = new SolidColorBrush(color);
                    s.SelectedColor = newBrush;
                    s.ColorChanged?.Invoke(s, newBrush);
                }
            });
        }

        public ColorPickerDropdown()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnDropdownButtonClick(object? sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("ColorPopup");
            if (popup != null)
            {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void OnColorSelected(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is IBrush selectedColor)
            {
                SelectedColor = selectedColor;
                ColorChanged?.Invoke(this, selectedColor);

                // Close the popup
                var popup = this.FindControl<Popup>("ColorPopup");
                if (popup != null)
                {
                    popup.IsOpen = false;
                }
            }
        }

        private static IEnumerable<IBrush> GetDefaultColorPalette()
        {
            // Extended color palette with 32 colors (8x4 grid)
            return new List<IBrush>
            {
                // Row 1: Reds to Oranges
                new SolidColorBrush(Color.Parse("#EF4444")),
                new SolidColorBrush(Color.Parse("#DC2626")),
                new SolidColorBrush(Color.Parse("#F97316")),
                new SolidColorBrush(Color.Parse("#EA580C")),
                new SolidColorBrush(Color.Parse("#F59E0B")),
                new SolidColorBrush(Color.Parse("#EAB308")),
                new SolidColorBrush(Color.Parse("#84CC16")),
                new SolidColorBrush(Color.Parse("#22C55E")),
                
                // Row 2: Greens to Blues
                new SolidColorBrush(Color.Parse("#10B981")),
                new SolidColorBrush(Color.Parse("#14B8A6")),
                new SolidColorBrush(Color.Parse("#06B6D4")),
                new SolidColorBrush(Color.Parse("#0EA5E9")),
                new SolidColorBrush(Color.Parse("#3B82F6")),
                new SolidColorBrush(Color.Parse("#6366F1")),
                new SolidColorBrush(Color.Parse("#8B5CF6")),
                new SolidColorBrush(Color.Parse("#A855F7")),
                
                // Row 3: Purples to Pinks
                new SolidColorBrush(Color.Parse("#D946EF")),
                new SolidColorBrush(Color.Parse("#EC4899")),
                new SolidColorBrush(Color.Parse("#F43F5E")),
                new SolidColorBrush(Color.Parse("#FB7185")),
                new SolidColorBrush(Color.Parse("#FDA4AF")),
                new SolidColorBrush(Color.Parse("#FCD34D")),
                new SolidColorBrush(Color.Parse("#A3E635")),
                new SolidColorBrush(Color.Parse("#4ADE80")),
                
                // Row 4: Neutrals
                new SolidColorBrush(Color.Parse("#FFFFFF")),
                new SolidColorBrush(Color.Parse("#F1F5F9")),
                new SolidColorBrush(Color.Parse("#CBD5E1")),
                new SolidColorBrush(Color.Parse("#94A3B8")),
                new SolidColorBrush(Color.Parse("#64748B")),
                new SolidColorBrush(Color.Parse("#475569")),
                new SolidColorBrush(Color.Parse("#1E293B")),
                new SolidColorBrush(Color.Parse("#000000"))
            };
        }
    }
}
