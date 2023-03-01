using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JBooth.BetterLit
{
   public class BetterLitMaterialUpgrader : EditorWindow
   {
      [MenuItem("Window/Better Lit Shader/Upgrade Materials to 2021")]
      public static void ShowWindow()
      {
         var window = GetWindow<BetterLitMaterialUpgrader>();
         window.Show();
      }

      bool UpgradeFloat(Material mat, string prop, string keyword)
      {
         if (mat.IsKeywordEnabled(keyword))
         {
            return false;
         }
         if (mat.HasProperty(prop) && mat.GetFloat(prop) > 0)
         {
            mat.EnableKeyword(keyword);
            return true;
         }
         return false;
      }

      bool UpgradeNoise(Material mat)
      {
         if (mat.shaderKeywords != null)
         {
            List<string> keywords = new List<string>(mat.shaderKeywords);
            if (mat.GetInt("_Version") > 2020)
               return false; // already revisioned.
            if (keywords.Contains("_LAYERNOISE") ||
               keywords.Contains("_LAYERNOISE_DEF1") ||
               keywords.Contains("_LAYERNOISE_DEF2") ||
               keywords.Contains("_PUDDLENOISE")
               )
            {
               // noise is now local instead of global
               int noiseSpace = 0;
               if (keywords.Contains("_NOISEWORLD"))
                  noiseSpace = 1;
               else if (keywords.Contains("_NOISELOCAL"))
                  noiseSpace = 2;

               int noiseMode = 0;
               if (keywords.Contains("_NOISEHQ"))
                  noiseMode = 1;
               else if (keywords.Contains("_NOISETEXTURE"))
                  noiseMode = 2;
               else if (keywords.Contains("_NOISEWORLEY"))
                  noiseMode = 3;

               if (mat.IsKeywordEnabled("_LAYERNOISE"))
               {
                  mat.SetTexture("_LayerNoiseTex", mat.GetTexture("_NoiseTex"));
                  if (noiseSpace == 1)
                     mat.EnableKeyword("_LAYERNOISEWORLD");
                  else if (noiseSpace == 2)
                     mat.EnableKeyword("_LAYERNOISELOCAL");

                  if (noiseMode == 1)
                     mat.EnableKeyword("_LAYERNOISEHQ");
                  else if (noiseMode == 2)
                     mat.EnableKeyword("_LAYERNOISETEXTURE");
                  else if (noiseMode == 3)
                     mat.EnableKeyword("_LAYERNOISEWORLEY");
               }
               if (mat.IsKeywordEnabled("_LAYERNOISE_DEF1"))
               {
                  mat.SetTexture("_LayerNoiseTex_Ext1", mat.GetTexture("_NoiseTex"));
                  if (noiseSpace == 1)
                     mat.EnableKeyword("_LAYERNOISEWORLD_DEF1");
                  else if (noiseSpace == 2)
                     mat.EnableKeyword("_LAYERNOISELOCAL_DEF1");

                  if (noiseMode == 1)
                     mat.EnableKeyword("_LAYERNOISEHQ_DEF1");
                  else if (noiseMode == 2)
                     mat.EnableKeyword("_LAYERNOISETEXTURE_DEF1");
                  else if (noiseMode == 3)
                     mat.EnableKeyword("_LAYERNOISEWORLEY_DEF1");
               }
               if (mat.IsKeywordEnabled("_LAYERNOISE_DEF2"))
               {
                  mat.SetTexture("_LayerNoiseTex_Ext2", mat.GetTexture("_NoiseTex"));
                  if (noiseSpace == 1)
                     mat.EnableKeyword("_LAYERNOISEWORLD_DEF2");
                  else if (noiseSpace == 2)
                     mat.EnableKeyword("_LAYERNOISELOCAL_DEF2");

                  if (noiseMode == 1)
                     mat.EnableKeyword("_LAYERNOISEHQ_DEF2");
                  else if (noiseMode == 2)
                     mat.EnableKeyword("_LAYERNOISETEXTURE_DEF2");
                  else if (noiseMode == 3)
                     mat.EnableKeyword("_LAYERNOISEWORLEY_DEF2");
               }
               if (mat.IsKeywordEnabled("_PUDDLENOISE"))
               {
                  mat.SetTexture("_PuddleNoiseTex", mat.GetTexture("_NoiseTex"));
                  if (noiseSpace == 1)
                     mat.EnableKeyword("_PUDDLENOISEWORLD");
                  else if (noiseSpace == 2)
                     mat.EnableKeyword("_PUDDLENOISELOCAL");

                  if (noiseMode == 1)
                     mat.EnableKeyword("_PUDDLENOISEHQ");
                  else if (noiseMode == 2)
                     mat.EnableKeyword("_PUDDLENOISETEXTURE");
                  else if (noiseMode == 3)
                     mat.EnableKeyword("_PUDDLENOISEWORLEY");
               }
               mat.SetTexture("_NoiseTex", null);
               return true;
            }
         }
         return false;
      }

      private bool UpgradeBlendMode(Material mat, string prop, string keyword0, string keyword1, string keyword2)
      {
         if (mat.IsKeywordEnabled(keyword0) || mat.IsKeywordEnabled(keyword1) || mat.IsKeywordEnabled(keyword2))
         {
            return false;
         }
         if (mat.HasProperty(prop))
         {
            float val = mat.GetFloat(prop);
            if (val > 1.5f)
            {
               mat.EnableKeyword(keyword2);
            }
            else if (val > 0.5f)
            {
               mat.EnableKeyword(keyword1);
            }
            else
            {
               mat.EnableKeyword(keyword0);
            }
            return true;
         }
         return false;
      }

      private bool Upgrade(Material mat)
      {
         bool changed = false;
         if (mat.HasProperty("_AlbedoContrast") && (mat.GetFloat("_AlbedoContrast") != 1 || mat.GetFloat("_AlbedoBrightness") != 0))
         {
            mat.EnableKeyword("_USEBRIGHTNESSCONTRAST");
            changed = true;
         }
         changed = changed || UpgradeFloat(mat, "_AlphaThreshold", "_ALPHACUT");
         changed = changed || UpgradeFloat(mat, "_FuzzyShadingOn", "_FUZZYSHADING");
         changed = changed || UpgradeFloat(mat, "_MicroShadowStrength", "_MICROSHADOW");
         changed = changed || UpgradeFloat(mat, "_LayerFuzzyShadingOn", "_LAYERFUZZYSHADING");
         changed = changed || UpgradeFloat(mat, "_LayerFuzzyShadingOn_Ext_1", "_LAYERFUZZYSHADING_DEF_1");
         changed = changed || UpgradeFloat(mat, "_LayerFuzzyShadingOn_Ext_2", "_LAYERFUZZYSHADING_DEF_2");
         changed = changed || UpgradeBlendMode(mat, "_LayerBlendMode", "_LAYERBLENDMULT2X", "_LAYERBLENDALPHA", "_LAYERBLENDHEIGHT");
         changed = changed || UpgradeBlendMode(mat, "_LayerBlendMode_Ext_1", "_LAYERBLENDMULT2X_DEF_1", "_LAYERBLENDALPHA_DEF_1", "_LAYERBLENDHEIGHT_DEF_1");
         changed = changed || UpgradeBlendMode(mat, "_LayerBlendMode_Ext_2", "_LAYERBLENDMULT2X_DEF_2", "_LAYERBLENDALPHA_DEF_2", "_LAYERBLENDHEIGHT_DEF_2");
         changed = changed || UpgradeNoise(mat);
         
         if (changed)
         {
            mat.SetInt("_Version", 2021);
            EditorUtility.SetDirty(mat);
         }
         return changed;
      }

      private void OnGUI()
      {
         EditorGUILayout.HelpBox("Upgrades materials from Better Lit Shader to the 2021 version", MessageType.Info);
         if (GUILayout.Button("Do It"))
         {
            string[] guids = AssetDatabase.FindAssets("t:Material");
            List<Material> mats = new List<Material>();
            int count = 0;
            foreach (var g in guids)
            {
               var path = AssetDatabase.GUIDToAssetPath(g);
               var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
               if (mat != null && mat.shader != null)
               {
                  if ((mat.shader.name == "Better Lit/Lit") ||
                     (mat.shader.name == "Hidden/Better Lit/Lit Alpha") ||
                     (mat.shader.name == "Hidden/Better Lit/Lit Tessellation") ||
                     (mat.shader.name == "Hidden/Better Lit/Lit Tessellation Alpha"))
                  {
                     
                     if (Upgrade(mat))
                     {
                        count++;
                        Debug.Log("Upgraded material : " + mat.name);
                     }
                     
                  }
               }
            }
            Debug.Log("Found " + count + " materials needing upgrade");
         }
      }
   }
}