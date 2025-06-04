using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

public class TriplanarShaderInspector : ShaderGUI
{
    string mainTexturePath;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;

        GUILayout.Label("Textures", EditorStyles.boldLabel);

        bool useTextures = Array.IndexOf(targetMat.shaderKeywords, "_USE_TEXTURES") != -1;

        EditorGUI.BeginChangeCheck();
        useTextures = EditorGUILayout.Toggle("Use Textures", useTextures);
        if (EditorGUI.EndChangeCheck())
        {
            TextureCheckbox(targetMat, useTextures);
        }

        bool useFloor = Array.IndexOf(targetMat.shaderKeywords, "_USE_FLOOR") != -1;
        if (useTextures)
        {
            GUIStyle style = new GUIStyle();
            if (GUILayout.Button("Replace Main Texture"))
            {
                string path = PromptTextures(targetMat, "Main");

                if (path != null)
                {
                    mainTexturePath = path;
                }
            }
            if (GUILayout.Button("Reset Textures"))
            {
                ClearMaterialTextures(targetMat, "Main");
                ClearMaterialTextures(targetMat, "Floor");
                ClearMaterialTextures(targetMat, "Red");
                ClearMaterialTextures(targetMat, "Green");
                ClearMaterialTextures(targetMat, "Blue");
                targetMat.DisableKeyword("_USE_TEXTURES");
                targetMat.DisableKeyword("_USE_FLOOR");
                targetMat.DisableKeyword("_OVERRIDE_VERTEX_COLOR");
            }
            GUILayout.Space(10f);
            EditorGUI.BeginChangeCheck();
            useFloor = EditorGUILayout.Toggle("Enable Ground Texture", useFloor);
            if (EditorGUI.EndChangeCheck())
            {
                FloorCheckbox(targetMat, useFloor);
            }
            if (GUILayout.Button("Replace Ground Texture"))
            {
                string path = PromptTextures(targetMat, "Floor");

                if (path != null)
                {
                    mainTexturePath = path;
                }
            }
            GUILayout.Space(10f);
            GUILayout.Label("Vertex Color");
            if (GUILayout.Button("Override red channel texture"))
            {
                PromptTextures(targetMat, "Red");
                targetMat.EnableKeyword("_OVERRIDE_VERTEX_COLOR");
            }
            if (GUILayout.Button("Override green channel texture"))
            {
                PromptTextures(targetMat, "Green");
                targetMat.EnableKeyword("_OVERRIDE_VERTEX_COLOR");
            }
            if (GUILayout.Button("Override blue channel texture"))
            {
                PromptTextures(targetMat, "Blue");
                targetMat.EnableKeyword("_OVERRIDE_VERTEX_COLOR");
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Clear Overrides"))
            {
                ClearMaterialTextures(targetMat, "Red");
                ClearMaterialTextures(targetMat, "Green");
                ClearMaterialTextures(targetMat, "Blue");
                targetMat.DisableKeyword("_OVERRIDE_VERTEX_COLOR");
            }

            GUILayout.Space(20f);
        }


        base.OnGUI(materialEditor, properties);
    }

    private string PromptTextures(Material targetMat, string label)
    {
        string path = EditorUtility.OpenFolderPanel("Load png Textures", "", "");
        if (path.Length == 0) return null;
        path = path.Substring(path.IndexOf("Assets"));

        SetMaterialTextures(targetMat, label, path);
        return path;
    }

