namespace UETexturePainter.Models;

public enum LayerType
{
    BaseColor,
    Mask,
    Details,
    Emissive,
    Opacity
}

public enum PaintChannel
{
    BaseColor,
    Normal,
    OrmAmbientOcclusion,
    OrmRoughness,
    OrmMetallic,
    Emissive,
    Opacity
}

public enum BlendMode
{
    Normal,
    Multiply,
    Overlay,
    Add,
    Subtract
}

public enum ToolType
{
    Brush,
    Eraser
}
