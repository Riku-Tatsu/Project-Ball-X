#define ALLOWPOM

//////////////////////////////////////////////////////
// Better Lit Shader
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

// This is basically all of the actual editor code, as functions.
// When the Better Lit Shader is used, these get called from the custom
// material editor. But if Better Shaders is installed and new shaders
// are built from these stackables, each has thier own custom editor
// defined in LitBaseSubMaterialEditor, which will use this shared code
// to present the same UI without the custom Better Lit specific editor. 

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace JBooth.BetterLit
{
   public class LitBaseStub
   {
      ShaderGUI shaderGUI;
      Material mat;
      public LitBaseStub(ShaderGUI sg, Material m)
      {
         shaderGUI = sg;
         mat = m;
         rolloutKeywordStates.Clear();
      }

      public enum ParallaxMode
      {
         Off,
         Parallax,
#if ALLOWPOM
         POM
#endif
      }
      
      public enum NoiseSpace
      {
         UV,
         Local,
         World
      }

      public enum Packing
      {
         Unity,
         Fastest,
         FastMetal
      }

      public enum TriplanarSpace
      {
         World,
         Local
      }

      public enum UVMode
      {
         UV,
         TriplanarUV,
         TriplanarTexturing,
      }

      public enum UVSource
      {
         UV0,
         UV1,
         ProjectX,
         ProjectY,
         ProjectZ
      }

      public enum LayerBlendMode
      {
         Multiply2X,
         AlphaBlended,
         HeightBlended,
      }

      public enum NormalMode
      {
         Textures,
         FromHeight,
         SurfaceGradient
      }

      public enum Workflow
      {
         Metallic,
         Specular
      }



      MaterialProperty FindProperty(string name, MaterialProperty[] props)
      {
         foreach (var p in props)
         {
            if (p != null && p.name == name)
               return p;
         }
         return null;
      }

      public enum TessellationMode
      {
         Edge,
         Distance
      }

      public enum AlphaMode
      {
         Opaque,
         Alpha
      }

      public bool IsUnlit() { return mat.shader.name.Contains("Unlit");  }
      public bool IsMatCap() { return mat.IsKeywordEnabled("_USEMATCAP"); }
      static System.Collections.Generic.Dictionary<string, bool> rolloutStates = new System.Collections.Generic.Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = new GUIStyle(GUI.skin.box);
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         var oldColor = GUI.contentColor;
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         if (inset == true)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
         }

         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = defaultState;
            string key = text;
            if (EditorPrefs.HasKey(key))
            {
               rolloutStates[text] = EditorPrefs.GetBool(key);
            }
         }
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20) }))
         {
            rolloutStates[text] = !rolloutStates[text];
            string key = text;
            EditorPrefs.SetBool(key, rolloutStates[text]);
         }
         if (inset == true)
         {
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
         }
         GUI.contentColor = oldColor;
         return rolloutStates[text];
      }

      static Dictionary<string, bool> rolloutKeywordStates = new System.Collections.Generic.Dictionary<string, bool>();

      public static bool DrawRollupKeywordToggle(Material mat, string text, string keyword)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = new GUIStyle(GUI.skin.box);
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         var oldColor = GUI.contentColor;

         bool toggle = mat.IsKeywordEnabled(keyword);

         if (mat.HasProperty("_HideUnused"))
         {
            bool hideUnused = mat.GetFloat("_HideUnused") > 0.5;
            if (!toggle && hideUnused)
               return toggle;
         }

         EditorGUILayout.BeginHorizontal(rolloutStyle);
         
         if (!rolloutKeywordStates.ContainsKey(keyword))
         {
            rolloutKeywordStates[keyword] = toggle;
         }
  
         var nt = EditorGUILayout.Toggle(toggle, GUILayout.Width(18));
         if (nt != toggle)
         {
            mat.DisableKeyword(keyword);
            if (nt)
            {
               mat.EnableKeyword(keyword);
               rolloutKeywordStates[keyword] = true;
            }
            EditorUtility.SetDirty(mat);
         }

         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20) }))
         {
            rolloutKeywordStates[keyword] = !rolloutKeywordStates[keyword];
         }
         EditorGUILayout.EndHorizontal();
         GUI.contentColor = oldColor;
         
         return rolloutKeywordStates[keyword];
      }


      public static bool DrawRollupToggle(Material mat, string text, ref bool toggle)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = new GUIStyle(GUI.skin.box);
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         var oldColor = GUI.contentColor;

         EditorGUILayout.BeginHorizontal(rolloutStyle);
         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = true;
            string key = text;
            if (EditorPrefs.HasKey(key))
            {
               rolloutStates[text] = EditorPrefs.GetBool(key);
            }
         }

         var nt = EditorGUILayout.Toggle(toggle, GUILayout.Width(18));
         if (nt != toggle && nt == true)
         {
            // open when changing toggle state to true
            rolloutStates[text] = true;
            EditorPrefs.SetBool(text, rolloutStates[text]);
         }
         toggle = nt;
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20) }))
         {
            rolloutStates[text] = !rolloutStates[text];
            string key = text;
            EditorPrefs.SetBool(key, rolloutStates[text]);
         }
         EditorGUILayout.EndHorizontal();
         GUI.contentColor = oldColor;
         return rolloutStates[text];
      }

      Texture2D FindDefaultTexture(string name)
      {
         var guids = AssetDatabase.FindAssets("t:Texture2D betterlit_default_");
         foreach (var g in guids)
         {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (path.Contains(name))
            {
               return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
         }
         return null;
      }

      public static void DrawSeparator()
      {
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
         EditorGUILayout.Separator();
      }

      public static void WarnLinear(Texture tex)
      {
         if (tex != null)
         {
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            if (ai != null)
            {
               TextureImporter ti = ai as TextureImporter;
               if (ti != null && ti.sRGBTexture != false)
               {
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.HelpBox("Texture is sRGB! Should be linear!", MessageType.Error);
                  if (GUILayout.Button("Fix"))
                  {
                     ti.sRGBTexture = false;
                     ti.SaveAndReimport();
                  }
                  EditorGUILayout.EndHorizontal();
               }
            }
         }
      }

      public static void WarnNormal(Texture tex)
      {
         if (tex != null)
         {
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            if (ai != null)
            {
               TextureImporter ti = ai as TextureImporter;
               if (ti != null && ti.textureType != TextureImporterType.NormalMap)
               {
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.HelpBox("Texture is set to type normal!", MessageType.Error);
                  if (GUILayout.Button("Fix"))
                  {
                     ti.textureType = TextureImporterType.NormalMap;
                     ti.SaveAndReimport();
                  }
                  EditorGUILayout.EndHorizontal();
               }
            }
         }
      }

      enum LightingModel
      {
         Unlit,
         Standard,
#if !USING_HDRP
         Simple,
#endif
#if USING_URP
         Baked
#endif
      }


      void DoLightingModel()
      {

         LightingModel model = LightingModel.Standard;
#if !USING_HDRP
         if (mat.IsKeywordEnabled("_SIMPLELIT"))
         {
            model = LightingModel.Simple;
         }
#endif
#if USING_URP
         if (mat.IsKeywordEnabled("_BAKEDLIT"))
         {
            model = LightingModel.Baked;
         }
#endif
         if (IsUnlit())
         {
            model = LightingModel.Unlit;
         }
         var nm = (LightingModel)EditorGUILayout.EnumPopup("Lighting Model", model);

         if (nm != model)
         {
#if !USING_HDRP
            mat.DisableKeyword("_SIMPLELIT");
            mat.DisableKeyword("_BAKEDLIT");
            if (nm == LightingModel.Simple)
            {
               mat.EnableKeyword("_SIMPLELIT");
            }
#endif
#if USING_URP
            if (nm == LightingModel.Baked)
            {
               mat.EnableKeyword("_BAKEDLIT");
            }
#endif

            bool tessOn;
            TessellationMode tess;
            GetTessellationState(out tess, out tessOn);
            SetChangeShader(GetAlphaState(), tess, tessOn, nm != LightingModel.Unlit);
            
         }
         
      }

      enum CullMode
      {
         Off = 0,
         Front,
         Back
      }

      enum BackfaceNormalMode
      {
         Flip = 0,
         Mirror = 1,
         None = 2
      }

      static GUIContent CCullMode = new GUIContent("Cull Mode", "(_CullMode) Do we render back faces?");
      static GUIContent CBackFaceNormal = new GUIContent("Back Face Normal", "(_DoubleSidedNormalMode) How should the normal be handled for the back side of the surface");
      public void DoCullMode()
      {
         CullMode cullMode = (CullMode)(int)mat.GetFloat("_CullMode");
         BackfaceNormalMode normalMode = (BackfaceNormalMode)(int)mat.GetFloat("_DoubleSidedNormalMode");

         EditorGUI.BeginChangeCheck();
         cullMode = (CullMode)EditorGUILayout.EnumPopup(CCullMode, cullMode);
         if (cullMode != CullMode.Back)
         {
            EditorGUI.indentLevel++;
            normalMode = (BackfaceNormalMode)EditorGUILayout.EnumPopup(CBackFaceNormal, normalMode);
            EditorGUI.indentLevel--;
         }
         if (EditorGUI.EndChangeCheck())
         {
            mat.SetFloat("_CullMode", (int)cullMode);
            mat.SetFloat("_DoubleSidedNormalMode", (int)normalMode);
            EditorUtility.SetDirty(mat);
         }
      }

      void GetTessellationState(out TessellationMode mode, out bool enabled)
      {
         enabled = mat.HasProperty("_TessellationMaxSubdiv");
         mode = TessellationMode.Distance;
         if (mat.IsKeywordEnabled("_TESSEDGE"))
         {
            mode = TessellationMode.Edge;
         }

      }

      AlphaMode GetAlphaState()
      {
         return mat.HasProperty("_IsAlpha") ? AlphaMode.Alpha : AlphaMode.Opaque;
      }

      void SetChangeShader(AlphaMode alpha, TessellationMode tess, bool tessOn, bool lit)
      {
         EditorUtility.SetDirty(mat);

         string shaderName = "";

         if (!lit)
         {
            if (alpha == AlphaMode.Alpha)
            {
               shaderName += "Hidden/Better Lit/Unlit Alpha";
            }
            else
            {
               shaderName += "Hidden/Better Lit/Unlit";
            }
         }
         else if (alpha == AlphaMode.Alpha && tessOn)
         {
            shaderName += "Hidden/Better Lit/Lit Tessellation Alpha";
         }
         else if (tessOn)
         {
            shaderName += "Hidden/Better Lit/Lit Tessellation";
         }
         else if (alpha == AlphaMode.Alpha)
         {
            shaderName += "Hidden/Better Lit/Lit Alpha";
         }
         else
         {
            shaderName += "Better Lit/Lit";
         }

         if (tess == TessellationMode.Edge)
         {
            mat.EnableKeyword("_TESSEDGE");
         }
         else
         {
            mat.DisableKeyword("_TESSEDGE");
         }
         Shader s = Shader.Find(shaderName);
         if (s != null && mat.shader != s)
         {
            changeShader = s;
            EditorUtility.SetDirty(mat);
         }
      }

      GUIContent CTessMethod = new GUIContent("Method", "Edge based tessellation tried to keep a consistant edge size, Distance based is based off distance from the camera. Edge can be a little less stable as it's view angle dependent, but is usually a bit more performant");
      // This is only for the final shader editor, not for better shaders.
      public void DoTessellationOption(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         AlphaMode alpha = GetAlphaState();
         TessellationMode tess;
         bool tessOn = false;
         GetTessellationState(out tess, out tessOn);

         if (mat.HasProperty("_HideUnused"))
         {
            bool hideUnused = mat.GetFloat("_HideUnused") > 0.5;
            if (!tessOn && hideUnused)
               return;
         }

         EditorGUI.BeginChangeCheck();
         bool show = (DrawRollupToggle(mat, "Tessellation", ref tessOn));
         if (EditorGUI.EndChangeCheck())
         {
            SetChangeShader(alpha, tess, tessOn, !IsUnlit());
         }
         if (tessOn && show)
         {
            EditorGUI.BeginChangeCheck();
            tess = (TessellationMode)EditorGUILayout.EnumPopup(CTessMethod, tess);
            if (EditorGUI.EndChangeCheck())
            {
               SetChangeShader(alpha, tess, tessOn, !IsUnlit());
            }

            if (tessOn)
            {
               OnTessGUI(materialEditor, props);
            }
         }
      }

      // This is only for the final shader editor, not for better shaders.
      public void DoAlphaOptions()
      {
         AlphaMode alpha = GetAlphaState();
         TessellationMode tess;
         bool tessOn = false;
         GetTessellationState(out tess, out tessOn);

         EditorGUI.BeginChangeCheck();

         alpha = (AlphaMode)EditorGUILayout.EnumPopup("Opacity", alpha);


         if (EditorGUI.EndChangeCheck())
         {
            SetChangeShader(alpha, tess, tessOn, !IsUnlit());
         }
      }

      public Packing GetPacking()
      {
         Packing packing = Packing.Unity;
         if (mat.IsKeywordEnabled("_PACKEDFAST"))
         {
            packing = Packing.Fastest;
         }
         else if (mat.IsKeywordEnabled("_PACKEDFASTMETAL"))
         {
            packing = Packing.FastMetal;
         }
         return packing;
      }

      GUIContent CPacking = new GUIContent("Texture Packing", "Unity : PBR Data is packed into 3 textures, Fastest : Packed into 2 textures. FastMetal : 2 texture packing with metal instead of AO. See docs for packing format");
      Packing DoPacking(Material mat)
      {
         Packing packing = GetPacking();

         var np = (Packing)EditorGUILayout.EnumPopup(CPacking, packing);

         if (np != packing)
         {
            mat.DisableKeyword("_PACKEDFAST");
            mat.DisableKeyword("_PACKEDFASTMETAL");
            if (np == Packing.Fastest)
            {
               mat.EnableKeyword("_PACKEDFAST");
            }
            else if (np == Packing.FastMetal)
            {
               mat.EnableKeyword("_PACKEDFASTMETAL");
            }
            EditorUtility.SetDirty(mat);
         }
         return np;
      }

      enum TriplanarBaryMode
      {
         Standard,
         FlatBlend,
         BarycentricBlend
      }
      static GUIContent CTriplanarContrast = new GUIContent("Triplanar Contrast", "How tight is the blend between triplanar projections");
      static GUIContent CTriplanarSpace = new GUIContent("Triplanar Space", "What space is the triplanar projection in?");
      static GUIContent CTriplanarBaryBlend = new GUIContent("Vertex -> Flat", "Blend between vertex normal (smooth) and flat normal (hard)");
      static GUIContent CTriplanarBlendMode = new GUIContent("Projection Mode", "Standard triplanar, a linear blend between soft and hard normals, or use the barycentric coordinates (which must be baked into the mesh with the mesh converter) to blend only the edges of faces smoothly");
      TriplanarSpace DoTriplanarSpace(Material mat, MaterialEditor materialEditor, MaterialProperty[] props, string spaceProp, string contrastProp,
         string baryKeyword, string baryProp, string flatKeyword)
      {
         EditorGUI.indentLevel++;
         TriplanarSpace space = TriplanarSpace.World;
         if (mat.GetFloat(spaceProp) > 0.5)
            space = TriplanarSpace.Local;

         EditorGUI.BeginChangeCheck();
         space = (TriplanarSpace)EditorGUILayout.EnumPopup(CTriplanarSpace, space);
         if (EditorGUI.EndChangeCheck())
         {
            mat.SetFloat(spaceProp, (int)space);
            EditorUtility.SetDirty(mat);
         }
         
         materialEditor.ShaderProperty(FindProperty(contrastProp, props), CTriplanarContrast);


         if (space == TriplanarSpace.World)
         {
            TriplanarBaryMode mode = TriplanarBaryMode.Standard;
            if (mat.IsKeywordEnabled(baryKeyword))
               mode = TriplanarBaryMode.BarycentricBlend;
            else if (mat.IsKeywordEnabled(flatKeyword))
               mode = TriplanarBaryMode.FlatBlend;

            if (mode == TriplanarBaryMode.BarycentricBlend)
            {
               EditorGUILayout.HelpBox("When set to barcentric blend, barycentric coordinates must be baked onto the mesh with the mesh converter!", MessageType.Warning);
            }
            EditorGUI.BeginChangeCheck();
            mode = (TriplanarBaryMode)EditorGUILayout.EnumPopup(CTriplanarBlendMode, mode);
            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword(baryKeyword);
               mat.DisableKeyword(flatKeyword);
               if (mode == TriplanarBaryMode.BarycentricBlend)
                  mat.EnableKeyword(baryKeyword);
               else if (mode == TriplanarBaryMode.FlatBlend)
                  mat.EnableKeyword(flatKeyword);
            }
            if (mode != TriplanarBaryMode.Standard)
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty(baryProp, props), CTriplanarBaryBlend);
               EditorGUI.indentLevel--;
            }
         }
         else if (mat.IsKeywordEnabled(baryKeyword) || mat.IsKeywordEnabled(flatKeyword))
         {
            mat.DisableKeyword(baryKeyword);
            mat.DisableKeyword(flatKeyword);
         }
         EditorGUI.indentLevel--;
         return space;
      }


      static GUIContent CFlatShadingBlend = new GUIContent("Flat Shading", "Flat shading adjusts the normals from smoothed towards faceted");
      public void DoFlatShadingMode( MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         bool mode = false;
         if (mat.IsKeywordEnabled("_FLATSHADE"))
         {
            mode = true;
         }
         EditorGUILayout.BeginHorizontal();
         EditorGUI.BeginChangeCheck();
         mode = EditorGUILayout.Toggle(mode, GUILayout.Width(24));
         if (EditorGUI.EndChangeCheck())
         {
            if (mode)
            {
               mat.EnableKeyword("_FLATSHADE");
            }
            else
            {
               mat.DisableKeyword("_FLATSHADE");
            }
            EditorUtility.SetDirty(mat);
         }

         var old = GUI.enabled;
         GUI.enabled = mode;

         materialEditor.ShaderProperty(FindProperty("_FlatShadingBlend", props), CFlatShadingBlend);

         EditorGUILayout.EndHorizontal();
         GUI.enabled = old;
      }

      GUIContent CStochastic = new GUIContent("Stochastic", "Prevents visible tiling on surfaces");
      GUIContent CStochatsicContrast = new GUIContent("Stochastic Contrast", "How tight the blend between stochastic clusers is");
      GUIContent CStochasticScale = new GUIContent("Stochastic Scale", "How large the patches of texture are before blending into the next area");
      bool DoStochastic(Material mat, MaterialEditor materialEditor, MaterialProperty[] props, string keyword, string prop, string prop2)
      {
         bool mode = false;
         if (mat.IsKeywordEnabled(keyword))
         {
            mode = true;
         }
         EditorGUI.BeginChangeCheck();
         mode = EditorGUILayout.Toggle(CStochastic, mode);
         if (EditorGUI.EndChangeCheck())
         {
            if (mode)
            {
               mat.EnableKeyword(keyword);
            }
            else
            {
               mat.DisableKeyword(keyword);
            }
            EditorUtility.SetDirty(mat);
         }

         var old = GUI.enabled;
         GUI.enabled = mode;
         if (mode)
         {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty(prop, props), CStochatsicContrast);
            materialEditor.ShaderProperty(FindProperty(prop2, props), CStochasticScale);
            EditorGUI.indentLevel--;
         }
         GUI.enabled = old;

         return mode;
      }

      public NormalMode GetNormalMode()
      {
         NormalMode normalMode = mat.IsKeywordEnabled("_AUTONORMAL") ? NormalMode.FromHeight : NormalMode.Textures;
         if (mat.IsKeywordEnabled("_SURFACEGRADIENT"))
         {
            normalMode = NormalMode.SurfaceGradient;
         }
         return normalMode;
      }

      GUIContent CNormalMode = new GUIContent("Normal Mode", "Use traditional normal textures, generate them from the height map, or use the surface gradient framework for slightly higher quality normals when blending normals");
      GUIContent CAutoNormalStrength = new GUIContent("Normal From Height Strength", "(_AutoNormalStrength) How strong the faked normal map effect is");
      public NormalMode DoNormalMode(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return NormalMode.Textures;
         NormalMode normalMode = GetNormalMode();
         EditorGUI.BeginChangeCheck();
         if (normalMode == NormalMode.FromHeight)
         {
            EditorGUILayout.HelpBox("Albedo must have height values in the alpha channel!", MessageType.Info);
         }

         normalMode = (NormalMode)EditorGUILayout.EnumPopup(CNormalMode, normalMode);
         if (EditorGUI.EndChangeCheck())
         {
            if (normalMode == NormalMode.FromHeight)
            {
               mat.EnableKeyword("_AUTONORMAL");
            }
            else
            {
               mat.DisableKeyword("_AUTONORMAL");
            }
            if (normalMode == NormalMode.SurfaceGradient)
            {
               mat.EnableKeyword("_SURFACEGRADIENT");
            }
            else
            {
               mat.DisableKeyword("_SURFACEGRADIENT");
            }
         }


         if (normalMode == NormalMode.FromHeight)
         {
            materialEditor.ShaderProperty(FindProperty("_AutoNormalStrength", props), CAutoNormalStrength);
         }
         return normalMode;

      }



      UVMode DoUVMode(Material mat, string triKeyword, string triProjKeyword, string label)
      {
         UVMode uvMode = UVMode.UV;
         if (mat.IsKeywordEnabled(triKeyword))
         {
            uvMode = UVMode.TriplanarUV;
         }
         else if (mat.IsKeywordEnabled(triProjKeyword))
         {
            uvMode = UVMode.TriplanarTexturing;
         }
         EditorGUI.BeginChangeCheck();
         uvMode = (UVMode)EditorGUILayout.EnumPopup(label, uvMode);
         if (EditorGUI.EndChangeCheck())
         {
            mat.DisableKeyword(triKeyword);
            mat.DisableKeyword(triProjKeyword);
            if (uvMode == UVMode.TriplanarUV)
            {
               mat.EnableKeyword(triKeyword);
            }
            else if (uvMode == UVMode.TriplanarTexturing)
            {
               mat.EnableKeyword(triProjKeyword);
            }
            EditorUtility.SetDirty(mat);
         }
         return uvMode;

      }

      static GUIContent CNoiseSpace = new GUIContent("Noise Space", "Space used to generate the noise - 3d noise used in world and local space");
      NoiseSpace DoNoiseSpace(string prefix, string def)
      {
         NoiseSpace noiseSpace = NoiseSpace.UV;

         if (mat.IsKeywordEnabled(prefix + "NOISELOCAL" + def))
            noiseSpace = NoiseSpace.Local;
         if (mat.IsKeywordEnabled(prefix + "NOISEWORLD" + def))
            noiseSpace = NoiseSpace.World;


         EditorGUI.BeginChangeCheck();
         noiseSpace = (NoiseSpace)EditorGUILayout.EnumPopup(CNoiseSpace, noiseSpace);

         if (EditorGUI.EndChangeCheck())
         {
            mat.DisableKeyword(prefix + "NOISEWORLD" + def);
            mat.DisableKeyword(prefix + "NOISELOCAL" + def);
            if (noiseSpace == NoiseSpace.World)
            {
               mat.EnableKeyword(prefix + "NOISEWORLD" + def);
            }
            else if (noiseSpace == NoiseSpace.Local)
            {
               mat.EnableKeyword(prefix + "NOISELOCAL" + def);
            }
            EditorUtility.SetDirty(mat);
         }
         return noiseSpace;

      }

      enum NoiseQuality
      {
         Texture,
         ProceduralLow,
         ProceduralHigh,
         Worley,

      }

      GUIContent CNoiseQuality = new GUIContent("Noise Quality", "Texture based (fastest), 1 octave of value noise, 3 octaves of value noise, worley noise");
      NoiseQuality DoNoiseQuality(string prefix, string ext, string def, string texprefix, MaterialEditor materialEditor, MaterialProperty[] props, bool noiseForced = false)
      {
         NoiseQuality noiseQuality = NoiseQuality.ProceduralLow;
         if (noiseForced)
         {
            noiseQuality = NoiseQuality.Texture;
         }

         if (mat.IsKeywordEnabled(prefix + "NOISETEXTURE" + def))
         {
            noiseQuality = NoiseQuality.Texture;
         }
         else if (mat.IsKeywordEnabled(prefix + "NOISEHQ" + def))
         {
            noiseQuality = NoiseQuality.ProceduralHigh;
         }
         else if (mat.IsKeywordEnabled(prefix + "NOISEWORLEY" + def))
         {
            noiseQuality = NoiseQuality.Worley;
         }
         else if (mat.IsKeywordEnabled(prefix + "NOISELQ" + def))
         {
            noiseQuality = NoiseQuality.ProceduralLow;
         }

         var nq = (NoiseQuality)EditorGUILayout.EnumPopup(CNoiseQuality, noiseQuality);
         if (nq != noiseQuality)
         {
            mat.DisableKeyword(prefix + "NOISETEXTURE" + def);
            mat.DisableKeyword(prefix + "NOISEHQ" + def);
            mat.DisableKeyword(prefix + "NOISEWORLEY" + def);
            mat.DisableKeyword(prefix + "NOISELQ" + def);
            if (nq == NoiseQuality.Texture && noiseForced == false)
            {
               mat.EnableKeyword(prefix + "NOISETEXTURE" + def);
            }
            else if (nq == NoiseQuality.ProceduralLow && noiseForced == true)
            {
               mat.EnableKeyword(prefix + "NOISELQ" + def);
            }
            else if (nq == NoiseQuality.ProceduralHigh)
            {
               mat.EnableKeyword(prefix + "NOISEHQ" + def);
            }
            else if (nq == NoiseQuality.Worley)
            {
               mat.EnableKeyword(prefix + "NOISEWORLEY" + def);
            }
         }

         if (nq == NoiseQuality.Texture && noiseForced == false)
         {
            var prop = FindProperty(texprefix + "NoiseTex" + ext, props);
            if (prop.textureValue == null)
            {
               prop.textureValue = FindDefaultTexture("betterlit_default_noise");
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Noise Texture"), prop);
         }
         return nq;
      }



      ParallaxMode DoParallax(Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         ParallaxMode mode = ParallaxMode.Off;
         if (mat.IsKeywordEnabled("_PARALLAX"))
            mode = ParallaxMode.Parallax;
#if ALLOWPOM
         else if (mat.IsKeywordEnabled("_POM"))
            mode = ParallaxMode.POM;
#endif

         EditorGUI.BeginChangeCheck();
         EditorGUILayout.BeginHorizontal();
         mode = (ParallaxMode)EditorGUILayout.EnumPopup("Parallax", mode);
         bool old = GUI.enabled;
         GUI.enabled = mode != ParallaxMode.Off;
         materialEditor.ShaderProperty(FindProperty("_ParallaxHeight", props), "");
         GUI.enabled = old;
         EditorGUILayout.EndHorizontal();
#if ALLOWPOM
      if (mode == ParallaxMode.POM)
      {
         bool warn = false;
         foreach(var s in mat.shaderKeywords)
         {
            if (s.Contains("TRIPLANAR"))
               warn = true;
         }
         if (warn)
         {
            EditorGUILayout.HelpBox("POM only works with standard UVs, triplanar or world space UVs will not give correct results", MessageType.Warning);
         }
         EditorGUI.indentLevel++;
         materialEditor.ShaderProperty(FindProperty("_POMMaxSamples", props), "Max Samples");
         materialEditor.ShaderProperty(FindProperty("_POMMin", props), "Fade Begin");
         materialEditor.ShaderProperty(FindProperty("_POMFade", props), "Fade Range");
         EditorGUI.indentLevel--;
      }
#endif
         if (EditorGUI.EndChangeCheck())
         {
            mat.DisableKeyword("_PARALLAX");
            mat.DisableKeyword("_POM");
            if (mode == ParallaxMode.Parallax)
            {
               mat.EnableKeyword("_PARALLAX");
            }

#if ALLOWPOM
            else if (mode == ParallaxMode.POM)
            {
               mat.EnableKeyword("_POM");
            }
#endif
            EditorUtility.SetDirty(mat);
         }
         return mode;
      }

      Workflow GetWorkflow()
      {
         Workflow workflow = Workflow.Metallic;
         if (mat.IsKeywordEnabled("_SPECULAR"))
         {
            workflow = Workflow.Specular;
         }
         return workflow;
      }

      Workflow DoWorkflow()
      {
         Workflow workflow = GetWorkflow();

         var nw = (Workflow)EditorGUILayout.EnumPopup("Workflow", workflow);
         if (nw != workflow)
         {
            if (nw == Workflow.Metallic)
            {
               mat.DisableKeyword("_SPECULAR");
            }
            else
            {
               mat.EnableKeyword("_SPECULAR");
            }
         }
         return nw;
      }

      UVSource DoUVSource(Material mat, string propName)
      {
         UVSource s = (UVSource)((int)mat.GetFloat(propName));
         EditorGUI.BeginChangeCheck();
         s = (UVSource)EditorGUILayout.EnumPopup("UV Source", s);
         if (EditorGUI.EndChangeCheck())
         {
            mat.SetFloat(propName, (float)s);
         }
         return s;
      }

      enum MaskMode
      {
         Vertex,
         Texture
      }

      static GUIContent CWireSpace = new GUIContent("Wire Space", "(_Wire) Space to use for wire scaling");
      static GUIContent CWireThickness = new GUIContent("Wire Thickness", "(_WireThickness) Size of wire frame effect");
      static GUIContent CWireSmoothing = new GUIContent("Wire Smoothing", "(_WireSmoothing) Softness of wire edge");
      static GUIContent CWireAlbedo = new GUIContent("Wire Albedo", "(_WireAlbedo) Apply wire effect to albedo?");
      static GUIContent CWireEmissive = new GUIContent("Wire Emissive", "(_WireEmis) Apply wire effect to emissive?");
      static GUIContent CWireAlpha = new GUIContent("Wire Alpha", "(_WireAlpha) Apply wire effect to alpha?");
      static GUIContent CWireUseEffector = new GUIContent("Use Effector", "When enabled, rendering of wireframe is controlled by disastance to effector objects");
      static GUIContent CWireEffectorInvert = new GUIContent("Invert", "Invert the effector effect");
      public void DoWireframe(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_WireSmoothing"))
            return;
         if (DrawRollupKeywordToggle(mat, "Wireframe", "_WIREFRAME"))
         {
            EditorGUILayout.HelpBox("Mesh must be processed with Better Lit mesh processor for barycentrics before wireframe will work", MessageType.Info);
            materialEditor.ShaderProperty(FindProperty("_Wire", props), CWireSpace);
            materialEditor.ShaderProperty(FindProperty("_WireThickness", props), CWireThickness);
            materialEditor.ShaderProperty(FindProperty("_WireSmoothing", props), CWireSmoothing);
            EditorGUILayout.BeginHorizontal();
            materialEditor.ShaderProperty(FindProperty("_WireAlbedo", props), CWireAlbedo);
            materialEditor.ShaderProperty(FindProperty("_WireColor", props), "");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            materialEditor.ShaderProperty(FindProperty("_WireEmis", props), CWireEmissive);
            materialEditor.ShaderProperty(FindProperty("_WireEmissive", props), "");
            EditorGUILayout.EndHorizontal();
            materialEditor.ShaderProperty(FindProperty("_WireAlpha", props), CWireAlpha);

            materialEditor.ShaderProperty(FindProperty("_WireUseEffector", props), CWireUseEffector);
            if (mat.IsKeywordEnabled("_WIREUSEEFFECTOR"))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_WireEffectorInvert", props), CWireEffectorInvert);
               EditorGUI.indentLevel--;
            }
         }
      }


      public void SetEffectorChannelKeywords()
      {
         // make sure we're really using it in one of the effects and set the keyword
         if (mat.IsKeywordEnabled("_WIREFRAME") && mat.IsKeywordEnabled("_WIREUSEEFFECTOR") ||
            mat.IsKeywordEnabled("_DISSOLVE_EFFECTOR") && mat.IsKeywordEnabled("_DISSOLVE") ||
            mat.IsKeywordEnabled("_LAYEREFFECTOR") && mat.IsKeywordEnabled("_USELAYER") ||
            mat.IsKeywordEnabled("_LAYEREFFECTOR_DEF_1") && mat.IsKeywordEnabled("_USELAYER_DEF_1") ||
            mat.IsKeywordEnabled("_LAYEREFFECTOR_DEF_2") && mat.IsKeywordEnabled("_USELAYER_DEF_2") ||
            mat.IsKeywordEnabled("_LAYEREFFECTOR_DEF_3") && mat.IsKeywordEnabled("_USELAYER_DEF_3")
            )
         {
            // must set property since component has a toggle property,
            // other wise they will fight..
            if (!mat.IsKeywordEnabled("_EFFECTORENABLED"))
            {
               mat.EnableKeyword("_EFFECTORENABLED");
               mat.SetFloat("_EnableEffectorSystem", 1);
               EditorUtility.SetDirty(mat);
            }
         }
         else  
         {
            if (mat.IsKeywordEnabled("_EFFECTORENABLED"))
            {
               mat.DisableKeyword("_EFFECTORENABLED");
               mat.SetFloat("_EnableEffectorSystem", 0);
               EditorUtility.SetDirty(mat);
            }
         }
      }


      GUIContent CTextureLayerWeights = new GUIContent("Texture Layer Weights", "Do we weight the texture layers with the vertex colors or with a texture?");
      GUIContent CCurveWeights = new GUIContent("Curve Weights", "Allows you to tighten the interpolation between textures towards a spline like transition");
      public void DoTextureLayerWeights(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         bool on = mat.IsKeywordEnabled("_LAYERVERTEXMASK") || mat.IsKeywordEnabled("_LAYERTEXTUREMASK");
         if (mat.HasProperty("_HideUnused"))
         {
            bool hideUnused = mat.GetFloat("_HideUnused") > 0.5;
            if (!on && hideUnused)
               return;
         }
         EditorGUI.BeginChangeCheck();
         bool show = (DrawRollupToggle(mat, "Texture Layer Weights", ref on));
         if (EditorGUI.EndChangeCheck())
         {
            if (!on)
            {
               mat.DisableKeyword("_LAYERVERTEXMASK");
               mat.DisableKeyword("_LAYERTEXTUREMASK");
            }
            else if (on)
            {
               if (!mat.IsKeywordEnabled("_LAYERVERTEXMASK") && !mat.IsKeywordEnabled("_LAYERTEXTUREMASK"))
               {
                  mat.EnableKeyword("_LAYERVERTEXMASK");
               }
            }
         }


         if (show)
         {
            var old = GUI.enabled;
            GUI.enabled = on;
            MaskMode mask = MaskMode.Vertex;
            if (mat.IsKeywordEnabled("_LAYERVERTEXMASK"))
            {
               mask = MaskMode.Vertex;
            }
            else if (mat.IsKeywordEnabled("_LAYERTEXTUREMASK"))
            {
               mask = MaskMode.Texture;
            }

            EditorGUI.BeginChangeCheck();
            mask = (MaskMode)EditorGUILayout.EnumPopup(CTextureLayerWeights, mask);

            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword("_LAYERVERTEXMASK");
               mat.DisableKeyword("_LAYERTEXTUREMASK");
               if (on)
               {
                  if (mask == MaskMode.Vertex)
                  {
                     mat.EnableKeyword("_LAYERVERTEXMASK");
                  }
                  else if (mask == MaskMode.Texture)
                  {
                     mat.EnableKeyword("_LAYERTEXTUREMASK");
                  }
               }
            }
            if (on && mask == MaskMode.Texture)
            {
               EditorGUI.indentLevel++;
               DoUVSource(mat, "_LayerTextureMaskUVMode");
               materialEditor.TexturePropertySingleLine(new GUIContent("Texture Mask", "An RGBA texture which masks each texture layer"), FindProperty("_LayerTextureMask", props));
               materialEditor.TextureScaleOffsetProperty(FindProperty("_LayerTextureMask", props));
               bool curve = mat.IsKeywordEnabled("_LAYERCURVEWEIGHTS");
               bool newCurve = EditorGUILayout.Toggle("Curve Weights", curve);
               if (curve != newCurve)
               {
                  mat.DisableKeyword("_LAYERCURVEWEIGHTS");
                  if (newCurve)
                  {
                     mat.EnableKeyword("_LAYERCURVEWEIGHTS");
                  }
                  EditorUtility.SetDirty(mat);
               }
               if (newCurve)
               {
                  Vector4 v = mat.GetVector("_LayerTextureMaskCurveWeights");
                  Vector4 vn = v;
                  EditorGUI.indentLevel++;
                  vn.x = EditorGUILayout.Slider("R", vn.x, 0, 0.5f);
                  vn.y = EditorGUILayout.Slider("G", vn.y, 0, 0.5f);
                  vn.z = EditorGUILayout.Slider("B", vn.z, 0, 0.5f);
                  vn.w = EditorGUILayout.Slider("A", vn.w, 0, 0.5f);
                  if (vn != v)
                  {
                     mat.SetVector("_LayerTextureMaskCurveWeights", vn);
                     EditorUtility.SetDirty(mat);
                  }
                  EditorGUI.indentLevel--;
               }
               EditorGUI.indentLevel--;
            }
            GUI.enabled = old;
         }
      }

      enum TintBlendMode
      {
         Multiply,
         Multiply2X,
      }

      public void DoTintMask(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_TintMask"))
            return;
         if (DrawRollupKeywordToggle(mat, "Tint Mask", "_TINTMASK"))
         {
            TintBlendMode blendMode = TintBlendMode.Multiply2X;
            if (mat.IsKeywordEnabled("_TINTMASK_MULT"))
               blendMode = TintBlendMode.Multiply;

            TintBlendMode nbm = (TintBlendMode)EditorGUILayout.EnumPopup("Blend Mode", blendMode);

            if (nbm != blendMode)
            {
               mat.DisableKeyword("_TINTMASK_MULT");
               if (nbm == TintBlendMode.Multiply)
               {
                  mat.EnableKeyword("_TINTMASK_MULT");
               }
               EditorUtility.SetDirty(mat);
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Tint Mask"), FindProperty("_TintMask", props));
            materialEditor.TextureScaleOffsetProperty(FindProperty("_TintMask", props));
            materialEditor.ColorProperty(FindProperty("_RColor", props), "R Color");
            materialEditor.ColorProperty(FindProperty("_GColor", props), "G Color");
            materialEditor.ColorProperty(FindProperty("_BColor", props), "B Color");
            materialEditor.ColorProperty(FindProperty("_AColor", props), "A Color");
         }
      }

      public void DoDissolve(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_DissolveTexture"))
            return;
         if (DrawRollupKeywordToggle(mat, "Dissolve", "_DISSOLVE"))
         {
            var noiseSpace = DoNoiseSpace("_DISSOLVE", "");
            var nq = DoNoiseQuality("_DISSOLVE", "", "", "", materialEditor, props, true);
            if (nq == NoiseQuality.Texture)
            {
               var prop = FindProperty("_DissolveTexture", props);
               materialEditor.TexturePropertySingleLine(new GUIContent("Texture"), FindProperty("_DissolveTexture", props));
               if (prop.textureValue == null)
               {
                  prop.textureValue = FindDefaultTexture("betterlit_default_noise");
               }
               if (noiseSpace == NoiseSpace.UV)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty("_DissolveTexture", props));
               }
               else
               {
                  EditorGUI.indentLevel++;
                  materialEditor.ShaderProperty(FindProperty("_DissolveNoiseFrequency", props), "Noise Frequency");
                  materialEditor.ShaderProperty(FindProperty("_DissolveNoiseOffset", props), "Noise Offset");
                  EditorGUI.indentLevel--;
               }
            }
            else
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_DissolveNoiseFrequency", props), "Noise Frequency");
               materialEditor.ShaderProperty(FindProperty("_DissolveNoiseAmplitude", props), "Noise Amplitude");
               materialEditor.ShaderProperty(FindProperty("_DissolveNoiseCenter", props), "Noise Center");
               materialEditor.ShaderProperty(FindProperty("_DissolveNoiseOffset", props), "Noise Offset");
               EditorGUI.indentLevel--;
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Gradient"), FindProperty("_DissolveGradient", props));
            materialEditor.RangeProperty(FindProperty("_DissolveAmount", props), "Amount");
            materialEditor.RangeProperty(FindProperty("_DissolveColoration", props), "Colorization");
            materialEditor.RangeProperty(FindProperty("_DissolveEdgeContrast", props), "Contrast");
            materialEditor.RangeProperty(FindProperty("_DissolveEmissiveStr", props), "Emissive Strength");

            materialEditor.ShaderProperty(FindProperty("_DissolveEffector", props), "Use Effector");
            if (mat.IsKeywordEnabled("_DISSOLVE_EFFECTOR"))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_DissolveEffectorInvert", props), "Invert");
               EditorGUI.indentLevel--;
            }
         }
      }

      GUIContent CTraxPackedNormal = new GUIContent("Packed Map", "Normal in fastest packed format, see docs for details");
      public void DoTrax(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (DrawRollupKeywordToggle(mat, "Trax", "_TRAX_ON"))
         {
            materialEditor.TexturePropertySingleLine(new GUIContent("Albedo"), FindProperty("_TraxAlbedo", props), FindProperty("_TraxTint", props));
            materialEditor.TextureScaleOffsetProperty(FindProperty("_TraxAlbedo", props));
            if (!IsUnlit())
            {
               materialEditor.TexturePropertySingleLine(CTraxPackedNormal, FindProperty("_TraxPackedNormal", props), FindProperty("_TraxNormalStrength", props));
            }
            materialEditor.RangeProperty(FindProperty("_TraxInterpContrast", props), "Interpolation Contrast");
            materialEditor.RangeProperty(FindProperty("_TraxHeightContrast", props), "Height Blend Contrast");
            if (mat.HasProperty("_TessellationMaxSubdiv"))
            {
               materialEditor.FloatProperty(FindProperty("_TraxDisplacementDepth", props), "Trax Depression Depth");
               materialEditor.RangeProperty(FindProperty("_TraxDisplacementStrength", props), "Trax Displacement Strength");
               materialEditor.RangeProperty(FindProperty("_TraxMipBias", props), "Trax Mip Bias");
            }
         }
      }

      public void DoBakery(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         if (DrawRollupKeywordToggle(mat, "Bakery", "USEBAKERY"))
         {
            materialEditor.ShaderProperty(FindProperty("_LightmapMode", props), "LightMap Mode");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_VERTEXLMMASK", props), "Enable vertex shadowmask");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_SHNONLINEAR", props), "SH non-linear mode");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_LMSPEC", props), "Enable Lightmap Specular");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_BICUBIC", props), "Enable Bicubic Filter");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_VOLUME", props), "Use volumes");
            materialEditor.ShaderProperty(FindProperty("_BAKERY_VOLROTATION", props), "Allow volume rotation");
         }
         else if (!mat.IsKeywordEnabled("USEBAKERY"))
         {
            FindProperty("_LightmapMode", props).floatValue = 0;
            FindProperty("_BAKERY_VERTEXLMMASK", props).floatValue = 0;
            FindProperty("_BAKERY_SHNONLINEAR", props).floatValue = 0;
            FindProperty("_BAKERY_LMSPEC", props).floatValue = 0;
            FindProperty("_BAKERY_BICUBIC", props).floatValue = 0;
            FindProperty("_BAKERY_VOLUME", props).floatValue = 0;
            FindProperty("_BAKERY_VOLROTATION", props).floatValue = 0;
         }
      }

      enum WindUVSpace
      {
         World,
         UV
      }

      public void DoWind(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_WindParticulateSpace"))
            return;
         if (DrawRollupKeywordToggle(mat, "Wind", "_WIND_ON"))
         {
            WindUVSpace space = (WindUVSpace)(int)mat.GetFloat("_WindParticulateSpace");
            EditorGUI.BeginChangeCheck();
            space = (WindUVSpace)EditorGUILayout.EnumPopup(new GUIContent("UV Space"), space);
            if (EditorGUI.EndChangeCheck())
            {
               FindProperty("_WindParticulateSpace", props).floatValue = (int)space;
            }
            var tex = FindProperty("_WindParticulate", props);
            if (mat.IsKeywordEnabled("_WIND_ON"))
            {
               if (tex.textureValue == null)
               {
                  tex.textureValue = FindDefaultTexture("betterlit_default_wind");
                  mat.SetTextureScale("_WindParticulate", new Vector2(0.05f, 0.2f));
               }
               WarnLinear(tex.textureValue);
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Wind Texture"), tex);
            Vector2 scale = mat.GetTextureScale("_WindParticulate");
            EditorGUI.BeginChangeCheck();
            scale.x = EditorGUILayout.FloatField("Length", scale.x);
            scale.y = EditorGUILayout.FloatField("Width", scale.y);
            if (EditorGUI.EndChangeCheck())
            {
               mat.SetTextureScale("_WindParticulate", scale);
            }
            materialEditor.RangeProperty(FindProperty("_WindParticulateStrength", props), "Strength");
            materialEditor.FloatProperty(FindProperty("_WindParticulateSpeed", props), "Speed");
            materialEditor.RangeProperty(FindProperty("_WindParticulatePower", props), "Power");
            materialEditor.FloatProperty(FindProperty("_WindParticulateRotation", props), "Rotation");
            materialEditor.ColorProperty(FindProperty("_WindParticulateColor", props), "Color");
            materialEditor.VectorProperty(FindProperty("_WindParticulateWorldHeightMask", props), "World Height Mask");
            materialEditor.RangeProperty(FindProperty("_WindParticulateTextureHeight", props), "Heightfield Height");
            materialEditor.VectorProperty(FindProperty("_WindParticulateAngleMask", props), "Angle Mask");
            materialEditor.RangeProperty(FindProperty("_WindParticulateOcclusionStrength", props), "Occlusion Strength");
            materialEditor.ColorProperty(FindProperty("_WindParticulateEmissive", props), "Emissive");
         }
      }

      enum WorldLocalSpace
      {
         World,
         Local,
      };

      public void DoTextureLayer(MaterialEditor materialEditor, MaterialProperty[] props, Packing packing, int index)
      {
         string ext = "";
         string def = "";
         if (index > 0)
         {
            def = "_DEF_" + index;
            ext = "_Ext_" + index;
         }
         if (!mat.HasProperty("_LayerTriplanarSpace" + ext))
            return;

         bool rollup = DrawRollupKeywordToggle(mat, "Texture Layer " + index, "_USELAYER" + def);

         if (rollup)
         {
            EditorGUI.indentLevel++;
            UVMode layerUVMode = DoUVMode(mat, "_LAYERTRIPLANAR" + def, "_LAYERTRIPLANARPROJECTION" + def, "Layer UV Mode");
            if (layerUVMode != UVMode.UV)
            {
               DoTriplanarSpace(mat, materialEditor, props, "_LayerTriplanarSpace" + ext,
                  "_LayerTriplanarContrast" + ext, "_LAYERTRIPLANARBARYBLEND" + def, "_LayerTriplanarBaryBlend" + ext,
                  "_LAYERTRIPLANARFLATBLEND" + def);
            }
            else
            {
               var uvSpace = DoUVSource(mat, "_LayerUVSource" + ext);
               if (uvSpace != UVSource.UV0 && uvSpace != UVSource.UV1)
               {
                  EditorGUI.BeginChangeCheck();
                  TriplanarSpace space = TriplanarSpace.World;
                  if (mat.GetFloat("_LayerTriplanarSpace" + ext) > 0.5)
                     space = TriplanarSpace.Local;
                  space = (TriplanarSpace)EditorGUILayout.EnumPopup("Projection Space", space);
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetFloat("_LayerTriplanarSpace" + ext, (int)space);
                     EditorUtility.SetDirty(mat);
                  }
               }
            }
            
            bool noiseOn = false;
            LayerBlendMode blendMode = (LayerBlendMode)(int)mat.GetFloat("_LayerBlendMode" + ext);

            if (mat.IsKeywordEnabled("_LAYERNOISE" + def))
               noiseOn = true;


            EditorGUI.BeginChangeCheck();
            blendMode = (LayerBlendMode)EditorGUILayout.EnumPopup("Layer Blend Mode", blendMode);

            if (blendMode == LayerBlendMode.HeightBlended)
            {
               materialEditor.ShaderProperty(FindProperty("_LayerStrength" + ext, props), "Layer Height");
               materialEditor.ShaderProperty(FindProperty("_LayerHeightContrast" + ext, props), "Blend Contrast");
            }
            else
            {
               materialEditor.ShaderProperty(FindProperty("_LayerStrength" + ext, props), "Layer Strength");
            }

            materialEditor.ShaderProperty(FindProperty("_LayerUseEffector" + ext, props), "Use Effector");
            if (mat.IsKeywordEnabled("_LAYEREFFECTOR" + def))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_LayerEffectorInvert" + ext, props), "Invert");
               EditorGUI.indentLevel--;
            }
            noiseOn = EditorGUILayout.Toggle("Layer Noise", noiseOn);

            // check for autoupgrade from 2020->2021
            bool blendKeywordMissing = (!mat.IsKeywordEnabled("_LAYERBLENDMULT2X" + def) &&
                                !mat.IsKeywordEnabled("_LAYERBLENDALPHA" + def) &&
                                !mat.IsKeywordEnabled("_LAYERBLENDHEIGHT" + def));

            if (EditorGUI.EndChangeCheck() || blendKeywordMissing)
            {
               mat.DisableKeyword("_LAYERHEIGHTBLEND" + def);
               mat.DisableKeyword("_LAYERALPHABLEND" + def);
               mat.DisableKeyword("_LAYERNOISE" + def);

               mat.SetFloat("_LayerBlendMode" + ext, (int)blendMode);

               if (noiseOn == true)
               {
                  mat.EnableKeyword("_LAYERNOISE" + def);
               }

               mat.DisableKeyword("_LAYERBLENDMULT2X" + def);
               mat.DisableKeyword("_LAYERBLENDALPHA" + def);
               mat.DisableKeyword("_LAYERBLENDHEIGHT" + def);
               if (blendMode == LayerBlendMode.Multiply2X)
               {
                  mat.EnableKeyword("_LAYERBLENDMULT2X" + def);
               }
               else if (blendMode == LayerBlendMode.AlphaBlended)
               {
                  mat.EnableKeyword("_LAYERBLENDALPHA" + def);
               }
               else
               {
                  mat.EnableKeyword("_LAYERBLENDHEIGHT" + def);
               }
               EditorUtility.SetDirty(mat);
            }


            if (noiseOn)
            {
               EditorGUI.indentLevel++;
               EditorGUI.BeginChangeCheck();
               DoNoiseSpace("_LAYER", def);
               DoNoiseQuality("_LAYER", ext, def, "_Layer", materialEditor, props);
               materialEditor.ShaderProperty(FindProperty("_LayerNoiseFrequency" + ext, props), "Noise Frequency");
               materialEditor.ShaderProperty(FindProperty("_LayerNoiseAmplitude" + ext, props), "Noise Amplitude");
               materialEditor.ShaderProperty(FindProperty("_LayerNoiseCenter" + ext, props), "Noise Center");
               materialEditor.ShaderProperty(FindProperty("_LayerNoiseOffset" + ext, props), "Noise Offset");
               EditorGUILayout.BeginHorizontal();
               materialEditor.ShaderProperty(FindProperty("_LayerBlendTint" + ext, props), "Layer Blend Tint");
               materialEditor.ShaderProperty(FindProperty("_LayerBlendContrast" + ext, props), "");
               EditorGUILayout.EndHorizontal();
               if (EditorGUI.EndChangeCheck())
               {
                  EditorUtility.SetDirty(mat);
               }

               EditorGUI.indentLevel--;
            }

            bool angleFilter = mat.IsKeywordEnabled("_LAYERANGLEFILTER" + def);
            bool naf = EditorGUILayout.Toggle("Angle/Height Filter", angleFilter);
            if (naf != angleFilter)
            {
               mat.DisableKeyword("_LAYERANGLEFILTER" + def);
               if (naf)
               {
                  mat.EnableKeyword("_LAYERANGLEFILTER" + def);
               }
            }
            if (naf)
            {
               EditorGUI.indentLevel++;

               WorldLocalSpace spc = WorldLocalSpace.World;
               if (mat.IsKeywordEnabled("_LAYERANGLE_LOCAL"))
               {
                  spc = WorldLocalSpace.Local;
               }
               EditorGUI.BeginChangeCheck();
               spc = (WorldLocalSpace)EditorGUILayout.EnumPopup("Angle Space", spc);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.DisableKeyword("_LAYERANGLE_LOCAL");
                  if (spc == WorldLocalSpace.Local)
                  {
                     mat.EnableKeyword("_LAYERANGLE_LOCAL");
                  }
               }
               materialEditor.ShaderProperty(FindProperty("_LayerAngleMin" + ext, props), "Angle Minimum");
               Vector4 angleVec = FindProperty("_LayerAngleVector" + ext, props).vectorValue;
               EditorGUI.BeginChangeCheck();
               Vector3 newVec = EditorGUILayout.Vector3Field("Angle Vector", angleVec);
               EditorGUI.indentLevel++;
               bool abs = EditorGUILayout.Toggle("Mirror", angleVec.w > 0 ? true : false);
               EditorGUI.indentLevel--;
               if (EditorGUI.EndChangeCheck())
               { 
                  FindProperty("_LayerAngleVector", props).vectorValue = new Vector4(newVec.x, newVec.y, newVec.z, abs ? 1 : 0);
               }
               
               materialEditor.ShaderProperty(FindProperty("_LayerVertexNormalBlend" + ext, props), "Vertex -> Normal Filter");
               materialEditor.ShaderProperty(FindProperty("_LayerHeight" + ext, props), "Height Filter");
               materialEditor.ShaderProperty(FindProperty("_LayerInvertHeight" + ext, props), "Layer on");
               materialEditor.ShaderProperty(FindProperty("_LayerFalloff" + ext, props), "Contrast");

               EditorGUI.indentLevel--;

            }
            Vector4 dist = mat.GetVector("_LayerWeightOverDistance" + ext);
            if (DrawRollup("Distance Fade", true, true))
            {
               EditorGUI.indentLevel++;
               EditorGUI.BeginChangeCheck();
               dist.x = EditorGUILayout.FloatField("Start Fade", dist.x);
               dist.y = EditorGUILayout.Slider("Start Weight", dist.y, 0, 1);
               dist.z = EditorGUILayout.FloatField("Fade Range", dist.z);
               dist.w = EditorGUILayout.Slider("End Weight", dist.w, 0, 1);
               EditorGUI.indentLevel--;
            
               if (EditorGUI.EndChangeCheck())
               {
                  FindProperty("_LayerWeightOverDistance" + ext, props).vectorValue = dist;
               }
            }

            EditorGUI.BeginChangeCheck();
            var detailAlbedo = FindProperty("_LayerAlbedoMap" + ext, props);
            var detailNormal = FindProperty("_LayerNormalMap" + ext, props);
            var detailMask = FindProperty("_LayerMaskMap" + ext, props);
            var detailEmission = FindProperty("_LayerEmissionMap" + ext, props);
            var detailEmissionColor = FindProperty("_LayerEmissionColor" + ext, props);
            DoTextureUI(materialEditor, props, layerUVMode, "Albedo", "_LayerAlbedoMap", true, "_LayerTextureRotation", "_LAYERTEXTUREROTATION" + def, "_LayerTint", ext);

            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(FindProperty("_LayerAlbedoBrightness" + ext, props), "Brightness");
            materialEditor.RangeProperty(FindProperty("_LayerAlbedoContrast" + ext, props), "Contrast");
            materialEditor.RangeProperty(FindProperty("_LayerAlbedoHue" + ext, props), "Hue");
            materialEditor.RangeProperty(FindProperty("_LayerAlbedoSaturation" + ext, props), "Saturation");

            bool enableHS = FindProperty("_LayerAlbedoHue" + ext, props).floatValue != 0 || FindProperty("_LayerAlbedoSaturation" + ext, props).floatValue != 1;
            if (enableHS && !mat.IsKeywordEnabled("_LAYERHSVSHIFT" + def))
            {
                mat.EnableKeyword("_LAYERHSVSHIFT" + def);
                EditorUtility.SetDirty(mat);
            }
            else if (!enableHS && mat.IsKeywordEnabled("_LAYERHSVSHIFT" + def))
            {
                mat.DisableKeyword("_LAYERHSVSHIFT" + def);
                EditorUtility.SetDirty(mat);
            }

                EditorGUI.indentLevel--;
            if (!IsUnlit() || IsMatCap())
            {
               if (!mat.IsKeywordEnabled("_AUTONORMAL"))
               {
                  if (packing != Packing.Unity)
                  {
                     WarnLinear(detailNormal.textureValue);
                     DoTextureUI(materialEditor, props, layerUVMode, "Layer Packed", "_LayerNormalMap", false, "", "", null, ext);
                     materialEditor.RangeProperty(FindProperty("_LayerNormalStrength" + ext, props), "Normal Strength");
                  }
                  else
                  {
                     WarnNormal(detailNormal.textureValue);
                     DoTextureUI(materialEditor, props, layerUVMode, "Normal", "_LayerNormalMap", false, "", "", "_LayerNormalStrength", ext);
                  }
               }
               if (packing == Packing.Unity)
               {
                  WarnLinear(detailMask.textureValue);
                  DoTextureUI(materialEditor, props, layerUVMode, "Layer Mask", "_LayerMaskMap", false, "", "", "", ext);
               }
               GUILayout.BeginHorizontal();

               var workflow = GetWorkflow();
               if (workflow == Workflow.Specular)
               {
                  DoTextureUI(materialEditor, props, layerUVMode, "Layer Specular Map", "_LayerSpecularMap", false, "", "", "_LayerSpecularTint", ext);
               }

               bool emissionEnabled = mat.IsKeywordEnabled("_LAYEREMISSION" + def);
               emissionEnabled = EditorGUILayout.Toggle(emissionEnabled, GUILayout.Width(24));
               if (layerUVMode == UVMode.TriplanarTexturing)
               {
                  materialEditor.TexturePropertyWithHDRColor(new GUIContent("Layer Emission Side"), detailEmission, detailEmissionColor, false);
                  GUILayout.EndHorizontal();
                  EditorGUI.indentLevel++;
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer Emission Top"), FindProperty("_LayerEmissionMap_P1", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer Emission Front"), FindProperty("_LayerEmissionMap_P2" + ext, props));
                  EditorGUI.indentLevel--;
               }
               else
               {
                  materialEditor.TexturePropertyWithHDRColor(new GUIContent("Layer Emission"), detailEmission, detailEmissionColor, false);
                  GUILayout.EndHorizontal();
               }
               EditorGUI.indentLevel++;
               materialEditor.FloatProperty(FindProperty("_LayerEmissionMultiplier" + ext, props), "Emission Multiplier");
               EditorGUI.indentLevel--;
               bool cheapSSSEnabled = mat.IsKeywordEnabled("_CHEAPSSS");
               if (cheapSSSEnabled)
               {
                  var sssTexture = FindProperty("_LayerCheapSSSTexture" + ext, props);
                  bool hadTexture = sssTexture.textureValue != null;
                  if (layerUVMode == UVMode.TriplanarTexturing)
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Side"), sssTexture);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Top"), FindProperty("_CheapSSSTexture_P1", props));
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Front"), FindProperty("_CheapSSSTexture_P2", props));
                  }
                  else
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("SSS Tint/Thickness"), sssTexture);
                  }

                  if (sssTexture.textureValue == null)
                  {
                     materialEditor.ColorProperty(FindProperty("_LayerCheapSSSTint" + ext, props), "Tint");
                     materialEditor.FloatProperty(FindProperty("_LayerCheapSSSThickness" + ext, props), "Thickness");
                  }

               }

