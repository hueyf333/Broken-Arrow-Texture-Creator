using System.Windows.Media;
using UETexturePainter.ViewModels;

namespace UETexturePainter.Models;

public class ColorPickerModel : NotifyBase
{
    private double _hue;
    private double _saturation = 1;
    private double _value = 1;

    public double Hue
    {
        get => _hue;
        set
        {
            if (SetProperty(ref _hue, value))
            {
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    public double Saturation
    {
        get => _saturation;
        set
        {
            if (SetProperty(ref _saturation, value))
            {
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    public double Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    public Color Color
    {
        get
        {
            var c = HsvToColor(Hue, Saturation, Value);
            return c;
        }
    }

    public void SetFromColor(Color color)
    {
        ColorToHsv(color, out var h, out var s, out var v);
        _hue = h;
        _saturation = s;
        _value = v;
        OnPropertyChanged(nameof(Hue));
        OnPropertyChanged(nameof(Saturation));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(Color));
    }

    public static Color HsvToColor(double h, double s, double v)
    {
        var chroma = v * s;
        var hPrime = h / 60.0;
        var x = chroma * (1 - Math.Abs(hPrime % 2 - 1));
        double r = 0;
        double g = 0;
        double b = 0;

        if (hPrime >= 0 && hPrime < 1)
        {
            r = chroma;
            g = x;
        }
        else if (hPrime >= 1 && hPrime < 2)
        {
            r = x;
            g = chroma;
        }
        else if (hPrime >= 2 && hPrime < 3)
        {
            g = chroma;
            b = x;
        }
        else if (hPrime >= 3 && hPrime < 4)
        {
            g = x;
            b = chroma;
        }
        else if (hPrime >= 4 && hPrime < 5)
        {
            r = x;
            b = chroma;
        }
        else if (hPrime >= 5 && hPrime <= 6)
        {
            r = chroma;
            b = x;
        }

        var m = v - chroma;
        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }

    public static void ColorToHsv(Color color, out double h, out double s, out double v)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        h = 0;
        if (delta > 0)
        {
            if (max == r)
            {
                h = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                h = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                h = 60 * (((r - g) / delta) + 4);
            }
        }

        if (h < 0)
        {
            h += 360;
        }

        s = max == 0 ? 0 : delta / max;
        v = max;
    }
}
