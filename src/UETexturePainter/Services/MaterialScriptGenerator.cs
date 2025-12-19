using System.IO;
using System.Text;

namespace UETexturePainter.Services;

public class MaterialScriptGenerator
{
    public string Generate(string assetName, string targetFolder, string outputFolder, string format)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import os");
        sb.AppendLine("import unreal");
        sb.AppendLine();
        sb.AppendLine($"asset_name = '{assetName}'");
        sb.AppendLine($"target_folder = '/Game/{targetFolder}'");
        sb.AppendLine($"export_folder = r'''{outputFolder}'''");
        sb.AppendLine($"extension = '{format}'");
        sb.AppendLine("texture_folder = f'{target_folder}/Textures'");
        sb.AppendLine("material_folder = f'{target_folder}/Materials'");
        sb.AppendLine();
        sb.AppendLine("asset_tools = unreal.AssetToolsHelpers.get_asset_tools()");
        sb.AppendLine("editor_asset_lib = unreal.EditorAssetLibrary()");
        sb.AppendLine();
        sb.AppendLine("def import_texture(name, file_path, srgb, compression):");
        sb.AppendLine("    task = unreal.AssetImportTask()");
        sb.AppendLine("    task.set_editor_property('automated', True)");
        sb.AppendLine("    task.set_editor_property('destination_path', texture_folder)");
        sb.AppendLine("    task.set_editor_property('destination_name', name)");
        sb.AppendLine("    task.set_editor_property('filename', file_path)");
        sb.AppendLine("    task.set_editor_property('replace_existing', True)");
        sb.AppendLine("    asset_tools.import_asset_tasks([task])");
        sb.AppendLine("    asset = editor_asset_lib.load_asset(f'{texture_folder}/{name}')");
        sb.AppendLine("    asset.set_editor_property('srgb', srgb)");
        sb.AppendLine("    asset.set_editor_property('compression_settings', compression)");
        sb.AppendLine("    asset.post_edit_change()");
        sb.AppendLine("    asset.mark_package_dirty()");
        sb.AppendLine("    return asset");
        sb.AppendLine();
        sb.AppendLine("def ensure_folder(path):");
        sb.AppendLine("    if not editor_asset_lib.does_directory_exist(path):");
        sb.AppendLine("        editor_asset_lib.make_directory(path)");
        sb.AppendLine();
        sb.AppendLine("ensure_folder(texture_folder)");
        sb.AppendLine("ensure_folder(material_folder)");
        sb.AppendLine();
        sb.AppendLine("base_color_path = f\"{export_folder}/{asset_name}_BaseColor.{extension}\"");
        sb.AppendLine("normal_path = f\"{export_folder}/{asset_name}_Normal.{extension}\"");
        sb.AppendLine("orm_path = f\"{export_folder}/{asset_name}_ORM.{extension}\"");
        sb.AppendLine("emissive_path = f\"{export_folder}/{asset_name}_Emissive.{extension}\"");
        sb.AppendLine("opacity_path = f\"{export_folder}/{asset_name}_Opacity.{extension}\"");
        sb.AppendLine();
        sb.AppendLine("base_color = import_texture(f'{asset_name}_BaseColor', base_color_path, True, unreal.TextureCompressionSettings.TC_DEFAULT)");
        sb.AppendLine("normal = import_texture(f'{asset_name}_Normal', normal_path, False, unreal.TextureCompressionSettings.TC_NORMALMAP)");
        sb.AppendLine("orm = import_texture(f'{asset_name}_ORM', orm_path, False, unreal.TextureCompressionSettings.TC_MASKS)");
        sb.AppendLine();
        sb.AppendLine("material_factory = unreal.MaterialFactoryNew()");
        sb.AppendLine("material = asset_tools.create_asset(f'M_{asset_name}', material_folder, unreal.Material, material_factory)");
        sb.AppendLine("material_editor = unreal.MaterialEditingLibrary");
        sb.AppendLine();
        sb.AppendLine("def add_texture_sample(tex, pos_x, pos_y):");
        sb.AppendLine("    node = material_editor.create_material_expression(material, unreal.MaterialExpressionTextureSample, pos_x, pos_y)");
        sb.AppendLine("    node.texture = tex");
        sb.AppendLine("    return node");
        sb.AppendLine();
        sb.AppendLine("base_node = add_texture_sample(base_color, -400, 0)");
        sb.AppendLine("normal_node = add_texture_sample(normal, -400, 200)");
        sb.AppendLine("orm_node = add_texture_sample(orm, -400, 400)");
        sb.AppendLine("base_node.sampler_type = unreal.MaterialSamplerType.SAMPLERTYPE_COLOR");
        sb.AppendLine("normal_node.sampler_type = unreal.MaterialSamplerType.SAMPLERTYPE_NORMAL");
        sb.AppendLine("orm_node.sampler_type = unreal.MaterialSamplerType.SAMPLERTYPE_LINEAR_COLOR");
        sb.AppendLine();
        sb.AppendLine("material_editor.connect_material_property(base_node, 'RGB', unreal.MaterialProperty.MP_BASE_COLOR)");
        sb.AppendLine("material_editor.connect_material_property(normal_node, 'RGB', unreal.MaterialProperty.MP_NORMAL)");
        sb.AppendLine("material_editor.connect_material_property(orm_node, 'R', unreal.MaterialProperty.MP_AMBIENT_OCCLUSION)");
        sb.AppendLine("material_editor.connect_material_property(orm_node, 'G', unreal.MaterialProperty.MP_ROUGHNESS)");
        sb.AppendLine("material_editor.connect_material_property(orm_node, 'B', unreal.MaterialProperty.MP_METALLIC)");
        sb.AppendLine();
        sb.AppendLine("if os.path.exists(emissive_path):");
        sb.AppendLine("    emissive = import_texture(f'{asset_name}_Emissive', emissive_path, True, unreal.TextureCompressionSettings.TC_DEFAULT)");
        sb.AppendLine("    emissive_node = add_texture_sample(emissive, -400, 600)");
        sb.AppendLine("    material_editor.connect_material_property(emissive_node, 'RGB', unreal.MaterialProperty.MP_EMISSIVE_COLOR)");
        sb.AppendLine();
        sb.AppendLine("if os.path.exists(opacity_path):");
        sb.AppendLine("    opacity = import_texture(f'{asset_name}_Opacity', opacity_path, False, unreal.TextureCompressionSettings.TC_MASKS)");
        sb.AppendLine("    opacity_node = add_texture_sample(opacity, -400, 800)");
        sb.AppendLine("    material_editor.connect_material_property(opacity_node, 'R', unreal.MaterialProperty.MP_OPACITY_MASK)");
        sb.AppendLine();
        sb.AppendLine("material.post_edit_change()");
        sb.AppendLine("material.mark_package_dirty()");
        sb.AppendLine();
        sb.AppendLine("mi_factory = unreal.MaterialInstanceConstantFactoryNew()");
        sb.AppendLine("material_instance = asset_tools.create_asset(f'MI_{asset_name}', material_folder, unreal.MaterialInstanceConstant, mi_factory)");
        sb.AppendLine("material_instance.set_editor_property('parent', material)");
        sb.AppendLine("material_instance.post_edit_change()");
        sb.AppendLine("material_instance.mark_package_dirty()");
        sb.AppendLine();
        sb.AppendLine("assets_to_save = [base_color, normal, orm, material, material_instance]");
        sb.AppendLine("if 'emissive' in locals():");
        sb.AppendLine("    assets_to_save.append(emissive)");
        sb.AppendLine("if 'opacity' in locals():");
        sb.AppendLine("    assets_to_save.append(opacity)");
        sb.AppendLine("editor_asset_lib.save_loaded_assets(assets_to_save)");
        return sb.ToString();
    }

    public void SaveTo(string outputFolder, string assetName, string targetFolder, string format)
    {
        var script = Generate(assetName, targetFolder, outputFolder, format);
        var path = Path.Combine(outputFolder, $"CreateMaterial_{assetName}.py");
        File.WriteAllText(path, script);
    }
}