#if USING_HDRP || USING_URP
               bool coatEnabled = mat.IsKeywordEnabled("_CLEARCOAT");
               if (coatEnabled)
               {
                  EditorGUI.BeginChangeCheck();
                  var coatProp = FindProperty("_LayerClearCoatMap" + ext, props);
                  EditorGUI.indentLevel++;
                  if (layerUVMode == UVMode.TriplanarTexturing)
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Side"), coatProp);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Top"), FindProperty("_LayerClearCoatMap_P1" + ext, props));
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Front"), FindProperty("_LayerClearCoatMap_P2" + ext, props));
                     if (coatProp.textureValue == null)
                     {
                        materialEditor.ShaderProperty(FindProperty("_LayerClearCoatMask" + ext, props), new GUIContent("Clear Coat Mask"));
                        materialEditor.ShaderProperty(FindProperty("_LayerClearCoatSmoothness" + ext, props), new GUIContent("Clear Coat Smoothness"));
                     }
                  }
                  else
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask"), coatProp);
                     if (coatProp.textureValue == null)
                     {
                        materialEditor.ShaderProperty(FindProperty("_LayerClearCoatMask" + ext, props), new GUIContent("Clear Coat Mask"));
                        materialEditor.ShaderProperty(FindProperty("_LayerClearCoatSmoothness" + ext, props), new GUIContent("Clear Coat Smoothness"));
                     }
                  }
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.DisableKeyword("_LAYERCLEARCOATMAP" + def);
                     if (coatProp.textureValue != null)
                     {
                        mat.EnableKeyword("_LAYERCLEARCOATMAP" + def);
                     }
                     EditorUtility.SetDirty(mat);
                  }

                  EditorGUI.indentLevel--;
               }

