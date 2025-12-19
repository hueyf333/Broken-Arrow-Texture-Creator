using UETexturePainter.ViewModels;

namespace UETexturePainter.Models;

public class BrushSettings : NotifyBase
{
    private double _size = 64;
    private double _hardness = 0.8;
    private double _opacity = 1.0;
    private double _flow = 0.8;
    private BlendMode _blendMode = BlendMode.Normal;

    public double Size
    {
        get => _size;
        set => SetProperty(ref _size, value);
    }

    public double Hardness
    {
        get => _hardness;
        set => SetProperty(ref _hardness, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public double Flow
    {
        get => _flow;
        set => SetProperty(ref _flow, value);
    }

    public BlendMode BlendMode
    {
        get => _blendMode;
        set => SetProperty(ref _blendMode, value);
    }
}
