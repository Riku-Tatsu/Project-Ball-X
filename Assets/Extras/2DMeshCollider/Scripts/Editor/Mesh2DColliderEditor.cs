using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Mesh2DCollider))]
[CanEditMultipleObjects]
public class Mesh2DColliderEditor : Editor
{
    private bool advancedFoldout;

    public override void OnInspectorGUI()
    {
        var M2DC = target as Mesh2DCollider;

        if (M2DC.isMeshBased)
        {
            if (M2DC.isEdge) { EditorGUILayout.LabelField("Type: Edge Collider (Mesh)"); }
            else { EditorGUILayout.LabelField("Type: Polygon Collider (Mesh)"); }
        }
        else
        {
            EditorGUILayout.LabelField("Type: Edge Collider (Terrain)");
        }

        if (GUILayout.Button("Select Collider"))
        {
            if (!M2DC.Collider)
            {
                M2DC.UpdateCollider();
            }
            Selection.activeObject = M2DC.Collider;
        }

        EditorGUI.BeginChangeCheck();

        //var extend = EditorGUILayout.Toggle("Add Thickness", M2DC.Extend);
        var updatePosOnly = !EditorGUILayout.Toggle("Update Shape", !M2DC.updatePosOnly);
        if (GUILayout.Button("Update Collider"))
        {
            if (M2DC.Collider)
            {
                Undo.RecordObject(M2DC.Collider, "Updated PolygonCollider");
            }
            M2DC.UpdateCollider();
        }

        bool update = false;

        advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced");
        if (advancedFoldout)
        {
            Mesh refMesh = null;
            TerrainData refTerrain = null;
            if (M2DC.isMeshBased)
            {
                refMesh = EditorGUILayout.ObjectField("Reference Mesh", M2DC.ReferenceMesh, typeof(Mesh), true) as Mesh;
            }
            else
            {
                refTerrain = EditorGUILayout.ObjectField("Reference Terrain", M2DC.ReferenceTerrain, typeof(TerrainData), false) as TerrainData;
            }
            Collider2D collider;
            if (M2DC.isEdge)
            {
                collider = EditorGUILayout.ObjectField("Edge Collider", M2DC.Collider, typeof(EdgeCollider2D), true) as Collider2D; 
            }
            else
            {
                collider = EditorGUILayout.ObjectField("Polygon Collider", M2DC.Collider, typeof(PolygonCollider2D), true) as Collider2D;
            }
            GUILayout.BeginHorizontal();
            var depth = EditorGUILayout.FloatField("Z-Depth", M2DC.Depth);
            if (GUILayout.Button("Match depth to pivot"))
            {
                depth = M2DC.transform.position.z;
                update = true;
            }
            GUILayout.EndHorizontal();
            var extraSampleOffsets = new List<float>();
            if (M2DC.ExtraSampleOffsets != null)
            {
                extraSampleOffsets = new List<float>(M2DC.ExtraSampleOffsets);
            }
            EditorGUILayout.LabelField("Extra Sample Offsets");
            //if (!Depths) { Depths = new List<float>(); }
            for (int i = 0; i < extraSampleOffsets.Count; i++)
            {
                GUILayout.BeginHorizontal();
                extraSampleOffsets[i] = EditorGUILayout.FloatField("Offset " + (i + 1), extraSampleOffsets[i]);
                if (GUILayout.Button("-"))
                {
                    extraSampleOffsets.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+"))
            {
                extraSampleOffsets.Add(0);
            }
            var margin = EditorGUILayout.FloatField("Error Margin", M2DC.OptimiseMargin);
            if (GUILayout.Button("Detach Collider"))
            {
                if (M2DC.Collider)
                {
                    Undo.RecordObject(M2DC.Collider.gameObject, "Detached PolygonCollider");
                    M2DC.Collider = null;
                    Destroy(M2DC);
                    if (Application.isPlaying)
                    {
                        Destroy(M2DC);
                    }
                    else
                    {
                        DestroyImmediate(M2DC);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed AutoPolygonCollider setting");
                M2DC.ReferenceMesh = refMesh;
                M2DC.ReferenceTerrain = refTerrain;
                M2DC.Collider = collider;
                M2DC.Depth = depth;
                M2DC.ExtraSampleOffsets = extraSampleOffsets.ToArray();
                M2DC.OptimiseMargin = margin;
                //M2DC.Extend = extend;
                M2DC.updatePosOnly = updatePosOnly;
            }
        }
        else if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed AutoPolygonCollider setting");
            M2DC.updatePosOnly = updatePosOnly;
        }

        if (update)
        {
            M2DC.UpdateCollider();
        }
    }
}