    private bool SetMaterialTextures(Material targetMat, string label, string path)
    {
        var textures = GetAllTextures(path);
        if (textures == null || textures.Count == 0) return false;

        Texture2DArray texArray = new Texture2DArray(textures.First().width, textures.First().height, 4, (textures.First() as Texture2D).format, false);

        for (int i = 0; i < textures.Count; i++)
        {
            if (textures[i].name.Contains("_albedo", StringComparison.InvariantCultureIgnoreCase) ||
                textures[i].name.Contains("-albedo", StringComparison.InvariantCultureIgnoreCase))
            {
                Graphics.CopyTexture(textures[i], 0, 0, texArray, 0, 0);
            }
            else if (textures[i].name.Contains("_normal", StringComparison.InvariantCultureIgnoreCase) ||
                textures[i].name.Contains("-normal", StringComparison.InvariantCultureIgnoreCase))
            {

                Texture2D normalTexture = new Texture2D(texArray.width, texArray.height, texArray.format, false);
                normalTexture.filterMode = FilterMode.Trilinear;
                normalTexture.wrapMode = TextureWrapMode.Clamp;
                Color[] pixels = (textures[i] as Texture2D).GetPixels(0, 0, texArray.width, texArray.height);
                float r, g, b, a;
                for (int j = pixels.Length - 1; j >= 0; j--)
                {
                    Color c = pixels[j];
                    r = g = b = c.g;
                    a = c.r;
                    pixels[j] = new Color(r, g, b, a);
                }
                normalTexture.SetPixels(pixels);
                normalTexture.Apply(true);

                Graphics.CopyTexture(normalTexture, 0, 0, texArray, 1, 0);
            }
            else if (textures[i].name.Contains("_height", StringComparison.InvariantCultureIgnoreCase) ||
                textures[i].name.Contains("-height", StringComparison.InvariantCultureIgnoreCase))
            {
                Graphics.CopyTexture(textures[i], 0, 0, texArray, 2, 0);
            }
            else if (textures[i].name.Contains("_metallic", StringComparison.InvariantCultureIgnoreCase) ||
                textures[i].name.Contains("-metallic", StringComparison.InvariantCultureIgnoreCase))
            {
                //Graphics.CopyTexture(textures[i], 0, 0, texArray, 3, 0);
            }
            else if (textures[i].name.Contains("_AO", StringComparison.InvariantCultureIgnoreCase) ||
                textures[i].name.Contains("-AO", StringComparison.InvariantCultureIgnoreCase))
            {
                Graphics.CopyTexture(textures[i], 0, 0, texArray, 3, 0);
            }
            else
            {
                continue;
            }
        }

        AssetDatabase.CreateAsset(texArray, "Assets/Textures/TextureArray/" + label + ".asset");

        targetMat.SetTexture("_" + label + "Textures", texArray);

        return true;
    }

    private void ClearMaterialTextures(Material targetMat, string label)
    {
        string property;
        property = "_" + label + "AlbedoMap";
        targetMat.SetTexture(property, null);
        property = "_" + label + "NormalMap";
        targetMat.SetTexture(property, null);
        property = "_" + label + "HeightMap";
        targetMat.SetTexture(property, null);
        property = "_" + label + "MetallicMap";
        targetMat.SetTexture(property, null);
        property = "_" + label + "AOMap";
        targetMat.SetTexture(property, null);
    }

    private bool MaterialTexturesMissing(Material targetMat, string label)
    {
        return targetMat.GetTexture("_" + label + "AlbedoMap") == null && targetMat.GetTexture("_" + label + "NormalMap") == null;
    }

    private void FloorCheckbox(Material targetMat, bool checkbox)
    {
        if (!checkbox)
        {
            targetMat.DisableKeyword("_USE_FLOOR");
            return;
        }

        if (MaterialTexturesMissing(targetMat, "Floor"))
        {
            string path = PromptTextures(targetMat, "Floor");
            if (path == null) return;
        }

        targetMat.EnableKeyword("_USE_FLOOR");
    }

    private void TextureCheckbox(Material targetMat, bool checkbox)
    {
        if (!checkbox)
        {
            targetMat.DisableKeyword("_USE_TEXTURES");
            targetMat.DisableKeyword("_USE_FLOOR");
            return;
        }

        if (MaterialTexturesMissing(targetMat, "Main"))
        {
            string path = PromptTextures(targetMat, "Main");
            SetMaterialTextures(targetMat, "Red", path);
            SetMaterialTextures(targetMat, "Green", path);
            SetMaterialTextures(targetMat, "Blue", path);

            if (path == null) return;

            mainTexturePath = path;
        }

        targetMat.EnableKeyword("_USE_TEXTURES");
    }

    private List<Texture> GetAllTextures(string dir)
    {;
        string[] foldersToSearch = { dir };
        List<Texture> allTextures = GetAssets<Texture>(foldersToSearch, "t:Texture");
        return allTextures;
    }

    public static List<T> GetAssets<T>(string[] foldersToSearch, string filter) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets(filter, foldersToSearch);
        var assets = new List<T>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            assets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
        }
        return assets;
    }
}