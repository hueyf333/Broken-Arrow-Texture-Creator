using System.Windows.Media.Imaging;
using UETexturePainter.ViewModels;

namespace UETexturePainter.Models;

public class LayerModel : NotifyBase
{
    private string _name;
    private double _opacity = 1.0;
    private bool _isVisible = true;
    private WriteableBitmap _bitmap;

    public LayerModel(string name, LayerType type, WriteableBitmap bitmap)
    {
        _name = name;
        Type = type;
        _bitmap = bitmap;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public LayerType Type { get; }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public WriteableBitmap Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }
}
