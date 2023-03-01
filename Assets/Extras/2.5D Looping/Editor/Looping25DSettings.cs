using UnityEditor;
using UnityEngine;

namespace Kamgam.Looping25D
{
    public class Looping25DSettings : ScriptableObject
    {
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            Looping25DSettings.GetOrCreateSettings();
        }

        public const string Version = "1.0.3";
        public const string SettingsFilePath = "Assets/2.5D Looping Settings.asset";
		
        protected static Looping25DSettings cachedSettings;

        public static Looping25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<Looping25DSettings>(SettingsFilePath);

                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:Looping25DSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<Looping25DSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<Looping25DSettings>();


                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();

                    MaterialShaderFixer.FixMaterialsAsync(MaterialShaderFixer.RenderPiplelineType.URP);
                }
            }
            return cachedSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = Looping25DSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Looping 2.5D settings could not be found or created.", "Ok");
            }
        }
    }
}