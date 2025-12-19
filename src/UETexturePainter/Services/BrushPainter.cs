using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UETexturePainter.Models;

namespace UETexturePainter.Services;

public class BrushPainter
{
    public void PaintStroke(LayerModel layer, BrushSettings brush, Color color, Point from, Point to, int canvasWidth, int canvasHeight)
    {
        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return;
        }

        var scaleX = layer.Bitmap.PixelWidth / (double)canvasWidth;
        var scaleY = layer.Bitmap.PixelHeight / (double)canvasHeight;
        var start = new Point(from.X * scaleX, from.Y * scaleY);
        var end = new Point(to.X * scaleX, to.Y * scaleY);
        var distance = (end - start).Length;
        var step = Math.Max(1, brush.Size * brush.Flow * 0.2);
        var steps = Math.Max(1, (int)(distance / step));

        for (var i = 0; i <= steps; i++)
        {
            var t = steps == 0 ? 1.0 : i / (double)steps;
            var position = new Point(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t);
            DrawBrush(layer.Bitmap, brush, color, position);
        }
    }

    private static void DrawBrush(WriteableBitmap bitmap, BrushSettings brush, Color color, Point position)
    {
        var radius = brush.Size / 2.0;
        var left = (int)Math.Max(0, position.X - radius);
        var top = (int)Math.Max(0, position.Y - radius);
        var right = (int)Math.Min(bitmap.PixelWidth - 1, position.X + radius);
        var bottom = (int)Math.Min(bitmap.PixelHeight - 1, position.Y + radius);
        var stride = bitmap.PixelWidth * 4;
        var buffer = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(buffer, stride, 0);

        for (var y = top; y <= bottom; y++)
        {
            for (var x = left; x <= right; x++)
            {
                var dx = x - position.X;
                var dy = y - position.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance > radius)
                {
                    continue;
                }

                var strength = 1 - (distance / radius);
                var hardnessBoost = Math.Pow(strength, Math.Max(0.01, 1 - brush.Hardness));
                var alpha = brush.Opacity * hardnessBoost;

                var index = (y * stride) + x * 4;
                var dstB = buffer[index];
                var dstG = buffer[index + 1];
                var dstR = buffer[index + 2];
                var dstA = buffer[index + 3] / 255.0;

                var srcA = (color.A / 255.0) * alpha;
                var srcR = color.R;
                var srcG = color.G;
                var srcB = color.B;

                ApplyBlend(brush.BlendMode, srcR, srcG, srcB, ref dstR, ref dstG, ref dstB);

                var outA = srcA + dstA * (1 - srcA);
                if (outA <= 0)
                {
                    continue;
                }

                var outR = (byte)((srcR * srcA + dstR * dstA * (1 - srcA)) / outA);
                var outG = (byte)((srcG * srcA + dstG * dstA * (1 - srcA)) / outA);
                var outB = (byte)((srcB * srcA + dstB * dstA * (1 - srcA)) / outA);

                buffer[index] = outB;
                buffer[index + 1] = outG;
                buffer[index + 2] = outR;
                buffer[index + 3] = (byte)(outA * 255);
            }
        }

        bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), buffer, stride, 0);
    }

    private static void ApplyBlend(BlendMode mode, byte srcR, byte srcG, byte srcB, ref byte dstR, ref byte dstG, ref byte dstB)
    {
        switch (mode)
        {
            case BlendMode.Multiply:
                dstR = (byte)(dstR * srcR / 255);
                dstG = (byte)(dstG * srcG / 255);
                dstB = (byte)(dstB * srcB / 255);
                break;
            case BlendMode.Overlay:
                dstR = Overlay(dstR, srcR);
                dstG = Overlay(dstG, srcG);
                dstB = Overlay(dstB, srcB);
                break;
            case BlendMode.Add:
                dstR = (byte)Math.Min(255, dstR + srcR);
                dstG = (byte)Math.Min(255, dstG + srcG);
                dstB = (byte)Math.Min(255, dstB + srcB);
                break;
            case BlendMode.Subtract:
                dstR = (byte)Math.Max(0, dstR - srcR);
                dstG = (byte)Math.Max(0, dstG - srcG);
                dstB = (byte)Math.Max(0, dstB - srcB);
                break;
            default:
                break;
        }
    }

    private static byte Overlay(byte dst, byte src)
    {
        var dstNorm = dst / 255.0;
        var srcNorm = src / 255.0;
        var result = dstNorm < 0.5
            ? (2 * dstNorm * srcNorm)
            : (1 - 2 * (1 - dstNorm) * (1 - srcNorm));
        return (byte)(result * 255);
    }
}
