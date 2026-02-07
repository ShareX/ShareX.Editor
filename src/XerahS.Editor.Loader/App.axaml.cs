#region License Information (GPL v3)

/*
    ShareX.Editor - The UI-agnostic Editor library for ShareX
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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using XerahS.Editor;
using XerahS.Editor.ViewModels;
using XerahS.Editor.Views;
using SkiaSharp;
using System;
using System.IO;

namespace XerahS.Editor.Loader
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var options = new EditorOptions();
                var window = new EditorWindow(options);
                desktop.MainWindow = window;

                if (window.DataContext is MainViewModel vm)
                {
                    LoadExampleImage(vm);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void LoadExampleImage(MainViewModel vm)
        {
            try
            {
                string location = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(location, "Assets", "Sample.png");

                if (File.Exists(path))
                {
                    using FileStream stream = File.OpenRead(path);
                    SKBitmap? skBitmap = SKBitmap.Decode(stream);
                    if (skBitmap != null)
                    {
                        vm.UpdatePreview(skBitmap);
                        return;
                    }
                }
            }
            catch
            {
                // Fall through to generated image.
            }

            GenerateSampleImage(vm);
        }

        private static void GenerateSampleImage(MainViewModel vm)
        {
            const int width = 800;
            const int height = 600;
            var info = new SKImageInfo(width, height);
            var skBitmap = new SKBitmap(info);

            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Transparent);
                using var paint = new SKPaint { Color = SKColors.LightBlue, IsAntialias = true };
                canvas.DrawCircle(width / 2, height / 2, 100, paint);
            }

            vm.UpdatePreview(skBitmap);
        }
    }
}