#endif

               if (EditorGUI.EndChangeCheck())
               {
                  mat.DisableKeyword("_LAYERALBEDO" + def);
                  mat.DisableKeyword("_LAYERNORMAL" + def);
                  mat.DisableKeyword("_LAYERMASK" + def);
                  mat.DisableKeyword("_LAYEREMISSION" + def);

                  if (detailAlbedo.textureValue != null)
                  {
                     mat.EnableKeyword("_LAYERALBEDO" + def);
                  }
                  if (detailNormal.textureValue != null)
                  {
                     mat.EnableKeyword("_LAYERNORMAL" + def);
                  }
                  if (detailMask.textureValue != null)
                  {
                     mat.EnableKeyword("_LAYERMASK" + def);
                  }
                  if (emissionEnabled)
                  {
                     mat.EnableKeyword("_LAYEREMISSION" + def);
                  }
               }

               if (detailMask.textureValue == null && packing == Packing.Unity)
               {
                  materialEditor.ShaderProperty(FindProperty("_LayerSmoothness" + ext, props), new GUIContent("Layer Smoothness"));
                  materialEditor.ShaderProperty(FindProperty("_LayerMetallic" + ext, props), new GUIContent("Layer Metallic"));
               }
               else if (packing == Packing.Fastest)
               {
                  materialEditor.ShaderProperty(FindProperty("_LayerMetallic" + ext, props), new GUIContent("Layer Metallic"));
                  RemapRange("Smoothness Remap", "_LayerSmoothnessRemap" + ext);
                  RemapRange("Occlusion Remap", "_LayerAORemap" + ext);
               }
               else if (packing == Packing.FastMetal)
               {
                  RemapRange("Smoothness Remap", "_LayerSmoothnessRemap" + ext);
                  RemapRange("Metallic Remap", "_LayerMetallicRemap" + ext);
               }
               else if (packing == Packing.Unity && detailMask.textureValue != null)
               {
                  RemapRange("Smoothness Remap", "_LayerSmoothnessRemap" + ext);
                  RemapRange("Metallic Remap", "_LayerMetallicRemap" + ext);
                  RemapRange("Occlusion Remap", "_LayerAORemap" + ext);
               }
            }
            if (mat.HasProperty("_IsAlpha"))
            {
               RemapRange("Alpha Remap", "_LayerHeightRemap" + ext);
            }
            else
            {
               RemapRange("Height Remap", "_LayerHeightRemap" + ext);
            }

            DoStochastic(mat, materialEditor, props, "_LAYERSTOCHASTIC" + def, "_LayerStochasticContrast" + ext, "_LayerStochasticScale" + ext);


            if (blendMode != LayerBlendMode.HeightBlended)
            {
               if (detailAlbedo.textureValue != null)
               {
                  materialEditor.ShaderProperty(FindProperty("_LayerAlbedoStrength" + ext, props), new GUIContent("Layer Albedo Strength"));
               }
               if (detailMask.textureValue != null || packing != Packing.Unity)
               {
                  materialEditor.ShaderProperty(FindProperty("_LayerSmoothnessStrength" + ext, props), new GUIContent("Layer Smoothness Strength"));
               }
            }
            if (!IsUnlit())
            {
               materialEditor.RangeProperty(FindProperty("_LayerMicroShadowStrength" + ext, props), "Micro Shadow Strength");
            }
            if (mat.shader != null && mat.HasProperty("_TessellationMaxSubdiv"))
            {
               materialEditor.ShaderProperty(FindProperty("_LayerTessStrength" + ext, props), "Displacement Strength");
            }
            if (!IsUnlit() || IsMatCap())
            {
               DoFuzzyShadingUI(materialEditor, props, "Layer", ext, def);
            }
            DoFresnel(materialEditor, props, "_LAYERFRESNEL" + def, "_LAYERFRESNELNORMAL" + def, "_LayerFresnelColor" + ext, "_LayerFresnelBSP" + ext);
            DoSparkle(materialEditor, props, "_LAYERSPARKLES" + def, "_LayerSparkleNoise" + ext, "_LayerSparkleTCI" + ext);

            EditorGUI.indentLevel--;
         }

      }

      enum RainMode
      {
         Off,
         Local,
         Global
      }

      public void DoRainDrops(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         RainMode mode = (RainMode)mat.GetInt("_RainMode");

         var nm = (RainMode)EditorGUILayout.EnumPopup("Rain Drops", mode);

         if (nm != mode)
         {
            mode = nm;
            mat.DisableKeyword("_RAINDROPS");
            mat.SetFloat("_RainMode", 0);
            if (mode == RainMode.Local)
            {
               mat.EnableKeyword("_RAINDROPS");
               mat.SetFloat("_RainMode", 1);
            }
            else if (mode == RainMode.Global)
            {
               mat.EnableKeyword("_RAINDROPS");
               mat.SetFloat("_RainMode", 2);
            }
            EditorUtility.SetDirty(mat);
         }
         if (mode != RainMode.Off)
         {
            EditorGUI.indentLevel++;
            var prop = FindProperty("_RainDropTexture", props);
            if (prop.textureValue == null)
            {
               prop.textureValue = FindDefaultTexture("betterlit_default_raindrops");
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Rain Texture"), FindProperty("_RainDropTexture", props));
            Vector4 data = mat.GetVector("_RainIntensityScale");
            EditorGUI.BeginChangeCheck();
            if (mode != RainMode.Global)
            {
               data.x = EditorGUILayout.Slider("Intensity", data.x, 0, 2);
            }
            data.y = EditorGUILayout.FloatField("UV Scale", data.y);
            data.z = EditorGUILayout.Slider("Effect Wet Areas", data.z, 0, 1);
            float oldW = data.w;
            data.w = EditorGUILayout.FloatField("Distance Falloff", data.w);
            // revision
            if (oldW == data.w && data.w == 0)
            {
               data.w = 200;
               mat.SetVector("_RainIntensityScale", data);
               EditorUtility.SetDirty(mat);
            }
            if (EditorGUI.EndChangeCheck())
            {
               mat.SetVector("_RainIntensityScale", data);
               EditorUtility.SetDirty(mat);
            }
            EditorGUI.indentLevel--;
         }

      }

      static GUIContent CPuddleUseEffector = new GUIContent("Use Effector", "Have effectors add or subtrack puddles");
      public void DoPuddles(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         if (!mat.HasProperty("_PuddleMode"))
            return;
         if (DrawRollupKeywordToggle(mat, "Puddles", "_PUDDLES"))
         {
            LocalGlobalMode mode = (LocalGlobalMode)mat.GetInt("_PuddleMode");

            EditorGUI.BeginChangeCheck();
            mode = (LocalGlobalMode)EditorGUILayout.EnumPopup("Puddle Mode", mode);

            if (EditorGUI.EndChangeCheck())
            {
               mat.SetFloat("_PuddleMode", (int)mode);
            }
            EditorGUI.indentLevel++;
            if (mode == LocalGlobalMode.Local)
            {
               materialEditor.ShaderProperty(FindProperty("_PuddleAmount", props), "Puddle Amount");
            }

            materialEditor.ShaderProperty(FindProperty("_PuddleUseEffector", props), CPuddleUseEffector);
            if (mat.IsKeywordEnabled("_PUDDLEEFFECTOR"))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_PuddleEffectorInvert", props), CWireEffectorInvert);
               EditorGUI.indentLevel--;
            }

            materialEditor.ShaderProperty(FindProperty("_PuddleColor", props), "Puddle Color");
            materialEditor.ShaderProperty(FindProperty("_PuddleAngleMin", props), "Puddle Angle Filter");
            materialEditor.ShaderProperty(FindProperty("_PuddleFalloff", props), "Puddle Contrast");
            

            bool noiseOn = mat.IsKeywordEnabled("_PUDDLENOISE");


            EditorGUI.BeginChangeCheck();
            noiseOn = EditorGUILayout.Toggle("Puddle Noise", noiseOn);
            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword("_PUDDLENOISE");
               if (noiseOn)
               {
                  mat.EnableKeyword("_PUDDLENOISE");
               }
               EditorUtility.SetDirty(mat);
            }

            if (noiseOn)
            {
               EditorGUI.indentLevel++;
               EditorGUI.BeginChangeCheck();

               DoNoiseSpace("_PUDDLE", "");
               DoNoiseQuality("_PUDDLE", "", "", "_Puddle", materialEditor, props);
               materialEditor.ShaderProperty(FindProperty("_PuddleNoiseFrequency", props), "Noise Frequency");
               materialEditor.ShaderProperty(FindProperty("_PuddleNoiseAmplitude", props), "Noise Amplitude");
               materialEditor.ShaderProperty(FindProperty("_PuddleNoiseCenter", props), "Noise Center");
               materialEditor.ShaderProperty(FindProperty("_PuddleNoiseOffset", props), "Noise Offset");

               if (EditorGUI.EndChangeCheck())
               {
                  EditorUtility.SetDirty(mat);
               }
               EditorGUI.indentLevel--;
            }

            DoRainDrops(materialEditor, props);
            EditorGUI.indentLevel--;
         }
         

      }

      void RemapRange(string label, string prop, float min = 0, float max = 1)
      {
         Vector4 value = mat.GetVector(prop);
         float low = value.x;
         float high = value.y;
         EditorGUILayout.MinMaxSlider(label, ref low, ref high, min, max);
         if (low != value.x || high != value.y)
         {
            mat.SetVector(prop, new Vector2(low, high));
            EditorUtility.SetDirty(mat);
         }
      }

      public void OnLitShaderSettings(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!IsUnlit())
         {
            DoWorkflow();
            DoLightingModel();
            DoPacking(mat);
         }
         else
         {
            DoLightingModel();
            if (IsMatCap())
               DoPacking(mat);
         }

      }

      void DoFuzzyShadingUI(MaterialEditor materialEditor, MaterialProperty[] props, string prefix, string postfix, string def = "")
      {
         if (IsUnlit())
            return;
         string k = "_" + prefix.ToUpper() + "FUZZYSHADING" + def;
         bool on = mat.IsKeywordEnabled(k);
         bool change = EditorGUILayout.Toggle("Fuzzy Shading", on);
         if (on != change)
         {
            if (change)
            {
               mat.EnableKeyword(k);
            }
            else
            {
               mat.DisableKeyword(k);
            }
            EditorUtility.SetDirty(mat);
            on = change;
         }
         if (on)
         {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(FindProperty("_" + prefix + "FuzzyShadingColor" + postfix, props), "Color");
            Vector4 param = mat.GetVector("_" + prefix + "FuzzyShadingParams" + postfix);
            EditorGUI.BeginChangeCheck();
            param.x = EditorGUILayout.Slider("Core Multiplier", param.x, 0.1f, 3);
            param.y = EditorGUILayout.Slider("Edge Multiplier", param.y, 0.1f, 3);
            param.z = EditorGUILayout.Slider("Power", param.z, 0.1f, 3);
            if (EditorGUI.EndChangeCheck())
            {
               mat.SetVector("_" + prefix + "FuzzyShadingParams" + postfix, param);
            }
            EditorGUI.indentLevel--;
         }

      }

      void DoTextureRotation3(MaterialEditor materialEditor, MaterialProperty[] props, string propName, string keyword, int index, string name = "Rotation")
      {
         if (string.IsNullOrEmpty(propName) || string.IsNullOrEmpty(keyword))
            return;
         EditorGUILayout.BeginHorizontal();
         EditorGUI.BeginChangeCheck();
         Vector3 v = FindProperty(propName, props).vectorValue;
         if (index < 0)
         {
            v.x = EditorGUILayout.Slider(new GUIContent(name), v.x, 0, 1.0f);
            v.y = v.x;
            v.z = v.x;
         }
         else
         {
            v[index] = EditorGUILayout.Slider(new GUIContent(name), v[index], 0, 1.0f);
         }
         if (EditorGUI.EndChangeCheck())
         {
            mat.SetVector(propName, v);
            
            if (v.x == 0 && v.y == 0 && v.z == 0)
            {
               mat.DisableKeyword(keyword);
            }
            else
            {
               mat.EnableKeyword(keyword);
            }
            EditorUtility.SetDirty(mat);
         }
         EditorGUILayout.EndHorizontal();
      }

      void DoTextureUI(MaterialEditor materialEditor, MaterialProperty[] props, UVMode uvMode, string name, string propName,
         bool scaleoffset, string rotProp = "", string rotKeyword = "", string propName2 = null, string ext = "")
      {
         if (uvMode == UVMode.UV)
         {
            if (!string.IsNullOrEmpty(propName2))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name), FindProperty(propName + ext, props), FindProperty(propName2 + ext, props));
            }
            else
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name), FindProperty(propName + ext, props));
            }
            if (scaleoffset)
            {
               materialEditor.TextureScaleOffsetProperty(FindProperty(propName + ext, props));
               DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 0);
            }
         }
         else if (uvMode == UVMode.TriplanarUV)
         {
            if (!string.IsNullOrEmpty(propName2))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name), FindProperty(propName + ext, props), FindProperty(propName2 + ext, props));
            }
            else
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name), FindProperty(propName + ext, props));
            }
            if (scaleoffset)
            {
               materialEditor.TextureScaleOffsetProperty(FindProperty(propName + ext, props));
               DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, -1);
            }
         }
         else
         {
            if (!string.IsNullOrEmpty(propName2))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Side"), FindProperty(propName + ext, props), FindProperty(propName2 + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 0);
               }
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Top"), FindProperty(propName + "_P1" + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + "_P1" + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 1);
               }
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Front"), FindProperty(propName + "_P2" + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + "_P2" + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 2);
               }
            }
            else
            {
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Side"), FindProperty(propName + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 0);
               }
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Top"), FindProperty(propName + "_P1" + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + "_P1" + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 1);
               }
               materialEditor.TexturePropertySingleLine(new GUIContent(name + " Front"), FindProperty(propName + "_P2" + ext, props));
               if (scaleoffset)
               {
                  materialEditor.TextureScaleOffsetProperty(FindProperty(propName + "_P2" + ext, props));
                  DoTextureRotation3(materialEditor, props, rotProp + ext, rotKeyword, 2);
               }
            }
         }
      }

      enum SSSType
      {
         None,
         Approximate,
      }

      enum DitherAlphaType
      {
         None,
         Constant,
         Fade
      }

      public Shader changeShader = null;
      public void OnLitGUI(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (DrawRollup("Main Layer"))
         {
            UVMode uvMode = DoUVMode(mat, "_TRIPLANAR", "_TRIPLANARPROJECTION", "UV Mode");
            if (uvMode == UVMode.TriplanarUV || uvMode == UVMode.TriplanarTexturing)
            {
               DoTriplanarSpace(mat, materialEditor, props, "_TriplanarSpace", "_TriplanarContrast",
                  "_TRIPLANARBARYBLEND", "_TriplanarBaryBlend", "_TRIPLANARFLATBLEND");
            }
            if (uvMode == UVMode.UV)
            {
               var uvSpace = DoUVSource(mat, "_UVSource");
               
               if (uvSpace != UVSource.UV0 && uvSpace != UVSource.UV1)
               {
                  EditorGUI.BeginChangeCheck();
                  TriplanarSpace space = TriplanarSpace.World;
                  if (mat.GetFloat("_TriplanarSpace") > 0.5)
                     space = TriplanarSpace.Local;
                  space = (TriplanarSpace)EditorGUILayout.EnumPopup("Projection Space", space);
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetFloat("_TriplanarSpace", (int)space);
                     EditorUtility.SetDirty(mat);
                  }
               }
            }
            
            if (mat.GetFloat("_IsConverted") < 1)
            {
               if (mat.GetTexture("_AlbedoMap") == null)
               {
                  var tex = (mat.GetTexture("_MainTex"));
                  if (tex == null)
                     tex = mat.GetTexture("_BaseColor");
                  if (tex != null)
                  {
                     mat.SetTexture("_AlbedoMap", tex);
                  }

               }

               if (mat.GetTexture("_NormalMap") == null)
               {
                  var tex = (mat.GetTexture("_BumpMap"));
                  if (tex != null)
                     mat.SetTexture("_NormalMap", tex);
               }
               mat.SetFloat("_IsConverted", 1);
               if (mat.GetTexture("_NormalMap"))
               {
                  mat.EnableKeyword("_NORMALMAP");
               }
               if (mat.GetTexture("_MaskMap"))
               {
                  mat.EnableKeyword("_MASKMAP");
               }
            }

            DoTextureUI(materialEditor, props, uvMode, "Albedo", "_AlbedoMap", true, "_TextureRotation", "_TEXTUREROTATION", "_Tint");
            
            
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(FindProperty("_AlbedoBrightness", props), "Brightness");
            materialEditor.RangeProperty(FindProperty("_AlbedoContrast", props), "Contrast");
            materialEditor.RangeProperty(FindProperty("_AlbedoHue", props), "Hue");
            materialEditor.RangeProperty(FindProperty("_AlbedoSaturation", props), "Saturation");
            bool enableBC = FindProperty("_AlbedoBrightness", props).floatValue != 0 || FindProperty("_AlbedoContrast", props).floatValue != 1;
            if (enableBC && !mat.IsKeywordEnabled("_USEBRIGHTNESSCONTRAST"))
            {
               mat.EnableKeyword("_USEBRIGHTNESSCONTRAST");
               EditorUtility.SetDirty(mat);
            }
            else if (!enableBC && mat.IsKeywordEnabled("_USEBRIGHTNESSCONTRAST"))
            {
               mat.DisableKeyword("_USEBRIGHTNESSCONTRAST");
               EditorUtility.SetDirty(mat);
            }

            bool enableHS = FindProperty("_AlbedoHue", props).floatValue != 0 || FindProperty("_AlbedoSaturation", props).floatValue != 1;
            if (enableHS && !mat.IsKeywordEnabled("_USEHSVSHIFT"))
            {
                mat.EnableKeyword("_USEHSVSHIFT");
                EditorUtility.SetDirty(mat);
            }
            else if (!enableHS && mat.IsKeywordEnabled("_USEHSVSHIFT"))
            {
                mat.DisableKeyword("_USEHSVSHIFT");
                EditorUtility.SetDirty(mat);
            }


            EditorGUI.indentLevel--;
            DoParallax(mat, materialEditor, props);
            EditorGUI.BeginChangeCheck();
            if (GetAlphaState() == AlphaMode.Opaque)
            {
                DitherAlphaType ditherAlphaType = DitherAlphaType.None;
                if (mat.IsKeywordEnabled("_DITHERFADE"))
                    ditherAlphaType = DitherAlphaType.Fade;
                else if (mat.IsKeywordEnabled("_DITHERCONSTANT"))
                    ditherAlphaType = DitherAlphaType.Constant;

                bool useAlphaChannel = mat.IsKeywordEnabled("_DITHERMULTALPHA");

                ditherAlphaType = (DitherAlphaType)EditorGUILayout.EnumPopup("Dither", ditherAlphaType);
              
                if (ditherAlphaType != DitherAlphaType.None)
                {
                    EditorGUI.indentLevel++;
                    if (ditherAlphaType == DitherAlphaType.Constant)
                    {
                        materialEditor.RangeProperty(FindProperty("_DitherAlpha", props), "Alpha");
                    }
                    else if (ditherAlphaType == DitherAlphaType.Fade)
                    {
                        materialEditor.VectorProperty(FindProperty("_DitherAlphaDistance", props), "Alpha Fade");
                    }
                    useAlphaChannel = EditorGUILayout.Toggle("Use Alpha Channel", useAlphaChannel);
                    EditorGUI.indentLevel--;

                }
                if (EditorGUI.EndChangeCheck())
                {
                    mat.DisableKeyword("_DITHERFADE");
                    mat.DisableKeyword("_DITHERCONSTANT");
                    mat.DisableKeyword("_DITHERMULTALPHA");
                    if (ditherAlphaType == DitherAlphaType.Constant)
                    {
                        mat.EnableKeyword("_DITHERCONSTANT");
                    }
                    else if (ditherAlphaType == DitherAlphaType.Fade)
                    {
                        mat.EnableKeyword("_DITHERFADE");
                    }
                    if (useAlphaChannel)
                    {
                        mat.EnableKeyword("_DITHERMULTALPHA");
                    }
                    EditorUtility.SetDirty(mat);
                }
            }
            var normalMode = GetNormalMode();
            var packing = GetPacking();
            if (!IsUnlit() || IsMatCap())
            {
               if (normalMode != NormalMode.FromHeight)
               {
                  EditorGUI.BeginChangeCheck();
                  if (packing == Packing.Unity)
                  {
                     WarnNormal(FindProperty("_NormalMap", props).textureValue);
                     DoTextureUI(materialEditor, props, uvMode, "Normal", "_NormalMap", false, "", "",  "_NormalStrength");
                     
                  }
                  else
                  {
                     WarnLinear(FindProperty("_NormalMap", props).textureValue);
                     DoTextureUI(materialEditor, props, uvMode, "Packed", "_NormalMap", false, "", "", "_NormalStrength");
                  }
                  if (EditorGUI.EndChangeCheck())
                  {
                     if (FindProperty("_NormalMap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_NORMALMAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_NORMALMAP");
                     }
                     EditorUtility.SetDirty(mat);
                  }
               }
               var maskProp = FindProperty("_MaskMap", props);
               if (packing == Packing.Unity)
               {
                  EditorGUI.BeginChangeCheck();
                  WarnLinear(maskProp.textureValue);
                  DoTextureUI(materialEditor, props, uvMode, "Mask Map", "_MaskMap", false);

                  if (EditorGUI.EndChangeCheck())
                  {
                     if (maskProp.textureValue != null)
                     {
                        mat.EnableKeyword("_MASKMAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_MASKMAP");
                     }
                  }
               }
               else
               {
                  if (mat.IsKeywordEnabled("_MASKMAP"))
                  {
                     mat.DisableKeyword("_MASKMAP");
                     EditorUtility.SetDirty(mat);
                  }
               }
               var workflow = GetWorkflow();
               if (workflow == Workflow.Specular)
               {
                  DoTextureUI(materialEditor, props, uvMode, "Specular Map", "_SpecularMap", false, "", "", "_SpecularTint");
               }

               SSSType ssstype = SSSType.None;
               if (mat.IsKeywordEnabled("_CHEAPSSS"))
               {
                  ssstype = SSSType.Approximate;
               }
               SSSType newSSSType = (SSSType)EditorGUILayout.EnumPopup("SubSurface Scattering Tyoe", ssstype);


               if (newSSSType != ssstype)
               {
                  mat.DisableKeyword("_CHEAPSSS");
                  if (newSSSType == SSSType.Approximate)
                  {
                     mat.EnableKeyword("_CHEAPSSS");
                  }
                  EditorUtility.SetDirty(mat);
               }
               if (newSSSType == SSSType.Approximate)
               {
                  EditorGUI.indentLevel++;
                  var sssTexture = FindProperty("_CheapSSSTexture", props);
                  bool hadTexture = sssTexture.textureValue != null;
                  if (uvMode == UVMode.TriplanarTexturing)
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Side"), sssTexture);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Top"), FindProperty("_CheapSSSTexture_P1", props));
                     materialEditor.TexturePropertySingleLine(new GUIContent("Tint/Thickness Front"), FindProperty("_CheapSSSTexture_P2", props));
                  }
                  else
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("SSS Tint/Thickness"), sssTexture);
                  }
                  if (hadTexture != (sssTexture.textureValue != null))
                  {
                     if (sssTexture.textureValue == null)
                     {
                        mat.DisableKeyword("_CHEAPSSSTEXTURE");
                     }
                     else
                     {
                        mat.EnableKeyword("_CHEAPSSSTEXTURE");
                     }
                  }
                  if (sssTexture.textureValue == null)
                  {
                     materialEditor.ColorProperty(FindProperty("_CheapSSSTint", props), "Tint");
                     materialEditor.FloatProperty(FindProperty("_CheapSSSThickness", props), "Thickness");
                  }
                  materialEditor.FloatProperty(FindProperty("_CheapSSSPower", props), "Power");
                  materialEditor.FloatProperty(FindProperty("_CheapSSSDistortion", props), "Distortion");
                  materialEditor.FloatProperty(FindProperty("_CheapSSSScale", props), "Scale");
                  EditorGUI.indentLevel--;
               }

               var emissionProp = FindProperty("_EmissionMap", props);
               var emissionColor = FindProperty("_EmissionColor", props);
               EditorGUI.BeginChangeCheck();

               GUILayout.BeginHorizontal();
               bool emissionEnabled = mat.IsKeywordEnabled("_EMISSION");
               emissionEnabled = EditorGUILayout.Toggle(emissionEnabled, GUILayout.Width(18));
               
               if (uvMode == UVMode.TriplanarTexturing)
               {
                  materialEditor.TexturePropertyWithHDRColor(new GUIContent("Emission Side"), emissionProp, emissionColor, false);
                  GUILayout.EndHorizontal();
                  EditorGUI.indentLevel++;
                  materialEditor.TexturePropertySingleLine(new GUIContent("Emission Top"), FindProperty("_EmissionMap_P1", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Emission Front"), FindProperty("_EmissionMap_P2", props));
                  EditorGUI.indentLevel--;
               }
               else
               {
                  materialEditor.TexturePropertyWithHDRColor(new GUIContent("Emission"), emissionProp, emissionColor, false);
                  GUILayout.EndHorizontal();
               }
               EditorGUI.indentLevel++;
               materialEditor.FloatProperty(FindProperty("_EmissionMultiplier", props), "Emission Multiplier");
               materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
               EditorGUI.indentLevel--;

#if USING_HDRP || USING_URP
               bool coatEnabled = mat.IsKeywordEnabled("_CLEARCOAT");
               bool newCoatEnabled = EditorGUILayout.Toggle("Enable Clear Coat", coatEnabled);
               if (coatEnabled != newCoatEnabled)
               {
                  mat.DisableKeyword("_CLEARCOAT");
                  if (newCoatEnabled)
                  {
                     mat.EnableKeyword("_CLEARCOAT");
                  }
                  EditorUtility.SetDirty(mat);
               }
               if (newCoatEnabled)
               {
                  EditorGUI.BeginChangeCheck();
                  var coatProp = FindProperty("_ClearCoatMap", props);
                  EditorGUI.indentLevel++;
                  if (uvMode == UVMode.TriplanarTexturing)
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Side"), coatProp);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Top"), FindProperty("_ClearCoatMap_P1", props));
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask Front"), FindProperty("_ClearCoatMap_P2", props));
                     if (coatProp.textureValue == null)
                     {
                        materialEditor.ShaderProperty(FindProperty("_ClearCoatMask", props), new GUIContent("Clear Coat Mask"));
                        materialEditor.ShaderProperty(FindProperty("_ClearCoatSmoothness", props), new GUIContent("Clear Coat Smoothness"));
                     }
                  }
                  else
                  {
                     materialEditor.TexturePropertySingleLine(new GUIContent("Coat Mask"), coatProp);
                     if (coatProp.textureValue == null)
                     {
                        materialEditor.ShaderProperty(FindProperty("_ClearCoatMask", props), new GUIContent("Clear Coat Mask"));
                        materialEditor.ShaderProperty(FindProperty("_ClearCoatSmoothness", props), new GUIContent("Clear Coat Smoothness"));
                     }
                  }
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.DisableKeyword("_CLEARCOATMAP");
                     if (coatProp.textureValue != null)
                     {
                        mat.EnableKeyword("_CLEARCOATMAP");
                     }
                     EditorUtility.SetDirty(mat);
                  }

                  EditorGUI.indentLevel--;
               }
               
