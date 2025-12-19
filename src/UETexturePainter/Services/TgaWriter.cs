using System.IO;
using System.Windows.Media.Imaging;

namespace UETexturePainter.Services;

public static class TgaWriter
{
    public static void Write(string path, WriteableBitmap bitmap)
    {
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        writer.Write((byte)0); // ID length
        writer.Write((byte)0); // Color map type
        writer.Write((byte)2); // Uncompressed true-color image
        writer.Write((short)0); // Color map origin
        writer.Write((short)0); // Color map length
        writer.Write((byte)0); // Color map depth
        writer.Write((short)0); // X origin
        writer.Write((short)0); // Y origin
        writer.Write((short)width);
        writer.Write((short)height);
        writer.Write((byte)32); // Bits per pixel
        writer.Write((byte)0x20); // Image descriptor (top-left origin)

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var index = (y * stride) + x * 4;
                writer.Write(pixels[index]);
                writer.Write(pixels[index + 1]);
                writer.Write(pixels[index + 2]);
                writer.Write(pixels[index + 3]);
            }
        }
    }
}
