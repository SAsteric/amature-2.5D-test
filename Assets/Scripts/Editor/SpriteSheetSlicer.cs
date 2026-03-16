#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to auto-slice the three character sprite sheets.
/// Menu: Tools → Character → Slice Sprite Sheets
///
/// IMPORTANT: Run this ONCE after importing the PNG files.
/// </summary>
public class SpriteSheetSlicer : EditorWindow
{
    private Texture2D walkSheet;
    private Texture2D runSheet;
    private Texture2D jumpSheet;

    [MenuItem("Tools/Character/Slice Sprite Sheets")]
    public static void ShowWindow()
        => GetWindow<SpriteSheetSlicer>("Sprite Sheet Slicer");

    private void OnGUI()
    {
        GUILayout.Label("Character Sprite Sheet Slicer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Assign the three imported PNG textures below.\n" +
            "Each sheet must be set to Texture Type: Sprite (Multiple).\n" +
            "Click Slice to create a 3×4 grid (3 columns, 4 rows).",
            MessageType.Info);

        walkSheet = (Texture2D)EditorGUILayout.ObjectField("Walk Sheet", walkSheet, typeof(Texture2D), false);
        runSheet  = (Texture2D)EditorGUILayout.ObjectField("Run Sheet",  runSheet,  typeof(Texture2D), false);
        jumpSheet = (Texture2D)EditorGUILayout.ObjectField("Jump Sheet", jumpSheet, typeof(Texture2D), false);

        EditorGUI.BeginDisabledGroup(walkSheet == null || runSheet == null || jumpSheet == null);
        if (GUILayout.Button("Slice All Three Sheets"))
        {
            SliceSheet(walkSheet);
            SliceSheet(runSheet);
            SliceSheet(jumpSheet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Sprite sheets sliced successfully!");
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Slices a sheet into a 3-column × 4-row grid.
    /// Cell size is derived from the texture dimensions.
    /// </summary>
    private static void SliceSheet(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode       = FilterMode.Point;   // pixel-art crisp rendering
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        int cols = 3;
        int rows = 4;
        int cellW = tex.width  / cols;
        int cellH = tex.height / rows;

        var metas = new SpriteMetaData[rows * cols];
        int idx = 0;

        // Unity's rect origin is bottom-left; sprite sheets read top-to-bottom.
        // We flip the row index so row 0 (top of sheet) = Down direction.
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int flippedRow = (rows - 1) - row;
                metas[idx] = new SpriteMetaData
                {
                    name   = $"{Path.GetFileNameWithoutExtension(path)}_r{row}_c{col}",
                    rect   = new Rect(col * cellW, flippedRow * cellH, cellW, cellH),
                    pivot  = new Vector2(0.5f, 0.15f), // feet anchor — tweak if needed
                    alignment = (int)SpriteAlignment.Custom
                };
                idx++;
            }
        }

        importer.spritesheet = metas;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }
}
#endif