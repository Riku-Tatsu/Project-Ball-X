using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Looping25D
{
    public static class MaterialShaderFixer
    {
        public enum RenderPiplelineType
        {
            URP, HDRP, Standard
        }

        class MaterialProperties
        {
            public Color Color;
            public string TextureName;
            public string TexturePath;

            public MaterialProperties(Color color)
            {
                Color = color;
            }

            public MaterialProperties(Color color, string textureName, string texturePath)
            {
                Color = color;
                TextureName = textureName;
                TexturePath = texturePath;
            }
        }

        static Dictionary<string, MaterialProperties> Materials = new Dictionary<string, MaterialProperties> {
            { "Assets/2.5D Looping/Examples/Materials/Sky.mat", 
                new MaterialProperties(Color.white, "_ColorBaseMap", "Assets/2.5D Looping/Examples/Textures/Sky.png" ) },
            { "Assets/2.5D Looping/Examples/Materials/TreeLeavesGreen.mat", 
                new MaterialProperties(new Color(0.15f, 0.5f, 0.15f)) },
            { "Assets/2.5D Looping/Examples/Materials/TreeLeavesYellow.mat", 
                new MaterialProperties(new Color(0.9f, 0.8f, 0.0f)) },
            { "Assets/2.5D Looping/Examples/Materials/TreeTrunk.mat", 
                new MaterialProperties(new Color(0.55f, 0.25f, 0.0f)) },
            { "Assets/2.5D Looping/Examples/Materials/BridgePartTextured.mat", 
                new MaterialProperties(Color.white, "_ColorBaseMap", "Assets/2.5D Looping/Examples/Textures/BridgePart.png" ) },
            { "Assets/2.5D Looping/Examples/Materials/Ball.mat", 
                new MaterialProperties(Color.white, "_ColorBaseMap", "Assets/2.5D Looping/Examples/Textures/Ball.psd" ) },
        };

        static RenderPiplelineType _createdForRenderPipleline;
        static double nextCheckTime;
        static double startTime;

        public static void FixMaterialsAsync(RenderPiplelineType createdForRenderPipleline)
        {
            // Materials may not be loaded at this time. Thus we wait for them to be imported.
            _createdForRenderPipleline = createdForRenderPipleline;
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;
            startTime = EditorApplication.timeSinceStartup;
        }

        static void onEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - nextCheckTime > 0)
            {
                nextCheckTime = EditorApplication.timeSinceStartup + 0.2;
                if (doMaterialsExist())
                {
                    EditorApplication.update -= onEditorUpdate;
                    FixMaterials(_createdForRenderPipleline);
                    return;
                }
            }

            // limit wait time if materials are not found
            if (EditorApplication.timeSinceStartup - startTime > 3)
            {
                EditorApplication.update -= onEditorUpdate;
                FixMaterials(_createdForRenderPipleline);
            }
        }

        static bool doMaterialsExist()
        {
            foreach (var kv in Materials)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(kv.Key);
                if (material == null)
                    return false;
            }

            return true;
        }

        public static void FixMaterials(RenderPiplelineType createdForRenderPipleline)
        {
            var currentRenderPipline = GetCurrentRenderPiplelineType();

            if (currentRenderPipline != createdForRenderPipleline)
            {
                EditorUtility.DisplayDialog(
                    "Render pipeline mismatch detected.",
                    "The materials in this asset have been created with the Universal Render Pipeline (URP). You are using '" + currentRenderPipline.ToString() + "', thus some of the materials may be broken.\n\nThe tool will attempt to auto update materials now. In case some are still broken afterwards please fix those manually.",
                    "Understood"
                    );

                Material material;
                Shader shader = GetDefaultShader();
                foreach (var kv in Materials)
                {
                    material = AssetDatabase.LoadAssetAtPath<Material>(kv.Key);
                    if (material != null)
                    {
                        material.shader = shader;
                        material.color = kv.Value.Color;
                        if(!string.IsNullOrEmpty(kv.Value.TextureName))
                        {
                            var texture = AssetDatabase.LoadAssetAtPath<Texture>(kv.Value.TexturePath);
                            if(texture != null)
                            {
                                material.SetTexture(kv.Value.TextureName, texture);
                                material.mainTexture = texture;
                            }
                        }
                        EditorUtility.SetDirty(material);
                    }
                }

                CheckPackages.CheckForPackage("com.unity.shadergraph", (found) =>
                {
                    if (found)
                    {
                        // Fix tri planar shader
                        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Looping/Examples/Materials/2.5D Terrain TriPlanar.mat");
                        if (material != null)
                        {
                            if (currentRenderPipline == RenderPiplelineType.URP)
                            {
                                shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/2.5D Looping/Examples/Shaders/URP/TriPlanar.shadergraph");
                                material.shader = shader;
                            }
                            else if (currentRenderPipline == RenderPiplelineType.HDRP)
                            {
                                shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/2.5D Looping/Examples/Shaders/HDRP/TriPlanar.shadergraph");
                                material.shader = shader;
                            }
                            else if (currentRenderPipline == RenderPiplelineType.Standard)
                            {
                                shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/2.5D Looping/Examples/Shaders/BuiltIn/TriPlanar.shader");
                                material.shader = shader;
                            }
                        }
                    }
                    else
                    { 
                        string msg = "Shader Graph Package is not installed.\n\nTo use the provided Tri-Planar shader you'll have to install shader graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/ \n\nFor now the 'Standard' shader will be assigned to all Tri-Planar materials";
                        EditorUtility.DisplayDialog("Shader Graph Package is not installed!", msg, "Okay");

                        // Revert shadergraph shader to default shader if shadergraph package is not installed
                        var shader = GetDefaultShader();
                        if (shader != null)
                        {
                            
                            Debug.LogWarning("Looping 2.5D Demo Scene: Shader Graph Package is not installed. Falling back to default shader.");

                            Material material;
                            material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Looping/Examples/Materials/2.5D Terrain TriPlanar.mat");
                            if (material != null)
                            {
                                material.shader = shader;
                                material.color = new Color(0.1f, 0.600f, 0.1f);
                            }

                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            Debug.LogError("No default shader found!");
                        }
                    }
                });
            }

            AssetDatabase.SaveAssets();
        }

        public static RenderPiplelineType GetCurrentRenderPiplelineType()
        {
            // Assume URP as default
            var renderPipeline = RenderPiplelineType.URP;

            // check if Standard or HDRP
            if (getUsedRenderPipeline() == null)
                renderPipeline = RenderPiplelineType.Standard; // Standard
            else if (!getUsedRenderPipeline().GetType().Name.Contains("Universal"))
                renderPipeline = RenderPiplelineType.HDRP; // HDRP

            return renderPipeline;
        }

        public static Shader GetDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        /// <summary>
        /// Returns the current pipline. Returns NULL if it's the standard render pipeline.
        /// </summary>
        /// <returns></returns>
        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

    }
}