using System.Windows.Media.Imaging;
using UETexturePainter.Models;

namespace UETexturePainter.Services;

public class BitmapSwapAction : IUndoableAction
{
    private readonly LayerModel _layer;
    private readonly WriteableBitmap _before;
    private readonly WriteableBitmap _after;

    public BitmapSwapAction(string description, LayerModel layer, WriteableBitmap before, WriteableBitmap after)
    {
        Description = description;
        _layer = layer;
        _before = before;
        _after = after;
    }

    public string Description { get; }

    public void Undo() => _layer.Bitmap = _before;

    public void Redo() => _layer.Bitmap = _after;
}

public class LayerReorderAction : IUndoableAction
{
    private readonly IList<LayerModel> _layers;
    private readonly List<LayerModel> _before;
    private readonly List<LayerModel> _after;

    public LayerReorderAction(string description, IList<LayerModel> layers, IEnumerable<LayerModel> before, IEnumerable<LayerModel> after)
    {
        Description = description;
        _layers = layers;
        _before = before.ToList();
        _after = after.ToList();
    }

    public string Description { get; }

    public void Undo()
    {
        _layers.Clear();
        foreach (var layer in _before)
        {
            _layers.Add(layer);
        }
    }

    public void Redo()
    {
        _layers.Clear();
        foreach (var layer in _after)
        {
            _layers.Add(layer);
        }
    }
}
