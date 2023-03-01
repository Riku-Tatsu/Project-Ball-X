using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JBooth.BetterLit
{
   [CustomEditor(typeof(ShapeEffectorGroup))]
   public class ShapeEffectorGroupEditor : Editor
   {
      [UnityEditor.MenuItem("Window/Better Lit Shader/Create Shape Effector Group")]
      static void AddGroup()
      {
         GameObject go = new GameObject("Effector Group");
         go.AddComponent<ShapeEffectorGroup>();
      }

      public override void OnInspectorGUI()
      {
         ShapeEffectorGroup group = target as ShapeEffectorGroup;
         if (group.effectors.Count > 4)
         {
            Debug.LogError("Cannot have more than 4 effectors in a group, removing additional effectors");
            while (group.effectors.Count > 4)
            {
               group.effectors.RemoveAt(4);
            }
         }
         if (GUILayout.Button("Create Effector"))
         {
            if (group.effectors.Count > 3)
            {
               Debug.LogError("Cannot have more than 4 effectors in a group");
               return;
            }
            GameObject go = new GameObject("Effector");
            var e = go.AddComponent<ShapeEffector>();
            
            group.effectors.Add(e);
            e.transform.parent = group.transform;
         }
         DrawDefaultInspector();
      }
   }


}