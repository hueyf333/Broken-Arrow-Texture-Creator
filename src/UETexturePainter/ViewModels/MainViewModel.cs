using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using UETexturePainter.Models;
using UETexturePainter.Services;

namespace UETexturePainter.ViewModels;

public class MainViewModel : NotifyBase
{
    private readonly BrushPainter _brushPainter = new();
    private readonly UndoRedoStack _undoRedoStack = new();
    private readonly ExportService _exportService = new();
    private readonly MaterialScriptGenerator _scriptGenerator = new();

    private LayerStack _baseColorStack;
    private LayerStack _normalStack;
    private LayerStack _ormAoStack;
    private LayerStack _ormRoughnessStack;
    private LayerStack _ormMetallicStack;
    private LayerStack _emissiveStack;
    private LayerStack _opacityStack;

    private WriteableBitmap? _compositeBitmap;
    private WriteableBitmap? _uvOverlay;
    private PaintChannel _selectedChannel;
    private ToolType _selectedTool = ToolType.Brush;
    private string _assetName = "NewAsset";
    private string _targetFolder = "Textures";
    private string _exportFormat = "png";
    private int _canvasWidth = 1024;
    private int _canvasHeight = 1024;
    private int _selectedResolution = 1024;
    private Point _lastPoint;
    private bool _isPainting;

    public MainViewModel()
    {
        Brush = new BrushSettings();
        ColorPicker = new ColorPickerModel();
        ColorPicker.PropertyChanged += (_, _) => UpdateBrushColor();
        Brush.PropertyChanged += (_, _) => UpdateBrushColor();

        ToolOptions = new ObservableCollection<ToolType>((ToolType[])Enum.GetValues(typeof(ToolType)));
        ChannelOptions = new ObservableCollection<PaintChannel>((PaintChannel[])Enum.GetValues(typeof(PaintChannel)));
        BlendModeOptions = new ObservableCollection<BlendMode>((BlendMode[])Enum.GetValues(typeof(BlendMode)));
        ResolutionOptions = new ObservableCollection<int>(new[] { 512, 1024, 2048, 4096 });
        ExportFormatOptions = new ObservableCollection<string>(new[] { "png", "tga" });
        PaletteColors = new ObservableCollection<Color>(new[]
        {
            Colors.White,
            Colors.Black,
            Color.FromRgb(255, 85, 85),
            Color.FromRgb(255, 153, 51),
            Color.FromRgb(255, 235, 59),
            Color.FromRgb(76, 175, 80),
            Color.FromRgb(33, 150, 243),
            Color.FromRgb(156, 39, 176)
        });

        _selectedChannel = PaintChannel.BaseColor;
        _selectedResolution = 1024;

        _baseColorStack = CreateDefaultStack("BaseColor");
        _normalStack = CreateDefaultStack("Normal", new Color?((Color)ColorConverter.ConvertFromString("#FF7F7FFF")));
        _ormAoStack = CreateDefaultStack("AO", Colors.White);
        _ormRoughnessStack = CreateDefaultStack("Roughness", Colors.White);
        _ormMetallicStack = CreateDefaultStack("Metallic", Colors.Black);
        _emissiveStack = CreateDefaultStack("Emissive", Colors.Black);
        _opacityStack = CreateDefaultStack("Opacity", Colors.White);

        HookLayerEvents(_baseColorStack);
        HookLayerEvents(_normalStack);
        HookLayerEvents(_ormAoStack);
        HookLayerEvents(_ormRoughnessStack);
        HookLayerEvents(_ormMetallicStack);
        HookLayerEvents(_emissiveStack);
        HookLayerEvents(_opacityStack);

        AddLayerCommand = new RelayCommand(_ => AddLayer());
        MoveLayerUpCommand = new RelayCommand(param => MoveLayer(param as LayerModel, -1));
        MoveLayerDownCommand = new RelayCommand(param => MoveLayer(param as LayerModel, 1));
        MergeLayerDownCommand = new RelayCommand(param => MergeLayerDown(param as LayerModel));
        UndoCommand = new RelayCommand(_ => Undo());
        RedoCommand = new RelayCommand(_ => Redo());
        ExportCommand = new RelayCommand(_ => Export());
        GenerateScriptCommand = new RelayCommand(_ => GenerateScript());
        ImportTextureCommand = new RelayCommand(_ => ImportTexture());
        LoadUvCommand = new RelayCommand(_ => LoadUvOverlay());
        SelectPaletteColorCommand = new RelayCommand(SelectPaletteColor);

        UpdateComposite();
        UpdateBrushColor();
    }

    public BrushSettings Brush { get; }
    public ColorPickerModel ColorPicker { get; }

