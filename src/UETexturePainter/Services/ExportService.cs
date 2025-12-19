using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using UETexturePainter.Models;

namespace UETexturePainter.Services;

public class ExportService
{
    public void ExportTextureSet(ExportRequest request)
    {
        Directory.CreateDirectory(request.OutputFolder);

        var baseColorPath = Path.Combine(request.OutputFolder, $"{request.AssetName}_BaseColor.{request.Format}");
        var normalPath = Path.Combine(request.OutputFolder, $"{request.AssetName}_Normal.{request.Format}");
        var ormPath = Path.Combine(request.OutputFolder, $"{request.AssetName}_ORM.{request.Format}");

        SaveBitmap(request.BaseColor, baseColorPath, request.Format);
        SaveBitmap(request.Normal, normalPath, request.Format);
        SaveBitmap(request.Orm, ormPath, request.Format);

        if (request.Emissive is not null)
        {
            SaveBitmap(request.Emissive, Path.Combine(request.OutputFolder, $"{request.AssetName}_Emissive.{request.Format}"), request.Format);
        }

        if (request.Opacity is not null)
        {
            SaveBitmap(request.Opacity, Path.Combine(request.OutputFolder, $"{request.AssetName}_Opacity.{request.Format}"), request.Format);
        }

        var manifest = new MaterialManifest
        {
            AssetName = request.AssetName,
            TargetFolder = request.TargetFolder,
            TextureFormat = request.Format,
            Textures = new List<MaterialTextureInfo>
            {
                new("BaseColor", baseColorPath, true),
                new("Normal", normalPath, false),
                new("ORM", ormPath, false)
            }
        };

        if (request.Emissive is not null)
        {
            manifest.Textures.Add(new MaterialTextureInfo("Emissive", Path.Combine(request.OutputFolder, $"{request.AssetName}_Emissive.{request.Format}"), true));
        }

        if (request.Opacity is not null)
        {
            manifest.Textures.Add(new MaterialTextureInfo("Opacity", Path.Combine(request.OutputFolder, $"{request.AssetName}_Opacity.{request.Format}"), false));
        }

        var settingsPath = Path.Combine(request.OutputFolder, "material_manifest.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsPath, json);
    }

    private static void SaveBitmap(WriteableBitmap bitmap, string path, string format)
    {
        if (format.Equals("tga", StringComparison.OrdinalIgnoreCase))
        {
            TgaWriter.Write(path, bitmap);
            return;
        }

        using var stream = File.Create(path);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);
    }
}

public record ExportRequest(
    string AssetName,
    string OutputFolder,
    string TargetFolder,
    string Format,
    WriteableBitmap BaseColor,
    WriteableBitmap Normal,
    WriteableBitmap Orm,
    WriteableBitmap? Emissive,
    WriteableBitmap? Opacity);

public record MaterialManifest
{
    public string AssetName { get; init; } = string.Empty;
    public string TargetFolder { get; init; } = string.Empty;
    public string TextureFormat { get; init; } = "png";
    public List<MaterialTextureInfo> Textures { get; init; } = new();
}

public record MaterialTextureInfo(string Role, string FilePath, bool Srgb);
