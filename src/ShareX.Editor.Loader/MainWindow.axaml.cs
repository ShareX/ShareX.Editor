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

using Avalonia.Controls;
using Avalonia.Threading;
using ShareX.Editor.ViewModels;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareX.Editor.Loader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize ViewModel
            var vm = new MainViewModel();

            // Wire up UploadRequested with a stub handler (standalone mode has no upload capability)
            vm.UploadRequested += (bitmap) =>
            {
                return Task.CompletedTask;
            };

            this.DataContext = vm;

            // Load sample image asynchronously
            Dispatcher.UIThread.Post(() => LoadExampleImage(vm));
        }

        private void LoadExampleImage(MainViewModel vm)
        {
            try
            {
                // Path to the asset - since we set CopyToOutputDirectory, it should be in the bin folder under Assets/
                var location = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(location, "Assets", "Sample.png");

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        // Use SKBitmap.Decode to load file
                        var skBitmap = SKBitmap.Decode(stream);
                        vm.UpdatePreview(skBitmap);
                    }
                }
                else
                {
                    // Fallback to generated if file missing
                    GenerateSampleImage(vm);
                }
            }
            catch (Exception)
            {
                GenerateSampleImage(vm);
            }
        }

        private void GenerateSampleImage(MainViewModel vm)
        {
            // Create a sample SKBitmap
            var width = 800;
            var height = 600;
            var info = new SKImageInfo(width, height);
            var skBitmap = new SKBitmap(info);

            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Transparent);
                using (var paint = new SKPaint { Color = SKColors.LightBlue, IsAntialias = true })
                {
                    canvas.DrawCircle(width / 2, height / 2, 100, paint);
                }
            }
            vm.UpdatePreview(skBitmap);
        }
    }
}