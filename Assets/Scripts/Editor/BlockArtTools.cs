using System.IO;
using UnityEditor;
using UnityEngine;

public static class BlockArtTools
{
    [MenuItem("Tools/Art/Check Block Model")]
    public static void CheckBlockModel()
    {
        if (Selection.objects.Length == 0)
        {
            Debug.LogWarning("[ArtCheck] 先在 Project 窗口选中 FBX 资源。");
            return;
        }

        foreach (Object selected in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(selected);

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer == null)
            {
                Debug.LogWarning($"[ArtCheck] {path} 不是模型资源。");
                continue;
            }

            Debug.Log(
                $"[ArtCheck] {path}\n"
                + $"  globalScale={importer.globalScale}  useFileScale={importer.useFileScale}  "
                + $"materialImportMode={importer.materialImportMode}  meshCompression={importer.meshCompression}");

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            int meshCount = 0;
            int materialCount = 0;

            foreach (Object asset in assets)
            {
                if (asset is Mesh mesh)
                {
                    meshCount++;
                    Bounds bounds = mesh.bounds;
                    Vector3 size = bounds.size;
                    Vector3 center = bounds.center;

                    bool sizeIsOne = Mathf.Abs(size.x - 1f) < 0.01f
                                     && Mathf.Abs(size.y - 1f) < 0.01f
                                     && Mathf.Abs(size.z - 1f) < 0.01f;

                    bool pivotCentered = center.magnitude < 0.01f;

                    Debug.Log(
                        $"[ArtCheck] Mesh '{mesh.name}'  size=({size.x:F3}, {size.y:F3}, {size.z:F3})  "
                        + $"center=({center.x:F3}, {center.y:F3}, {center.z:F3})  "
                        + $"verts={mesh.vertexCount}  tris={mesh.triangles.Length / 3}  "
                        + $"size1m={sizeIsOne}  pivotCentered={pivotCentered}");
                }
                else if (asset is Material material)
                {
                    materialCount++;
                    Debug.Log($"[ArtCheck] Material '{material.name}' shader={material.shader.name}");
                }
            }

            Debug.Log($"[ArtCheck] 汇总：网格 {meshCount} 个，材质 {materialCount} 个");
        }
    }

    [MenuItem("Tools/Art/Fix Block Textures")]
    public static void FixBlockTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Blocks" });
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            bool isNormal = fileName.Contains("normal");
            bool isData = fileName.Contains("metallic") || fileName.Contains("roughness");

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal && !isData;
            importer.maxTextureSize = isData ? 512 : 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();

            changed++;
            Debug.Log($"[ArtCheck] {path}  type={(isNormal ? "Normal" : isData ? "Data" : "Color")}  maxSize={importer.maxTextureSize}");
        }

        Debug.Log($"[ArtCheck] 共处理 {changed} 张贴图。");
    }
}