    public ObservableCollection<ToolType> ToolOptions { get; }
    public ObservableCollection<PaintChannel> ChannelOptions { get; }
    public ObservableCollection<BlendMode> BlendModeOptions { get; }
    public ObservableCollection<int> ResolutionOptions { get; }
    public ObservableCollection<string> ExportFormatOptions { get; }
    public ObservableCollection<Color> PaletteColors { get; }

    public RelayCommand AddLayerCommand { get; }
    public RelayCommand MoveLayerUpCommand { get; }
    public RelayCommand MoveLayerDownCommand { get; }
    public RelayCommand MergeLayerDownCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand RedoCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand GenerateScriptCommand { get; }
    public RelayCommand ImportTextureCommand { get; }
    public RelayCommand LoadUvCommand { get; }
    public RelayCommand SelectPaletteColorCommand { get; }

    public PaintChannel SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            if (SetProperty(ref _selectedChannel, value))
            {
                UpdateComposite();
            }
        }
    }

    public ToolType SelectedTool
    {
        get => _selectedTool;
        set => SetProperty(ref _selectedTool, value);
    }

    public string AssetName
    {
        get => _assetName;
        set => SetProperty(ref _assetName, value);
    }

    public string TargetFolder
    {
        get => _targetFolder;
        set => SetProperty(ref _targetFolder, value);
    }

    public string ExportFormat
    {
        get => _exportFormat;
        set => SetProperty(ref _exportFormat, value);
    }

    public int SelectedResolution
    {
        get => _selectedResolution;
        set
        {
            if (SetProperty(ref _selectedResolution, value))
            {
                ResizeCanvas(value, value);
            }
        }
    }

    public WriteableBitmap? CompositeBitmap
    {
        get => _compositeBitmap;
        private set => SetProperty(ref _compositeBitmap, value);
    }

    public WriteableBitmap? UvOverlay
    {
        get => _uvOverlay;
        set => SetProperty(ref _uvOverlay, value);
    }

    public SolidColorBrush BrushColorBrush => new(ColorPicker.Color);

    public LayerStack ActiveLayerStack => SelectedChannel switch
    {
        PaintChannel.BaseColor => _baseColorStack,
        PaintChannel.Normal => _normalStack,
        PaintChannel.OrmAmbientOcclusion => _ormAoStack,
        PaintChannel.OrmRoughness => _ormRoughnessStack,
        PaintChannel.OrmMetallic => _ormMetallicStack,
        PaintChannel.Emissive => _emissiveStack,
        PaintChannel.Opacity => _opacityStack,
        _ => _baseColorStack
    };

    public void UpdateCanvasSize(double width, double height)
    {
        _canvasWidth = Math.Max(1, (int)Math.Round(width));
        _canvasHeight = Math.Max(1, (int)Math.Round(height));
    }

    public void BeginStroke(Point position)
    {
        _lastPoint = position;
        _isPainting = true;
    }

    public void ContinueStroke(Point position)
    {
        if (!_isPainting)
        {
            return;
        }

        ApplyStroke(_lastPoint, position);
        _lastPoint = position;
    }

    public void EndStroke(Point position)
    {
        if (!_isPainting)
        {
            return;
        }

        ApplyStroke(_lastPoint, position);
        _isPainting = false;
    }

    private void ApplyStroke(Point from, Point to)
    {
        var layer = ActiveLayerStack.Layers.LastOrDefault();
        if (layer is null)
        {
            return;
        }

        var before = CloneBitmap(layer.Bitmap);
        var paintColor = GetChannelPaintColor();
        if (SelectedTool == ToolType.Eraser)
        {
            paintColor = Colors.Transparent;
        }

        _brushPainter.PaintStroke(layer, Brush, paintColor, from, to, _canvasWidth, _canvasHeight);
        var after = CloneBitmap(layer.Bitmap);
        _undoRedoStack.Execute(new BitmapSwapAction("Stroke", layer, before, after));
        UpdateComposite();
    }

    private void AddLayer()
    {
        var layer = CreateLayer($"Layer {ActiveLayerStack.Layers.Count + 1}", ActiveLayerStack.Width, ActiveLayerStack.Height);
        ActiveLayerStack.Layers.Add(layer);
        UpdateComposite();
    }

    private void MoveLayer(LayerModel? layer, int direction)
    {
        if (layer is null)
        {
            return;
        }

        var index = ActiveLayerStack.Layers.IndexOf(layer);
        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= ActiveLayerStack.Layers.Count)
        {
            return;
        }

        var before = ActiveLayerStack.Layers.ToList();
        ActiveLayerStack.Layers.Move(index, newIndex);
        var after = ActiveLayerStack.Layers.ToList();
        _undoRedoStack.Execute(new LayerReorderAction("Reorder Layer", ActiveLayerStack.Layers, before, after));
        UpdateComposite();
    }

    private void MergeLayerDown(LayerModel? layer)
    {
        if (layer is null)
        {
            return;
        }

        var index = ActiveLayerStack.Layers.IndexOf(layer);
        if (index <= 0)
        {
            return;
        }

        var below = ActiveLayerStack.Layers[index - 1];
        var before = CloneBitmap(below.Bitmap);
        BlendLayers(below, layer);
        ActiveLayerStack.Layers.Remove(layer);
        var after = CloneBitmap(below.Bitmap);
        _undoRedoStack.Execute(new BitmapSwapAction("Merge Layer", below, before, after));
        UpdateComposite();
    }

    private void BlendLayers(LayerModel target, LayerModel source)
    {
        var stride = target.Bitmap.PixelWidth * 4;
        var buffer = new byte[stride * target.Bitmap.PixelHeight];
        var sourceBuffer = new byte[stride * target.Bitmap.PixelHeight];
        target.Bitmap.CopyPixels(buffer, stride, 0);
        source.Bitmap.CopyPixels(sourceBuffer, stride, 0);

        var opacity = source.Opacity;
        for (var i = 0; i < buffer.Length; i += 4)
        {
            var srcA = sourceBuffer[i + 3] / 255.0 * opacity;
            var dstA = buffer[i + 3] / 255.0;
            var outA = srcA + dstA * (1 - srcA);
            if (outA <= 0)
            {
                continue;
            }

            buffer[i] = (byte)((sourceBuffer[i] * srcA + buffer[i] * dstA * (1 - srcA)) / outA);
            buffer[i + 1] = (byte)((sourceBuffer[i + 1] * srcA + buffer[i + 1] * dstA * (1 - srcA)) / outA);
            buffer[i + 2] = (byte)((sourceBuffer[i + 2] * srcA + buffer[i + 2] * dstA * (1 - srcA)) / outA);
            buffer[i + 3] = (byte)(outA * 255);
        }

        target.Bitmap.WritePixels(new Int32Rect(0, 0, target.Bitmap.PixelWidth, target.Bitmap.PixelHeight), buffer, stride, 0);
    }

    private void Undo()
    {
        _undoRedoStack.Undo();
        UpdateComposite();
    }

    private void Redo()
    {
        _undoRedoStack.Redo();
        UpdateComposite();
    }

    private void Export()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }

        var baseColor = _baseColorStack.Compose();
        var normal = _normalStack.Compose();
        var orm = ComposeOrm();
        var emissive = _emissiveStack.Layers.Count > 0 ? _emissiveStack.Compose() : null;
        var opacity = _opacityStack.Layers.Count > 0 ? _opacityStack.Compose() : null;

        var request = new ExportRequest(
            AssetName,
            dialog.SelectedPath,
            TargetFolder,
            ExportFormat,
            baseColor,
            normal,
            orm,
            emissive,
            opacity);

        _exportService.ExportTextureSet(request);
        _scriptGenerator.SaveTo(dialog.SelectedPath, AssetName, TargetFolder, ExportFormat);
    }

    private void GenerateScript()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"CreateMaterial_{AssetName}.py",
            Filter = "Python Script (*.py)|*.py"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var outputFolder = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
        var script = _scriptGenerator.Generate(AssetName, TargetFolder, outputFolder, ExportFormat);
        File.WriteAllText(dialog.FileName, script);
    }

    private void ImportTexture()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png)|*.png"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var bitmap = new BitmapImage(new Uri(dialog.FileName));
        var wb = ResizeBitmap(bitmap, ActiveLayerStack.Width, ActiveLayerStack.Height);
        ActiveLayerStack.Layers.Add(new LayerModel($"Imported {Path.GetFileName(dialog.FileName)}", LayerType.Details, wb));
        UpdateComposite();
    }

    private void LoadUvOverlay()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png)|*.png"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var bitmap = new BitmapImage(new Uri(dialog.FileName));
        UvOverlay = new WriteableBitmap(bitmap);
    }

    private void ResizeCanvas(int width, int height)
    {
        _baseColorStack = CreateDefaultStack("BaseColor", width, height);
        _normalStack = CreateDefaultStack("Normal", width, height, (Color)ColorConverter.ConvertFromString("#FF7F7FFF"));
        _ormAoStack = CreateDefaultStack("AO", width, height, Colors.White);
        _ormRoughnessStack = CreateDefaultStack("Roughness", width, height, Colors.White);
        _ormMetallicStack = CreateDefaultStack("Metallic", width, height, Colors.Black);
        _emissiveStack = CreateDefaultStack("Emissive", width, height, Colors.Black);
        _opacityStack = CreateDefaultStack("Opacity", width, height, Colors.White);
        HookLayerEvents(_baseColorStack);
        HookLayerEvents(_normalStack);
        HookLayerEvents(_ormAoStack);
        HookLayerEvents(_ormRoughnessStack);
        HookLayerEvents(_ormMetallicStack);
        HookLayerEvents(_emissiveStack);
        HookLayerEvents(_opacityStack);
        UpdateComposite();
    }

    private LayerStack CreateDefaultStack(string name, int? width = null, int? height = null, Color? baseColor = null)
    {
        var stackWidth = width ?? _selectedResolution;
        var stackHeight = height ?? _selectedResolution;
        var stack = new LayerStack(stackWidth, stackHeight);
        var layer = CreateLayer($"{name} Base", stackWidth, stackHeight, baseColor);
        stack.Layers.Add(layer);
        return stack;
    }

    private LayerModel CreateLayer(string name, int width, int height, Color? fillColor = null)
    {
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        if (fillColor.HasValue)
        {
            FillBitmap(bitmap, fillColor.Value);
        }

        return new LayerModel(name, LayerType.BaseColor, bitmap);
    }

    private void HookLayerEvents(LayerStack stack)
    {
        stack.Layers.CollectionChanged += OnLayersChanged;
        foreach (var layer in stack.Layers)
        {
            layer.PropertyChanged += (_, _) => UpdateComposite();
        }
    }

    private void OnLayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (LayerModel layer in e.NewItems)
            {
                layer.PropertyChanged += (_, _) => UpdateComposite();
            }
        }

        UpdateComposite();
    }

    private void FillBitmap(WriteableBitmap bitmap, Color color)
    {
        var stride = bitmap.PixelWidth * 4;
        var buffer = new byte[stride * bitmap.PixelHeight];
        for (var i = 0; i < buffer.Length; i += 4)
        {
            buffer[i] = color.B;
            buffer[i + 1] = color.G;
            buffer[i + 2] = color.R;
            buffer[i + 3] = color.A;
        }
        bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), buffer, stride, 0);
    }

    private static WriteableBitmap ResizeBitmap(BitmapSource source, int width, int height)
    {
        if (source.PixelWidth == width && source.PixelHeight == height)
        {
            return new WriteableBitmap(source);
        }

        var scaleX = width / (double)source.PixelWidth;
        var scaleY = height / (double)source.PixelHeight;
        var transformed = new TransformedBitmap(source, new ScaleTransform(scaleX, scaleY));
        return new WriteableBitmap(transformed);
    }

    private void UpdateComposite()
    {
        CompositeBitmap = ActiveLayerStack.Compose();
        OnPropertyChanged(nameof(ActiveLayerStack));
    }

    private void UpdateBrushColor()
    {
        OnPropertyChanged(nameof(BrushColorBrush));
    }

    private void SelectPaletteColor(object? color)
    {
        if (color is not Color selected)
        {
            return;
        }

        ColorPicker.SetFromColor(selected);
        UpdateBrushColor();
    }

    private Color GetChannelPaintColor()
    {
        var color = ColorPicker.Color;
        return SelectedChannel switch
        {
            PaintChannel.Normal => Color.FromRgb(color.R, color.G, 255),
            PaintChannel.OrmAmbientOcclusion => ToGrayscale(color),
            PaintChannel.OrmRoughness => ToGrayscale(color),
            PaintChannel.OrmMetallic => ToGrayscale(color),
            PaintChannel.Opacity => ToGrayscale(color),
            _ => color
        };
    }

    private static Color ToGrayscale(Color color)
    {
        var value = (byte)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
        return Color.FromRgb(value, value, value);
    }

    private WriteableBitmap ComposeOrm()
    {
        var ao = _ormAoStack.Compose();
        var roughness = _ormRoughnessStack.Compose();
        var metallic = _ormMetallicStack.Compose();

        var width = ao.PixelWidth;
        var height = ao.PixelHeight;
        var stride = width * 4;
        var aoBuffer = new byte[stride * height];
        var roughBuffer = new byte[stride * height];
        var metalBuffer = new byte[stride * height];
        ao.CopyPixels(aoBuffer, stride, 0);
        roughness.CopyPixels(roughBuffer, stride, 0);
        metallic.CopyPixels(metalBuffer, stride, 0);

        var output = new byte[stride * height];
        for (var i = 0; i < output.Length; i += 4)
        {
            output[i] = metalBuffer[i + 2];
            output[i + 1] = roughBuffer[i + 1];
            output[i + 2] = aoBuffer[i + 2];
            output[i + 3] = 255;
        }

        var orm = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        orm.WritePixels(new Int32Rect(0, 0, width, height), output, stride, 0);
        return orm;
    }

    private static WriteableBitmap CloneBitmap(WriteableBitmap source)
    {
        var stride = source.PixelWidth * 4;
        var buffer = new byte[stride * source.PixelHeight];
        source.CopyPixels(buffer, stride, 0);
        var clone = new WriteableBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format, null);
        clone.WritePixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), buffer, stride, 0);
        return clone;
    }
}
