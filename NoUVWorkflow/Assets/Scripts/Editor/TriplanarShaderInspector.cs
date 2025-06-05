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
        GUILayout.Space(5f);

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
        }
        else
        {
            targetMat.SetColor("_MainColor", EditorGUILayout.ColorField("Main Terrain Color", targetMat.GetColor("_MainColor")));
        }

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Main Smoothness");
        targetMat.SetFloat("_MainSmoothness", EditorGUILayout.Slider(targetMat.GetFloat("_MainSmoothness"), 0f, 1f));
        EditorGUILayout.LabelField("Floor Smoothness");
        targetMat.SetFloat("_FloorSmoothness", EditorGUILayout.Slider(targetMat.GetFloat("_FloorSmoothness"), 0f, 1f));
        if (useTextures)
        {
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Texture Scale");
            targetMat.SetFloat("_Scale", EditorGUILayout.Slider(targetMat.GetFloat("_Scale"), 0.001f, 1f));
        }

        GUILayout.Space(50f);
        EditorGUI.BeginChangeCheck();
        bool useNoise = targetMat.GetInt("_UseNoise") > 0;
        useNoise = EditorGUILayout.Toggle("Use Noise", useNoise);
        if (EditorGUI.EndChangeCheck())
        {
            targetMat.SetInt("_UseNoise", useNoise ? 1 : 0);
        }
        if (useNoise)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Noise", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            EditorGUILayout.LabelField("Noise Intensity");
            targetMat.SetFloat("_NoiseIntensity", EditorGUILayout.Slider(targetMat.GetFloat("_NoiseIntensity"), 0f, 1f));
            EditorGUILayout.LabelField("Floor Noise Power");
            targetMat.SetFloat("_NoisePower", EditorGUILayout.Slider(targetMat.GetFloat("_NoisePower"), 0f, 10f));
            EditorGUILayout.LabelField("Noise Step");
            targetMat.SetFloat("_NoiseStep", EditorGUILayout.Slider(targetMat.GetFloat("_NoiseStep"), 0f, 1f));
            EditorGUILayout.LabelField("Noise Scale");
            targetMat.SetFloat("_NoiseScale", EditorGUILayout.Slider(targetMat.GetFloat("_NoiseScale"), 0.01f, 200f));
            EditorGUILayout.LabelField("Noise Distortion Intensity");
            targetMat.SetFloat("_NoiseDistortionIntensity", EditorGUILayout.Slider(targetMat.GetFloat("_NoiseDistortionIntensity"), 0f, 1f));
            EditorGUILayout.LabelField("Noise Intensity Scale");
            targetMat.SetFloat("_NoiseIntensityScale", EditorGUILayout.Slider(targetMat.GetFloat("_NoiseIntensityScale"), 0.1f, 30f));
        }

        GUILayout.Space(50f);
        EditorGUI.BeginChangeCheck();
        bool useEdgeDetection = targetMat.GetInt("_UseEdgeDetection") > 0;
        useEdgeDetection = EditorGUILayout.Toggle("Use Edge Detection", useEdgeDetection);
        if (EditorGUI.EndChangeCheck())
        {
            targetMat.SetInt("_UseEdgeDetection", useEdgeDetection ? 1 : 0);
        }
        if (useEdgeDetection)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Edge Detection", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            targetMat.SetColor("_OutlinesColor", EditorGUILayout.ColorField("Outline Color", targetMat.GetColor("_OutlinesColor")));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Thickness");
            targetMat.SetFloat("_Thickness", EditorGUILayout.Slider(targetMat.GetFloat("_Thickness"), 0f, 10f));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Normal Sensitivity");
            targetMat.SetFloat("_NormalsSensitivity", EditorGUILayout.Slider(targetMat.GetFloat("_NormalsSensitivity"), 0f, 1f));
            EditorGUILayout.LabelField("Depth Sensitivity");
            targetMat.SetFloat("_DepthSensitivity", EditorGUILayout.Slider(targetMat.GetFloat("_DepthSensitivity"), 0f, 1f));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Normal Threshold");
            targetMat.SetFloat("_NormalsThreshold", EditorGUILayout.Slider(targetMat.GetFloat("_NormalsThreshold"), 0f, 1f));
            EditorGUILayout.LabelField("Normal Tightening");
            targetMat.SetFloat("_NormalsTightening", EditorGUILayout.Slider(targetMat.GetFloat("_NormalsTightening"), 0f, 50f));
            EditorGUILayout.LabelField("Normal Strength");
            targetMat.SetFloat("_NormalsStrength", EditorGUILayout.Slider(targetMat.GetFloat("_NormalsStrength"), 0f, 300f));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Depth Threshold");
            targetMat.SetFloat("_DepthThreshold", EditorGUILayout.Slider(targetMat.GetFloat("_DepthThreshold"), 0f, 1f));
            EditorGUILayout.LabelField("Depth Tightening");
            targetMat.SetFloat("_DepthTightening", EditorGUILayout.Slider(targetMat.GetFloat("_DepthTightening"), 0f, 10f));
            EditorGUILayout.LabelField("Depth Strength");
            targetMat.SetFloat("_DepthStrength", EditorGUILayout.Slider(targetMat.GetFloat("_DepthStrength"), 0f, 300f));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Acute Depth Start Dot");
            targetMat.SetFloat("_AcuteDepthStartDot", EditorGUILayout.Slider(targetMat.GetFloat("_AcuteDepthStartDot"), 0f, 1f));
            EditorGUILayout.LabelField("Acute Depth Threshold Multiplier");
            targetMat.SetFloat("_AcuteDepthThresholdMult", EditorGUILayout.Slider(targetMat.GetFloat("_AcuteDepthThresholdMult"), 0f, 10f));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Far Depth Start");
            targetMat.SetFloat("_FarDepthStart", EditorGUILayout.Slider(targetMat.GetFloat("_FarDepthStart"), 0f, 1f));
            EditorGUILayout.LabelField("Far Depth Threshold Multiplier");
            targetMat.SetFloat("_FarDepthThresholdMult", EditorGUILayout.Slider(targetMat.GetFloat("_FarDepthThresholdMult"), 0f, 10f));
            EditorGUILayout.LabelField("Far Normal Start Depth");
            targetMat.SetFloat("_FarNormalStartDepth", EditorGUILayout.Slider(targetMat.GetFloat("_FarNormalStartDepth"), 0f, 1f));
            EditorGUILayout.LabelField("Far Normal Threshold Multiplier");
            targetMat.SetFloat("_FarNormalThresholdMult", EditorGUILayout.Slider(targetMat.GetFloat("_FarNormalThresholdMult"), 0f, 10f));
        }

        GUILayout.Space(50f);
        EditorGUI.BeginChangeCheck();
        bool useRimLight = targetMat.GetInt("_UseRimLight") > 0;
        useRimLight = EditorGUILayout.Toggle("Use Rim Light", useRimLight);
        if (EditorGUI.EndChangeCheck())
        {
            targetMat.SetInt("_UseRimLight", useRimLight ? 1 : 0);
        }
        if (useRimLight)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Rim Light", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            targetMat.SetColor("_RimLightColor", EditorGUILayout.ColorField("Rim Light Color", targetMat.GetColor("_RimLightColor")));
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Rim Light Strength");
            targetMat.SetFloat("_RimLightStrength", EditorGUILayout.Slider(targetMat.GetFloat("_RimLightStrength"), 0f, 10f));
            EditorGUILayout.LabelField("Edge Power");
            targetMat.SetFloat("_RimLightEdgePower", EditorGUILayout.Slider(targetMat.GetFloat("_RimLightEdgePower"), 0.01f, 10f));
            EditorGUILayout.LabelField("Shadow Power");
            targetMat.SetFloat("_RimLightShadowPower", EditorGUILayout.Slider(targetMat.GetFloat("_RimLightShadowPower"), 0.01f, 10f));
            EditorGUILayout.LabelField("Rim Light Color To Light Color Ratio");
            targetMat.SetFloat("_RimLightColorInfluence", EditorGUILayout.Slider(targetMat.GetFloat("_RimLightColorInfluence"), 0f, 1f));

        }

        GUILayout.Space(80f);
        GUILayout.Label("Raw Material Fields", EditorStyles.boldLabel);

        base.OnGUI(materialEditor, properties);
    }

    private string PromptTextures(Material targetMat, string label)
    {
        string path = EditorUtility.OpenFolderPanel("Select Texture Folder", "", "");
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
                //Graphics.CopyTexture(normalTexture, 0, 0, texArray, 1, 0);
                targetMat.SetTexture("_" + label + "NormalTexture", textures[i]);
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
        if (!AssetDatabase.IsValidFolder("Assets/Textures/TextureArray/" + targetMat.name))
        {
            AssetDatabase.CreateFolder("Assets/Textures/TextureArray", targetMat.name);
        }
        AssetDatabase.CreateAsset(texArray, "Assets/Textures/TextureArray/" + targetMat.name + "/" + label + ".asset");

        targetMat.SetTexture("_" + label + "Textures", texArray);

        return true;
    }

    private void ClearMaterialTextures(Material targetMat, string label)
    {
        string property;
        property = "_" + label + "Textures";
        targetMat.SetTexture(property, null);
        property = "_" + label + "NormalTexture";
        targetMat.SetTexture(property, null);
    }

    private bool MaterialTexturesMissing(Material targetMat, string label)
    {
        return targetMat.GetTexture("_" + label + "Textures") == null;
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