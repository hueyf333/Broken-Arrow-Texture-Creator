using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UETexturePainter.Models;

public class LayerStack
{
    public LayerStack(int width, int height)
    {
        Width = width;
        Height = height;
        Layers = new ObservableCollection<LayerModel>();
    }

    public int Width { get; }
    public int Height { get; }
    public ObservableCollection<LayerModel> Layers { get; }

    public WriteableBitmap Compose()
    {
        var composite = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
        var stride = Width * 4;
        var buffer = new byte[stride * Height];

        foreach (var layer in Layers)
        {
            if (!layer.IsVisible)
            {
                continue;
            }

            var layerBuffer = new byte[stride * Height];
            layer.Bitmap.CopyPixels(layerBuffer, stride, 0);
            BlendBuffers(buffer, layerBuffer, layer.Opacity);
        }

        composite.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), buffer, stride, 0);
        return composite;
    }

    private static void BlendBuffers(byte[] target, byte[] source, double opacity)
    {
        var alphaMultiplier = opacity;
        for (int i = 0; i < target.Length; i += 4)
        {
            var srcA = source[i + 3] / 255.0 * alphaMultiplier;
            if (srcA <= 0.0)
            {
                continue;
            }

            var dstA = target[i + 3] / 255.0;
            var outA = srcA + dstA * (1 - srcA);
            if (outA <= 0)
            {
                continue;
            }

            target[i] = (byte)((source[i] * srcA + target[i] * dstA * (1 - srcA)) / outA);
            target[i + 1] = (byte)((source[i + 1] * srcA + target[i + 1] * dstA * (1 - srcA)) / outA);
            target[i + 2] = (byte)((source[i + 2] * srcA + target[i + 2] * dstA * (1 - srcA)) / outA);
            target[i + 3] = (byte)(outA * 255);
        }
    }
}
