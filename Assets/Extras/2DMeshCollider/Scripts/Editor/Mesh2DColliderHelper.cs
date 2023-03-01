using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Mesh2DColliderHelper : EditorWindow
{
    [SerializeField]
    float Depth = 0;
    List<float> ExtraSampleOffsets = new List<float>();
    bool foldout;
    [SerializeField]
    bool onlyWhenSelected = true;
    [SerializeField]
    bool useContainer;
    [SerializeField]
    Transform Container;
    [SerializeField]
    bool EdgeCollider;

    private enum ColliderType { polygon, edge};

    [MenuItem("Window/2D Mesh Collider Helper")]
    static void Init()
    {
        Mesh2DColliderHelper window = (Mesh2DColliderHelper)GetWindow(typeof(Mesh2DColliderHelper), false, "2D Mesh Collider Helper");
        window.Show();
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        if (ExtraSampleOffsets == null)
        {
            ExtraSampleOffsets = new List<float>();
        }

        GUILayout.BeginHorizontal();
        var depth = EditorGUILayout.FloatField("Z-Depth", Depth);
        if (GUILayout.Button("Set for all"))
        {
            var autoPolygonColliders = FindObjectsOfType<Mesh2DCollider>();
            for (int i = 0; i < autoPolygonColliders.Length; i++)
            {
                autoPolygonColliders[i].ExtraSampleOffsets = ExtraSampleOffsets.ToArray();
                autoPolygonColliders[i].UpdateCollider();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        var extraSampleOffsets = new List<float>(ExtraSampleOffsets);
        foldout = EditorGUILayout.Foldout(foldout, "Extra Sample Offsets");
        if (GUILayout.Button("Set for all"))
        {
            var autoPolygonColliders = FindObjectsOfType<Mesh2DCollider>();
            for (int i = 0; i < autoPolygonColliders.Length; i++)
            {
                autoPolygonColliders[i].ExtraSampleOffsets = ExtraSampleOffsets.ToArray();
                autoPolygonColliders[i].UpdateCollider();
            }
        }
        GUILayout.EndHorizontal();
        if (foldout)
        {
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
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();
        bool placeInContainer = EditorGUILayout.Toggle("Place In Container", useContainer);
        var containerTransform = Container;
        if (useContainer)
        {
            containerTransform = EditorGUILayout.ObjectField("", Container, typeof(Transform), true) as Transform;
            if (!containerTransform)
            {
                var go = GameObject.Find("2DMeshColliders");
                if (go)
                {
                    containerTransform = go.transform;
                }
                else
                {
                    containerTransform = new GameObject("2DMeshColliders").transform;
                }
            }
        }
        GUILayout.EndHorizontal();

        var onlySelected = EditorGUILayout.Toggle("Only selected objects", onlyWhenSelected);

        ColliderType type = EdgeCollider ? ColliderType.edge : ColliderType.polygon;
        type = (ColliderType)EditorGUILayout.EnumPopup("Collider type", type);


        if (EditorGUI.EndChangeCheck() || containerTransform!= Container)
        {
            Undo.RecordObject(this, "Changed Mesh2DColliderHelper setting");
            Depth = depth;
            ExtraSampleOffsets = extraSampleOffsets;
            useContainer = placeInContainer;
            Container = containerTransform;
            onlyWhenSelected = onlySelected;
            EdgeCollider = type == ColliderType.edge;
        }

        string collidertype = " PolygonColliders";
        if (EdgeCollider) { collidertype = " EdgeColliders"; }
        string selected = "";
        if (onlySelected)
        {
            selected = " selected";
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Convert all" + selected + " MeshColliders"))
        {
            Transform parent = null;
            if (useContainer)
            {
                parent = Container;
            }
            MeshCollider[] meshColliders;
            if (onlyWhenSelected)
            {
                meshColliders = Selection.GetFiltered<MeshCollider>(SelectionMode.Deep);
            }
            else
            {
                meshColliders = FindObjectsOfType<MeshCollider>();
            }
            int count = 0;
            for (int i = 0; i < meshColliders.Length; i++)
            {                
                var m2dc = meshColliders[i].gameObject.AddComponent<Mesh2DCollider>();
                m2dc.CreateCollider(meshColliders[i].sharedMesh, depth, ExtraSampleOffsets.ToArray(), parent, EdgeCollider);
                DestroyImmediate(meshColliders[i]);
                count++;
            }

            Debug.Log("Converted " + count + " MeshColliders to" + collidertype);
        }

        if (GUILayout.Button("Create for all" + selected + " MeshFilters"))
        {
            Transform parent = null;
            if (useContainer)
            {
                parent = Container;
            }
            MeshFilter[] meshFilters;
            if (onlyWhenSelected)
            {
                meshFilters = Selection.GetFiltered<MeshFilter>(SelectionMode.Deep);
            }
            else
            {
                meshFilters = FindObjectsOfType<MeshFilter>();
            }
            int count = 0;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                var existingAPC = meshFilters[i].GetComponent<Mesh2DCollider>();
                if (!existingAPC)
                {
                    var m2dc = meshFilters[i].gameObject.AddComponent<Mesh2DCollider>();
                    m2dc.CreateCollider(meshFilters[i].sharedMesh, depth, ExtraSampleOffsets.ToArray(), parent, EdgeCollider);
                    count++;
                }
            }

            Debug.Log("Created " + count + collidertype);
        }

        if (GUILayout.Button("Create for all" + selected + " Terrains"))
        {
            Transform parent = null;
            if (useContainer)
            {
                parent = Container;
            }
            Terrain[] terrains;
            if (onlyWhenSelected)
            {
                terrains = Selection.GetFiltered<Terrain>(SelectionMode.Deep);
            }
            else
            {
                terrains = FindObjectsOfType<Terrain>();
            }
            int count = 0;
            for (int i = 0; i < terrains.Length; i++)
            {
                var existingAPC = terrains[i].GetComponent<Mesh2DCollider>();
                if (!existingAPC)
                {
                    var m2dc = terrains[i].gameObject.AddComponent<Mesh2DCollider>();
                    m2dc.CreateCollider(terrains[i].terrainData, depth, ExtraSampleOffsets.ToArray(), parent);
                    count++;
                }
            }

            Debug.Log("Created " + count + collidertype);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Update all Mesh2DColliders"))
        {
            var autoPolygonColliders = FindObjectsOfType<Mesh2DCollider>();
            for (int i = 0; i < autoPolygonColliders.Length; i++)
            {
                autoPolygonColliders[i].UpdateCollider();
            }
        }
    }
}