#endif


               if (EditorGUI.EndChangeCheck())
               {
                  if (emissionEnabled)
                  {
                     mat.EnableKeyword("_EMISSION");
                  }
                  else
                  {
                     mat.DisableKeyword("_EMISSION");
                  }
               }

               if (packing == Packing.Unity && maskProp.textureValue == null)
               {
                  materialEditor.ShaderProperty(FindProperty("_Smoothness", props), "Smoothness");
                  materialEditor.ShaderProperty(FindProperty("_Metallic", props), "Metallic");
               }
               else if (packing == Packing.Fastest)
               {
                  materialEditor.ShaderProperty(FindProperty("_Metallic", props), "Metallic");
                  RemapRange("Smoothness Remap", "_SmoothnessRemap");
                  RemapRange("Occlusion Remap", "_AORemap");
               }
               else if (packing == Packing.FastMetal)
               {
                  RemapRange("Smoothness Remap", "_SmoothnessRemap");
                  RemapRange("Metallic Remap", "_MetallicRemap");
               }
               if (packing == Packing.Unity && maskProp.textureValue != null)
               {
                  RemapRange("Smoothness Remap", "_SmoothnessRemap");
                  RemapRange("Metallic Remap", "_MetallicRemap");
                  RemapRange("Occlusion Remap", "_AORemap");
               }
            }

            EditorGUI.BeginChangeCheck();
            bool specAA = mat.IsKeywordEnabled("_GEOMSPECULARAA");
            specAA = EditorGUILayout.Toggle("Geometric Specular AA", specAA);
            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword("_GEOMSPECULARAA");
               if (specAA)
               {
                   mat.EnableKeyword("_GEOMSPECULARAA");
               }
            }

            if (mat.HasProperty("_IsAlpha"))
            {
               RemapRange("Alpha Remap", "_HeightRemap");
            }
            else
            {
               RemapRange("Height Remap", "_HeightRemap");
            }
            if (mat.HasProperty("_DisplacementStrength"))
            {
               var prop = FindProperty("_DisplacementStrength", props);
               if (prop != null)
               {
                  materialEditor.RangeProperty(prop, "Displacement Strength");
               }
            }

            DoStochastic(mat, materialEditor, props, "_STOCHASTIC", "_StochasticContrast", "_StochasticScale");
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(FindProperty("_AlphaThreshold", props), "Alpha Clip Threshold");
            if (EditorGUI.EndChangeCheck())
            {
               if (FindProperty("_AlphaThreshold", props).floatValue > 0)
               {
                  mat.EnableKeyword("_ALPHACUT");
                  mat.renderQueue = 2450;
                  mat.SetOverrideTag("RenderType", "TransparentCutout");
               }
               else
               {
                  mat.DisableKeyword("_ALPHACUT");
                  mat.renderQueue = -1;
                  mat.SetOverrideTag("RenderType", "");
               }
               // because Unity's lightmapper is hard coded to use _Cutoff
               FindProperty("_Cutoff", props).floatValue = FindProperty("_AlphaThreshold", props).floatValue;
            }
            EditorGUI.BeginChangeCheck();
            if (!IsUnlit())
            {
               materialEditor.RangeProperty(FindProperty("_MicroShadowStrength", props), "Micro Shadow Strength");
               if (EditorGUI.EndChangeCheck())
               {
                  if (FindProperty("_MicroShadowStrength", props).floatValue > 0)
                  {
                     mat.EnableKeyword("_MICROSHADOW");
                  }
                  else
                  {
                     mat.DisableKeyword("_MICROSHADOW");
                  }
               }
               DoFuzzyShadingUI(materialEditor, props, "", "");
            }

            DoFresnel(materialEditor, props, "_FRESNEL", "_FRESNELNORMAL", "_FresnelColor", "_FresnelBSP");
            DoSparkle(materialEditor, props, "_SPARKLES", "_SparkleNoise", "_SparkleTCI");


            bool detailRollup = (DrawRollupKeywordToggle(mat, "Detail Texture", "_DETAIL"));
            if (detailRollup)
            {
               EditorGUI.indentLevel++;
               UVMode detailUVMode = DoUVMode(mat, "_DETAILTRIPLANAR", "_DETAILTRIPLANARPROJECTION", "Detail UV Mode");
               if (detailUVMode == UVMode.UV)
               {
                  var uvSpace = DoUVSource(mat, "_DetailUVSource");

                  if (uvSpace != UVSource.UV0 && uvSpace != UVSource.UV1)
                  {
                     EditorGUI.BeginChangeCheck();
                     TriplanarSpace space = TriplanarSpace.World;
                     if (mat.GetFloat("_DetailTriplanarSpace") > 0.5)
                        space = TriplanarSpace.Local;
                     space = (TriplanarSpace)EditorGUILayout.EnumPopup("Projection Space", space);
                     if (EditorGUI.EndChangeCheck())
                     {
                        mat.SetFloat("_DetailTriplanarSpace", (int)space);
                        EditorUtility.SetDirty(mat);
                     }
                  }
                  
               }
               else
               {
                  DoTriplanarSpace(mat, materialEditor, props, "_DetailTriplanarSpace", "_DetailTriplanarContrast",
                     "_DETIALTRIPLANARBARY", "_DetailTriplanarBaryBlend", "_DETAILTRIPLANARFLATBLEND");
               }
               var detailTex = FindProperty("_DetailMap", props);
               if (mat.IsKeywordEnabled("_DETAIL"))
               {
                  if (detailTex.textureValue == null)
                  {
                     detailTex.textureValue = FindDefaultTexture("betterlit_default_detail");
                  }
                  WarnLinear(detailTex.textureValue);
               }
               DoTextureUI(materialEditor, props, detailUVMode, "Detail", "_DetailMap", true, "_DetailTextureRotation", "_DETAILTEXTUREROTATION");
               materialEditor.ShaderProperty(FindProperty("_DetailAlbedoStrength", props), new GUIContent("Detail Albedo Strength"));
               if (!IsUnlit() || IsMatCap())
               {
                  if (normalMode != NormalMode.FromHeight)
                  {
                     materialEditor.ShaderProperty(FindProperty("_DetailNormalStrength", props), new GUIContent("Detail Normal Strength"));
                  }
                  materialEditor.ShaderProperty(FindProperty("_DetailSmoothnessStrength", props), new GUIContent("Detail Smoothness Strength"));
               }
               DoStochastic(mat, materialEditor, props, "_DETAILSTOCHASTIC", "_DetailStochasticContrast", "_DetailStochasticScale");
               EditorGUI.indentLevel--;
            }
            
         }
         
      }

      public void OnTessGUI(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         if (mat != null && mat.HasProperty("_TessellationDisplacement") && mat.HasProperty("_TessellationMaxSubdiv") &&
            materialEditor != null && props != null)
         {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty("_TessellationDisplacement", props), "Displacement");
            if (mat.IsKeywordEnabled("_TESSEDGE"))
            {
               materialEditor.ShaderProperty(FindProperty("_TessellationMinEdgeLength", props), "Min Edge Length");
            }
            else
            {
               Vector2 dist = mat.GetVector("_TessellationDistanceRange");

               EditorGUI.BeginChangeCheck();
               dist = EditorGUILayout.Vector2Field("Fade Start/Falloff", dist);

               if (EditorGUI.EndChangeCheck())
               {
                  FindProperty("_TessellationDistanceRange", props).vectorValue = dist;
               }
            }

            
            materialEditor.ShaderProperty(FindProperty("_TessellationMaxSubdiv", props), "Max Subdivisions");
            materialEditor.ShaderProperty(FindProperty("_TessellationMipBias", props), "Mip Bias");
            materialEditor.ShaderProperty(FindProperty("_TessellationOffset", props), "Offset");
            EditorGUI.indentLevel--;
         }
      }


      enum LocalGlobalMode
      {
         Local = 0,
         Global = 1
      }

      static GUIContent CWetnessUseEffector = new GUIContent("Use Effector", "Use the effector system to add or remove wetness");
      public void DoWetness(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (IsUnlit())
            return;
         if (!mat.HasProperty("_WetnessAmount"))
            return;
         if (DrawRollupKeywordToggle(mat, "Wetness", "_WETNESS"))
         {
            LocalGlobalMode mode = (LocalGlobalMode)mat.GetInt("_WetnessMode");
            EditorGUI.BeginChangeCheck();
            mode = (LocalGlobalMode)EditorGUILayout.EnumPopup("Wetness Mode", mode);

            if (EditorGUI.EndChangeCheck())
            {
               mat.SetInt("_WetnessMode", (int)mode);
            }
            EditorGUI.indentLevel++;
            var old = GUI.enabled;
            GUI.enabled = mode == LocalGlobalMode.Local;
            materialEditor.ShaderProperty(FindProperty("_WetnessAmount", props), "Wetness Amount");
            GUI.enabled = old;

            materialEditor.ShaderProperty(FindProperty("_WetnessUseEffector", props), CWetnessUseEffector);
            if (mat.IsKeywordEnabled("_WETNESSEFFECTOR"))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_WetnessEffectorInvert", props), CWireEffectorInvert);
               EditorGUI.indentLevel--;
            }

            materialEditor.ShaderProperty(FindProperty("_WetnessMin", props), "Wetness Min");
            materialEditor.ShaderProperty(FindProperty("_WetnessMax", props), "Wetness Max");
            materialEditor.ShaderProperty(FindProperty("_WetnessFalloff", props), "Wetness Falloff");
            materialEditor.ShaderProperty(FindProperty("_WetnessAngleMin", props), "Wetness Angle Minimum");
            var shore = FindProperty("_WetnessShoreline", props);
            EditorGUILayout.BeginHorizontal();
            bool on = false;
            if (shore.floatValue > -9990)
            {
               on = true;
            }
            var newOn = EditorGUILayout.Toggle("Wetness Shore Height", on);
            if (newOn != on)
            {
               if (newOn)
                  shore.floatValue = 0;
               else
                  shore.floatValue = -9999;
               on = newOn;
            }
            var oldEnabled = GUI.enabled;
            GUI.enabled = on;
            if (on)
            {
               float nv = EditorGUILayout.FloatField(shore.floatValue);
               if (nv != shore.floatValue)
               {
                  shore.floatValue = nv;
               }
            }
            else
            {
               EditorGUILayout.FloatField(0);
            }
            GUI.enabled = oldEnabled;
            EditorGUILayout.EndHorizontal();
            materialEditor.ShaderProperty(FindProperty("_Porosity", props), "Porosity");
          
            EditorGUI.indentLevel--;
         }
      }

      enum Fresnel
      {
         None,
         Vertex,
         Normal
      }
      static GUIContent CUseTexturedNormal = new GUIContent("Textured Normal", "Use normal from normal map or vertex normal for fresnel effect?");
      public void DoFresnel(MaterialEditor materialEditor, MaterialProperty[] props,
         string keyword, string normKeyword, string colorProp, string paramProp)
      {
         Fresnel fres = Fresnel.None;
         if (mat.IsKeywordEnabled(keyword))
            fres = Fresnel.Vertex;
         else if (mat.IsKeywordEnabled(normKeyword))
            fres = Fresnel.Normal;
         
         Fresnel newFres = (Fresnel)EditorGUILayout.EnumPopup("Fresnel", fres);

         if (newFres != fres)
         {
            mat.DisableKeyword(keyword);
            mat.DisableKeyword(normKeyword);
            if (newFres == Fresnel.Vertex)
            {
               mat.EnableKeyword(keyword);
            }
            else if (newFres == Fresnel.Normal)
            {
               mat.EnableKeyword(normKeyword);
            }
         }

         
         if (newFres != Fresnel.None)
         {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty(colorProp, props), "Color");
            var dataProp = FindProperty(paramProp, props);
            Vector4 v4 = dataProp.vectorValue;
            EditorGUI.BeginChangeCheck();
            v4.x = EditorGUILayout.FloatField("Bias", v4.x);
            v4.y = EditorGUILayout.FloatField("Scale", v4.y);
            v4.z = EditorGUILayout.FloatField("Power", v4.z);
            if (EditorGUI.EndChangeCheck())
            {
               dataProp.vectorValue = v4;
            }
            EditorGUI.indentLevel--;
         }
      }

      public void DoSparkle(MaterialEditor materialEditor, MaterialProperty[] props,
         string keyword, string noiseTex, string paramProp)
      {
         bool enabled = mat.IsKeywordEnabled(keyword);
         bool newEnabled = EditorGUILayout.Toggle("Sparkle", enabled);
         if (newEnabled != enabled)
         {
            mat.DisableKeyword(keyword);
            if (newEnabled)
            {
               mat.EnableKeyword(keyword);
            }
         }
         if (newEnabled)
         {
            EditorGUI.indentLevel++;

            var prop = FindProperty(noiseTex, props);
            if (prop.textureValue == null)
            {
               prop.textureValue = FindDefaultTexture("betterlit_default_sparkle_noise");
            }
            materialEditor.TexturePropertySingleLine(new GUIContent("Noise Texture"), prop);
            var dataProp = FindProperty(paramProp, props);
            Vector4 v4 = dataProp.vectorValue;
            EditorGUI.BeginChangeCheck();
            v4.x = EditorGUILayout.FloatField("Tiling", v4.x);
            v4.y = EditorGUILayout.Slider("Cutoff", v4.y, 0, 1);
            v4.z = EditorGUILayout.Slider("Intensity", v4.z, 0, 1);
            v4.w = EditorGUILayout.Slider("Emission", v4.w, 0, 4);
            if (EditorGUI.EndChangeCheck())
            {
               dataProp.vectorValue = v4;
            }
            EditorGUI.indentLevel--;
         }
      }

      enum MatCapMode
      {
         Single,
         Layered
      }

      void KeywordToggle(string label, string keyword)
      {
         bool enabled = mat.IsKeywordEnabled(keyword);
         bool newEnabled = EditorGUILayout.Toggle(label, enabled);
         if (newEnabled != enabled)
         {
            mat.DisableKeyword(keyword);
            if (newEnabled)
               mat.EnableKeyword(keyword);
            EditorUtility.SetDirty(mat);
         }
         
      }
      public void DoMatCap(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_MainMatcap"))
            return;
         if (DrawRollupKeywordToggle(mat, "Mat Cap", "_USEMATCAP"))
         {
            LocalGlobalMode mode = mat.IsKeywordEnabled("_USEGLOBALMATCAP") ? LocalGlobalMode.Global : LocalGlobalMode.Local;
            EditorGUI.BeginChangeCheck();
            mode = (LocalGlobalMode)EditorGUILayout.EnumPopup("Scope", mode);

            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword("_USEGLOBALMATCAP");
               if (mode == LocalGlobalMode.Global)
               {
                  mat.EnableKeyword("_USEGLOBALMATCAP");
               }
            }
            
            MatCapMode matCapMode = MatCapMode.Layered;
            if (IsUnlit())
            {
               matCapMode = mat.IsKeywordEnabled("_MULTIMATCAP") ? MatCapMode.Layered : MatCapMode.Single;
               EditorGUI.BeginChangeCheck();
               matCapMode = (MatCapMode)EditorGUILayout.EnumPopup("Mode", matCapMode);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.DisableKeyword("_MULTIMATCAP");
                  if (matCapMode == MatCapMode.Layered)
                  {
                     mat.EnableKeyword("_MULTIMATCAP");
                  }
               }
            }
            if (mode == LocalGlobalMode.Global)
            {
               if (matCapMode == MatCapMode.Layered)
               {
                  if (!IsUnlit())
                  {
                     // enable this, so if we switch to unlit it stays..
                     if (!mat.IsKeywordEnabled("_MULTIMATCAP"))
                     {
                        mat.EnableKeyword("_MULTIMATCAP");
                     }
                     EditorGUILayout.HelpBox("In this mode, you can have a matcap per texture layer. Any matcap texture left empty will fall through to the full lighting pipeline.", MessageType.Info);
                  }
                  else
                  {
                     EditorGUILayout.HelpBox("In this mode, you can have a matcap per texture layer", MessageType.Info);
                  }

                  EditorGUILayout.HelpBox("You are set to use global texture properties. You will have to set them from code", MessageType.Info);
                  KeywordToggle("Use Main Matcap", "_MAINMATCAP");
                  KeywordToggle("Use Layer0 Matcap", "_LAYER0MATCAP");
                  KeywordToggle("Use Layer1 Matcap", "_LAYER1MATCAP");
                  KeywordToggle("Use Layer2 Matcap", "_LAYER2MATCAP");
                  KeywordToggle("Use Layer3 Matcap", "_LAYER3MATCAP");

               }
               else
               {
                  EditorGUILayout.HelpBox("You are set to use global texture properties. You will have to set them from code (_GMainMatcap)", MessageType.Info);
               }
            }
            else
            {
               if (matCapMode == MatCapMode.Single)
               {
                  materialEditor.TexturePropertySingleLine(new GUIContent("MatCap Texture"), FindProperty("_MainMatcap", props));
               }
               else
               {
                  if (!IsUnlit())
                  {
                     // enable this, so if we switch to unlit it stays..
                     if (!mat.IsKeywordEnabled("_MULTIMATCAP"))
                     {
                        mat.EnableKeyword("_MULTIMATCAP");
                     }
                     EditorGUILayout.HelpBox("In this mode, you can have a matcap per texture layer. Any matcap texture left empty will fall through to the full lighting pipeline.", MessageType.Info);
                  }
                  else
                  {
                     EditorGUILayout.HelpBox("In this mode, you can have a matcap per texture layer", MessageType.Info);
                  }

                  EditorGUI.BeginChangeCheck();
                  materialEditor.TexturePropertySingleLine(new GUIContent("Main MatCap"), FindProperty("_MainMatcap", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer0 MatCap"), FindProperty("_Layer0Matcap", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer1 MatCap"), FindProperty("_Layer1Matcap", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer2 MatCap"), FindProperty("_Layer2Matcap", props));
                  materialEditor.TexturePropertySingleLine(new GUIContent("Layer3 MatCap"), FindProperty("_Layer3Matcap", props));
                  if (EditorGUI.EndChangeCheck())
                  {
                     if (FindProperty("_MainMatcap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_MAINMATCAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_MAINMATCAP");
                     }
                     if (FindProperty("_Layer0Matcap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_LAYER0MATCAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_LAYER0MATCAP");
                     }
                     if (FindProperty("_Layer1Matcap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_LAYER1MATCAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_LAYER1MATCAP");
                     }
                     if (FindProperty("_Layer2Matcap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_LAYER2MATCAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_LAYER2MATCAP");
                     }
                     if (FindProperty("_Layer3Matcap", props).textureValue != null)
                     {
                        mat.EnableKeyword("_LAYER3MATCAP");
                     }
                     else
                     {
                        mat.DisableKeyword("_LAYER3MATCAP");
                     }
                  }

               }
            }
            
         }
      }

      enum SnowWorldFadeMode
      {
         Off = 0,
         On,
         Global
      }
      static GUIContent CSnowUseEffector = new GUIContent("Use Effector", "Use effector system to modify the snow amount");
      public void DoSnow(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_SnowMode"))
            return;
         if (DrawRollupKeywordToggle(mat, "Snow", "_SNOW"))
         {
            LocalGlobalMode mode = (LocalGlobalMode)mat.GetInt("_SnowMode");
            EditorGUI.BeginChangeCheck();
            mode = (LocalGlobalMode)EditorGUILayout.EnumPopup("Snow Mode", mode);

            if (EditorGUI.EndChangeCheck())
            {
               mat.SetInt("_SnowMode", (int)mode);
            }
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            materialEditor.TexturePropertySingleLine(new GUIContent("Snow Albedo"), FindProperty("_SnowAlbedo", props), FindProperty("_SnowTint", props));
            materialEditor.TextureScaleOffsetProperty(FindProperty("_SnowAlbedo", props));
            if (!IsUnlit())
            {
               if (!mat.IsKeywordEnabled("_AUTONORMAL"))
               {
                  if (mat.IsKeywordEnabled("_PACKEDFAST"))
                  {
                     WarnLinear(FindProperty("_SnowNormal", props).textureValue);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Snow Packed"), FindProperty("_SnowNormal", props));
                  }
                  else
                  {
                     WarnNormal(FindProperty("_SnowNormal", props).textureValue);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Snow Normal"), FindProperty("_SnowNormal", props));
                  }

               }
               if (!mat.IsKeywordEnabled("_PACKEDFAST"))
               {
                  WarnLinear(FindProperty("_SnowMaskMap", props).textureValue);
                  materialEditor.TexturePropertySingleLine(new GUIContent("Snow Mask"), FindProperty("_SnowMaskMap", props));
               }
            }
            if (mode == LocalGlobalMode.Local)
            {
               materialEditor.ShaderProperty(FindProperty("_SnowAmount", props), "Snow Amount");
            }
            materialEditor.ShaderProperty(FindProperty("_SnowUseEffector", props), CSnowUseEffector);
            if (mat.IsKeywordEnabled("_SNOWEFFECTOR"))
            {
               EditorGUI.indentLevel++;
               materialEditor.ShaderProperty(FindProperty("_SnowEffectorInvert", props), CWireEffectorInvert);
               EditorGUI.indentLevel--;
            }
            DoStochastic(mat, materialEditor, props, "_SNOWSTOCHASTIC", "_SnowStochasticContrast", "_SnowStochasticScale");
            materialEditor.ShaderProperty(FindProperty("_SnowAngle", props), "Snow Angle Falloff");
            materialEditor.ShaderProperty(FindProperty("_SnowContrast", props), "Snow Contrast");

            Vector3 worldData = mat.GetVector("_SnowWorldFade");
            EditorGUI.BeginChangeCheck();
            SnowWorldFadeMode sfm = (SnowWorldFadeMode)worldData.z;
            sfm = (SnowWorldFadeMode)EditorGUILayout.EnumPopup("World Height Fade", sfm);
            worldData.z = (int)sfm;
            bool old = GUI.enabled;
            GUI.enabled = sfm == SnowWorldFadeMode.On;
            EditorGUI.indentLevel++;
            worldData.x = EditorGUILayout.FloatField("Start Height", worldData.x);
            worldData.y = EditorGUILayout.FloatField("Fade In Range", worldData.y);
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
               mat.SetVector("_SnowWorldFade", worldData);
               EditorUtility.SetDirty(mat);
            }
            GUI.enabled = old;
            materialEditor.ShaderProperty(FindProperty("_SnowVertexHeight", props), "Snow Vertex Offset");
            if (EditorGUI.EndChangeCheck())
            {
               if (mat.GetTexture("_SnowMaskMap") != null)
               {
                  mat.EnableKeyword("_SNOWMASKMAP");
               }
               else
               {
                  mat.DisableKeyword("_SNOWMASKMAP");
               }
               if (mat.GetTexture("_SnowNormal") != null)
               {
                  mat.EnableKeyword("_SNOWNORMALMAP");
               }
               else
               {
                  mat.DisableKeyword("_SNOWNORMALMAP");
               }
               
            }

            bool noise = mat.IsKeywordEnabled("_SNOWNOISE");
            EditorGUI.BeginChangeCheck();
            noise = EditorGUILayout.Toggle("Transition Noise", noise);
            if (EditorGUI.EndChangeCheck())
            {
               mat.DisableKeyword("_SNOWNOISE");
               if (noise)
               {
                  mat.EnableKeyword("_SNOWNOISE");
               }
            }
            if (noise)
            {
               EditorGUI.indentLevel++;
               materialEditor.FloatProperty(FindProperty("_SnowNoiseFreq", props), "Frequency");
               materialEditor.FloatProperty(FindProperty("_SnowNoiseAmp", props), "Amplitude");
               materialEditor.FloatProperty(FindProperty("_SnowNoiseOffset", props), "Offset");
               EditorGUI.indentLevel--;
            }

            DoFresnel(materialEditor, props, "_SNOWFRESNEL", "_SNOWFRESNELNORMAL", "_SnowFresnelColor", "_SnowFresnelBSP");
            DoSparkle(materialEditor, props, "_SNOWSPARKLES", "_SnowSparkleNoise", "_SnowSparkleTCI");

            if (mat.IsKeywordEnabled("_TRAX_ON"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Trax Snow Albedo"), FindProperty("_SnowTraxAlbedo", props), FindProperty("_SnowTraxTint", props));
               materialEditor.TextureScaleOffsetProperty(FindProperty("_SnowTraxAlbedo", props));
               if (!IsUnlit())
               {
                  if (!mat.IsKeywordEnabled("_AUTONORMAL"))
                  {
                     if (mat.IsKeywordEnabled("_PACKEDFAST"))
                     {
                        WarnLinear(FindProperty("_SnowTraxNormal", props).textureValue);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Trax Snow Packed"), FindProperty("_SnowTraxNormal", props));
                     }
                     else
                     {
                        WarnNormal(FindProperty("_SnowTraxNormal", props).textureValue);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Trax Snow Normal"), FindProperty("_SnowTraxNormal", props));
                     }

                  }
                  if (!mat.IsKeywordEnabled("_PACKEDFAST"))
                  {
                     WarnLinear(FindProperty("_SnowTraxMaskMap", props).textureValue);
                     materialEditor.TexturePropertySingleLine(new GUIContent("Trax Snow Mask"), FindProperty("_SnowTraxMaskMap", props));
                  }
               }
            }
            EditorGUI.indentLevel--;
         }
      }

      public void DoHDRPExtra(MaterialEditor materialEditor, MaterialProperty[] props)
      {
#if USING_HDRP

         materialEditor.ShaderProperty(FindProperty("_EnableGeometrySpecAA", props), new GUIContent("Enable Geometry Specular AA"));
         if (mat.IsKeywordEnabled("_ENABLE_GEOMETRIC_SPECULAR_AA"))
         {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty("_SpecularAAScreenSpaceVariance", props), new GUIContent("Screen Space Variance"));
            materialEditor.ShaderProperty(FindProperty("_SpecularAAThreshold", props), new GUIContent("Threshold"));
            EditorGUI.indentLevel--;
         }
         materialEditor.ShaderProperty(FindProperty("_DisableDecals", props), new GUIContent("Disable Decals"));
         materialEditor.ShaderProperty(FindProperty("_DisableSSR", props), new GUIContent("Disable SSR"));
         materialEditor.ShaderProperty(FindProperty("_WriteTransparentMotionVector", props), new GUIContent("Write Transparent Motion Vectors"));
         materialEditor.ShaderProperty(FindProperty("_AddPrecomputedVelocity", props), new GUIContent("Add Precomputed Velocity"));
