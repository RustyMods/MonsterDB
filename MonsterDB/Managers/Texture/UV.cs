using UnityEngine;
using System.IO;
using MonsterDB;

public static class UVMapExporter
{
    public static void ExportUVMaps(Terminal context, string prefabId, string bkgHex = "", string lineHex = "")
    {
        if (PrefabManager.GetPrefab(prefabId) is not { } prefab)
        {
            context.LogWarning($"Failed to find prefab: {prefabId}");
            return;
        }
        
        context.LogInfo("Trying to generate UV maps for " + prefabId);

        Color bkg = string.IsNullOrEmpty(bkgHex) ? default : bkgHex.FromHexOrRGBA(new Color(0.15f, 0.15f, 0.15f, 1f));
        Color line = string.IsNullOrEmpty(lineHex) ? default : lineHex.FromHexOrRGBA(new Color(0.4f, 0.8f, 1.0f, 1f));
        
        MeshRenderer[]? renderers = prefab.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) continue;

            context.LogWarning("Trying to generate UV maps for " + renderer.name);
            var width = 256;
            var height = 256;
            if (renderer.material != null && renderer.material.mainTexture != null)
            {
                width = renderer.material.mainTexture.width;
                height = renderer.material.mainTexture.height;
            }
            
            DrawAndExportUVMap(context, prefab, filter.sharedMesh, width, height, line, bkg);
        }

        var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skin in skins)
        {
            if (skin.sharedMesh == null) continue;
            
            context.LogWarning("Trying to generate UV maps for " + skin.name);
            
            var width = 256;
            var height = 256;
            if (skin.material != null && skin.material.mainTexture != null)
            {
                width = skin.material.mainTexture.width;
                height = skin.material.mainTexture.height;
            }
            DrawAndExportUVMap(context, prefab, skin.sharedMesh, width, height, line, bkg);
        }

    }
    public static Texture2D? DrawAndExportUVMap(
        Terminal context,
        GameObject prefab,
        Mesh mesh,
        int width,
        int height,
        Color lineColor = default,
        Color bgColor = default,
        string? outputPath = null)
    {
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        if (uvs == null || uvs.Length == 0)
        {
            context.LogError($"Mesh '{mesh.name}' has no UV coordinates.");
            return null;
        }
        
        if (lineColor == default) lineColor = new Color(0.4f, 0.8f, 1.0f, 1f);   // light blue
        if (bgColor  == default) bgColor  = new Color(0.15f, 0.15f, 0.15f, 1f); // dark gray
        
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bgColor;
        tex.SetPixels(pixels);
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv0 = uvs[triangles[i    ]];
            Vector2 uv1 = uvs[triangles[i + 1]];
            Vector2 uv2 = uvs[triangles[i + 2]];

            DrawLine(tex, uv0, uv1, lineColor, width, height);
            DrawLine(tex, uv1, uv2, lineColor, width, height);
            DrawLine(tex, uv2, uv0, lineColor, width, height);
        }

        tex.Apply();
        
        if (outputPath == null)
            outputPath = Path.Combine(FileManager.ExportFolder, $"{prefab.name}_{mesh.name}_uv.png");

        File.WriteAllBytes(outputPath, tex.EncodeToPNG());
        context.LogInfo($"UV map saved to: {outputPath}");
        
        return tex;
    }

    // Bresenham's line algorithm in UV space (0–1) mapped to pixel space
    private static void DrawLine(Texture2D tex, Vector2 uvA, Vector2 uvB, Color color, int width, int height)
    {
        int x0 = Mathf.RoundToInt(uvA.x * (width  - 1));
        int y0 = Mathf.RoundToInt(uvA.y * (height - 1));
        int x1 = Mathf.RoundToInt(uvB.x * (width  - 1));
        int y1 = Mathf.RoundToInt(uvB.y * (height - 1));

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
            {
                tex.SetPixel(x0, y0, color);
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = err;
            if (e2 > -dx)
            {
                err -= dy; x0 += sx;
            }

            if (e2 < dy)
            {
                err += dx; y0 += sy;
            }
        }
    }
}
