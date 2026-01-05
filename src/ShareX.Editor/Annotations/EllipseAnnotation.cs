#region License Information (GPL v3)

/*
    ShareX.Editor - The UI-agnostic Editor library for ShareX
    Copyright (c) 2007-2025 ShareX Team

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

using SkiaSharp;

namespace ShareX.Editor.Annotations;

/// <summary>
/// Ellipse/circle annotation
/// </summary>
public class EllipseAnnotation : Annotation
{
    public EllipseAnnotation()
    {
        ToolType = EditorTool.Ellipse;
    }

    public override void Render(SKCanvas canvas)
    {
        var rect = GetBounds();
        using var paint = CreateStrokePaint();
        canvas.DrawOval(rect, paint);
    }

    public override bool HitTest(SKPoint point, float tolerance = 5)
    {
        var rect = GetBounds();
        var centerX = rect.MidX;
        var centerY = rect.MidY;
        var radiusX = rect.Width / 2;
        var radiusY = rect.Height / 2;
        
        if (radiusX <= 0 || radiusY <= 0) return false;
        
        // Normalize point relative to ellipse center
        var dx = (point.X - centerX) / radiusX;
        var dy = (point.Y - centerY) / radiusY;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);
        
        // Check if point is on the ellipse border (within tolerance)
        var toleranceNormalized = tolerance / Math.Min(radiusX, radiusY);
        return Math.Abs(distance - 1.0) <= toleranceNormalized;
    }
}