#endif
      }

      enum DebugMode
      {
         Off,
         SampleCount,
         Barycentrics,
         ShapeMask,
         VertexColor,
         WorldNormal,
         WorldTangent,
         UV0,
         FinalAlbedo,
         FinalNormalTangent,
         FinalNormalWorld,
         FinalSmoothness,
         FinalAO,
         FinalMetallic,
         FinalEmission
      }

      public void DoDebugGUI(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (!mat.HasProperty("_DebugSampleCountThreshold"))
            return;
         DebugMode mode = DebugMode.Off;
         if (mat.IsKeywordEnabled("_DEBUG_SAMPLECOUNT"))
         {
            mode = DebugMode.SampleCount;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_SHAPEWEIGHTMASK"))
         {
            mode = DebugMode.ShapeMask;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_BARYCENTRICS"))
         {
            mode = DebugMode.Barycentrics;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_VERTEXCOLOR"))
         {
            mode = DebugMode.VertexColor;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_WORLDNORMAL"))
         {
            mode = DebugMode.WorldNormal;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_WORLDTANGENT"))
         {
            mode = DebugMode.WorldTangent;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_UV0"))
         {
            mode = DebugMode.UV0;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALALBEDO"))
         {
            mode = DebugMode.FinalAlbedo;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALNORMALTANGENT"))
         {
            mode = DebugMode.FinalNormalTangent;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALNORMALWORLD"))
         {
            mode = DebugMode.FinalNormalWorld;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALSMOOTHNESS"))
         {
            mode = DebugMode.FinalSmoothness;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALAO"))
         {
            mode = DebugMode.FinalAO;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALMETALLIC"))
         {
            mode = DebugMode.FinalMetallic;
         }
         else if (mat.IsKeywordEnabled("_DEBUG_FINALEMISSION"))
         {
            mode = DebugMode.FinalEmission;
         }

         var nm = (DebugMode)EditorGUILayout.EnumPopup("Debug Mode", mode);
         if (nm != mode)
         {
            mat.DisableKeyword("_DEBUG_SAMPLECOUNT");
            mat.DisableKeyword("_DEBUG_BARYCENTRICS");
            mat.DisableKeyword("_DEBUG_SHAPEWEIGHTMASK");
            mat.DisableKeyword("_DEBUG_VERTEXCOLOR");
            mat.DisableKeyword("_DEBUG_WORLDNORMAL");
            mat.DisableKeyword("_DEBUG_WORLDTANGENT");
            mat.DisableKeyword("_DEBUG_UV0");
            mat.DisableKeyword("_DEBUG_FINALALBEDO");
            mat.DisableKeyword("_DEBUG_FINALNORMALTANGENT");
            mat.DisableKeyword("_DEBUG_FINALNORMALWORLD");
            mat.DisableKeyword("_DEBUG_FINALSMOOTHNESS");
            mat.DisableKeyword("_DEBUG_FINALAO");
            mat.DisableKeyword("_DEBUG_FINALMETALLIC");
            mat.DisableKeyword("_DEBUG_FINALEMISSION");

            if (nm == DebugMode.SampleCount)
            {
               mat.EnableKeyword("_DEBUG_SAMPLECOUNT");
            }
            else if (nm == DebugMode.Barycentrics)
            {
               mat.EnableKeyword("_DEBUG_BARYCENTRICS");
            }
            else if (nm == DebugMode.ShapeMask)
            {
               mat.EnableKeyword("_DEBUG_SHAPEWEIGHTMASK");
            }
            else if (nm == DebugMode.VertexColor)
            {
               mat.EnableKeyword("_DEBUG_VERTEXCOLOR");
            }
            else if (nm == DebugMode.WorldNormal)
            {
               mat.EnableKeyword("_DEBUG_WORLDNORMAL");
            }
            else if (nm == DebugMode.WorldTangent)
            {
               mat.EnableKeyword("_DEBUG_WORLDTANGENT");
            }
            else if (nm == DebugMode.UV0)
            {
               mat.EnableKeyword("_DEBUG_UV0");
            }
            else if (nm == DebugMode.FinalAlbedo)
            {
               mat.EnableKeyword("_DEBUG_FINALALBEDO");
            }
            else if (nm == DebugMode.FinalNormalTangent)
            {
               mat.EnableKeyword("_DEBUG_FINALNORMALTANGENT");
            }
            else if (nm == DebugMode.FinalNormalWorld)
            {
               mat.EnableKeyword("_DEBUG_FINALNORMALWORLD");
            }
            else if (nm == DebugMode.FinalSmoothness)
            {
               mat.EnableKeyword("_DEBUG_FINALSMOOTHNESS");
            }
            else if (nm == DebugMode.FinalAO)
            {
               mat.EnableKeyword("_DEBUG_FINALAO");
            }
            else if (nm == DebugMode.FinalMetallic)
            {
               mat.EnableKeyword("_DEBUG_FINALMETALLIC");
            }
            else if (nm == DebugMode.FinalEmission)
            {
               mat.EnableKeyword("_DEBUG_FINALEMISSION");
            }

         }
         if (nm == DebugMode.SampleCount)
         {
            EditorGUILayout.HelpBox("The shader will draw red when texture samples are greater than the Debug Sample Threshold, and blue when below it. This can let you see exactly how many samples are needed for the shader based on it's current configuration", MessageType.Info);
            materialEditor.ShaderProperty(FindProperty("_DebugSampleCountThreshold", props), "Debug Sample Threshold");
         }

      }

      public void DoVertexColor(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (DrawRollupKeywordToggle(mat, "Vertex Coloring", "_VERTEXTINT"))
         {
            materialEditor.ShaderProperty(FindProperty("_VertexColor", props), "Vertex Color Blend");
            materialEditor.ShaderProperty(FindProperty("_VertexAO", props), "Vertex Alpha Blend");
         }
      }


      enum ColorSideMode
      {
         None,
         Color,
         Gradient,
         Texture
      }

      enum ColorSideSpace
      {
         UV = 0,
         Local,
         World
      }

      void DoColorSide(MaterialEditor materialEditor, MaterialProperty[] props,
            string label, string colorKeyword, string texkeyword, string gradkeyword,
           string colorProp, string colorProp2, string texProp, string rangeProp, string clampProp)
      {
         ColorSideMode csm = ColorSideMode.None;
         if (mat.IsKeywordEnabled(colorKeyword))
         {
            csm = ColorSideMode.Color;
         }
         else if (mat.IsKeywordEnabled(texkeyword))
         {
            csm = ColorSideMode.Texture;
         }
         else if (mat.IsKeywordEnabled(gradkeyword))
         {
            csm = ColorSideMode.Gradient;
         }

         var ncsm = (ColorSideMode)EditorGUILayout.EnumPopup(label, csm);
         if (ncsm != csm)
         {
            csm = ncsm;
            mat.DisableKeyword(texkeyword);
            mat.DisableKeyword(gradkeyword);
            mat.DisableKeyword(colorKeyword);
            if (ncsm == ColorSideMode.Color)
            {
               mat.EnableKeyword(colorKeyword);
            }
            else if (ncsm == ColorSideMode.Texture)
            {
               mat.EnableKeyword(texkeyword);
            }
            else if (ncsm == ColorSideMode.Gradient)
            {
               mat.EnableKeyword(gradkeyword);
            }
         }
         EditorGUI.indentLevel++;
         if (csm == ColorSideMode.Color)
         {
            materialEditor.ColorProperty(FindProperty(colorProp, props), "Color");
         }
         else if (csm == ColorSideMode.Gradient)
         {
            EditorGUILayout.BeginHorizontal();
            materialEditor.ColorProperty(FindProperty(colorProp, props), "Color");
            materialEditor.ColorProperty(FindProperty(colorProp2, props), "");
            EditorGUILayout.EndHorizontal();
         }
         else if (csm == ColorSideMode.Texture)
         {
            materialEditor.TexturePropertySingleLine(new GUIContent("Albedo"), FindProperty(texProp, props));
         }
         if (csm != ColorSideMode.Color && csm != ColorSideMode.None)
         {
            var prop = FindProperty(rangeProp, props);
            var data = prop.vectorValue;
            ColorSideSpace space = (ColorSideSpace)(int)data.w;
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            space = (ColorSideSpace)EditorGUILayout.EnumPopup("Space", space);
            data.w = (int)space;
            data.x = EditorGUILayout.FloatField("Start", data.x);
            data.y = EditorGUILayout.FloatField("Size", data.y);
            data.z = EditorGUILayout.Slider("Rotation", data.z, -Mathf.PI, Mathf.PI);
            if (EditorGUI.EndChangeCheck())
            {
               prop.vectorValue = data;
            }
            if (csm == ColorSideMode.Texture)
            {
               Vector2 vals = FindProperty(clampProp, props).vectorValue;
               bool clamped = vals.x > 0 && vals.y < 1.0f;
               bool newClamped = EditorGUILayout.Toggle("Clamp UV Range", clamped);
               if (newClamped != clamped)
               {
                  if (newClamped)
                  {
                     FindProperty(clampProp, props).vectorValue = new Vector4(0.001f, 0.999f);
                  }
                  else
                  {
                     FindProperty(clampProp, props).vectorValue = new Vector4(-99999, 99999);
                  }
               }
            }
            EditorGUI.indentLevel--;
         }
         EditorGUI.indentLevel--;

      }

      enum BlendMode6Side
      {
         Tint,
         Multiply2X,
         Overlay
      }

      public void Do6SidedColor(MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (FindProperty("_6SidedSpace", props) == null)
            return;
         if (DrawRollupKeywordToggle(mat, "6 Sided Gradient Tint", "_6TINT"))
         {
            EditorGUILayout.HelpBox("Apply a tint to each side of the object, using a color, a 2 color gradient, or a texture", MessageType.Info);
            
            BlendMode6Side mode = BlendMode6Side.Tint;
            if (mat.IsKeywordEnabled("_6SIDEBLEND_MULT2X"))
               mode = BlendMode6Side.Multiply2X;
            else if (mat.IsKeywordEnabled("_6SIDEBLEND_OVERLAY"))
               mode = BlendMode6Side.Overlay;

            var newMode = (BlendMode6Side)EditorGUILayout.EnumPopup("Blend Mode", mode);
            if (newMode != mode)
            {
               mat.DisableKeyword("_6SIDEBLEND_MULT2X");
               mat.DisableKeyword("_6SIDEBLEND_OVERLAY");
               if (newMode == BlendMode6Side.Multiply2X)
               {
                  mat.EnableKeyword("_6SIDEBLEND_MULT2X");
               }
               else if (newMode == BlendMode6Side.Overlay)
               {
                  mat.EnableKeyword("_6SIDEBLEND_OVERLAY");
               }
            }

            materialEditor.ShaderProperty(FindProperty("_6SidedSpace", props), new GUIContent("Blend Space", "Color Space to perform blending"));
            materialEditor.ShaderProperty(FindProperty("_6SidedAngleContrast", props), new GUIContent("Angle Contrast", "How sharp is the transition between sides"));


            DoColorSide(materialEditor, props, "X Facing", "_POSX_COLOR", "_POSX_TEXTURE", "_POSX_GRADIENT", "_ColorX", "_ColorX2", "_GradientX", "_ColorXRange", "_TextureClampX");
            DoColorSide(materialEditor, props, "Y Facing", "_POSY_COLOR", "_POSY_TEXTURE", "_POSY_GRADIENT", "_ColorY", "_ColorY2", "_GradientY", "_ColorYRange", "_TextureClampY");
            DoColorSide(materialEditor, props, "Z Facing", "_POSZ_COLOR", "_POSZ_TEXTURE", "_POSZ_GRADIENT", "_ColorZ", "_ColorZ2", "_GradientZ", "_ColorZRange", "_TextureClampZ");
            DoColorSide(materialEditor, props, "-X Facing", "_NEGX_COLOR", "_NEGX_TEXTURE", "_NEGX_GRADIENT", "_ColorNX", "_ColorNX2", "_GradientNX", "_ColorNXRange", "_TextureClampNX");
            DoColorSide(materialEditor, props, "-Y Facing", "_NEGY_COLOR", "_NEGY_TEXTURE", "_NEGY_GRADIENT", "_ColorNY", "_ColorNY2", "_GradientNY", "_ColorNYRange", "_TextureClampNY");
            DoColorSide(materialEditor, props, "-Z Facing", "_NEGZ_COLOR", "_NEGZ_TEXTURE", "_NEGZ_GRADIENT", "_ColorNZ", "_ColorNZ2", "_GradientNZ", "_ColorNZRange", "_TextureClampNZ");
         }
      }

      public void OnEffectsGUI(MaterialEditor materialEditor, MaterialProperty[] props)
      { 
         DoWetness(materialEditor, props);
         DoPuddles(materialEditor, props);
         DoSnow(materialEditor, props);
         DoDebugGUI(materialEditor, props);
      }
   }
}