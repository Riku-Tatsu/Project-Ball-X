using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Mesh2DCollider : MonoBehaviour
{
    //public Transform ReferenceTransform;
    public bool isMeshBased;
    public Mesh ReferenceMesh;
    public TerrainData ReferenceTerrain;
    public Collider2D Collider;
    public List<Collider2D> ExtraColliders = new List<Collider2D>();
    public float Depth;
    public float[] ExtraSampleOffsets = new float[0];
    public float OptimiseMargin;
    //public bool Extend;
    public bool isEdge;
    public bool updatePosOnly;

    private bool isInitialized;

    [SerializeField] int instanceID = 0;
    void OnValidate()
    {
        if (!isInitialized)
        {
            if (ReferenceMesh || ReferenceTerrain)
            {
                isInitialized = true;
            }
            else  //in case the Mesh2DCollider is created by manually adding it to a gameobject
            {
                //Debug.Log("not initialized");
                var mf = GetComponent<MeshFilter>();
                var mc = GetComponent<MeshCollider>();
                if (mf)
                {
                    //CreateCollider(mf.sharedMesh, transform.position.z, null, false); //can't be done in OnValidate()
                    isMeshBased = true;
                    ReferenceMesh = mf.sharedMesh;
                    Depth = transform.position.z;
                }
                else if (mc)
                {
                    //CreateCollider(mc.sharedMesh, transform.position.z, null, false); //can't be done in OnValidate()
                    isMeshBased = true;
                    ReferenceMesh = mf.sharedMesh;
                    Depth = transform.position.z;
                }
                else
                {
                    var terrain = GetComponent<Terrain>();
                    if (terrain)
                    {
                        //CreateCollider(terrain.terrainData, transform.position.z, null); //can't be done in OnValidate()
                        isMeshBased = true;
                        ReferenceTerrain = terrain.terrainData;
                        Depth = transform.position.z;
                    }
                    else
                    {
                        isMeshBased = true; //default case
                        Debug.LogWarning("AutoPolygonCollider Warning: Missing Reference-Mesh for " + name);
                        isInitialized = true;
                    }
                }
            }
        }

        if (instanceID == 0)
        {
            instanceID = GetInstanceID();
            return;
        }

        if (instanceID != GetInstanceID() && GetInstanceID() < 0)
        {
            //Debug.Log("Detected Duplicate!");
            instanceID = GetInstanceID();
            if (Collider)
            {
                Collider = Instantiate(Collider.gameObject, Collider.transform.parent).GetComponent<Collider2D>();
                if (isEdge)
                {
                    Collider.name = name + " Edge Collider";
                }
                else
                {
                    Collider.name = name + " Polygon Collider";
                }
                if (ExtraColliders!=null && ExtraColliders.Count>0)
                {
                    for (int i = 0; i < ExtraColliders.Count; i++)
                    {
                        ExtraColliders[i] = Instantiate(ExtraColliders[i].gameObject, ExtraColliders[i].transform.parent).GetComponent<Collider2D>();
                        if (isEdge)
                        {
                            ExtraColliders[i].name = name + " Edge Collider " + (i);
                        }
                        else
                        {
                            ExtraColliders[i].name = name + " Polygon Collider " + (i);
                        }
                    }
                }
            }
        }
        //Debug.Log("instanceID: "+ instanceID);
    }

    void OnDestroy()
    {
        if (Collider)
        {
            if (Application.isPlaying)
            {
                Destroy(Collider.gameObject);
                for (int i = 0; i < ExtraColliders.Count; i++)
                {
                    if (ExtraColliders[i])
                    {
                        Destroy(ExtraColliders[i].gameObject);
                    }
                }
            }
            else
            {
                //UnityEditor.Undo.RecordObject(PolygonCollider.gameObject, PolygonCollider.name); //doesn't work
                DestroyImmediate(Collider.gameObject);
                for (int i = 0; i < ExtraColliders.Count; i++)
                {
                    if (ExtraColliders[i])
                    {
                        DestroyImmediate(ExtraColliders[i].gameObject);
                    }
                }
            }
        }
    }

    public void CreateCollider(Mesh mesh, float depth, float[] extraSampleOffsets, Transform parent, bool edge)
    {
        isMeshBased = true;
        ReferenceMesh = mesh;
        Depth = depth;
        ExtraSampleOffsets = extraSampleOffsets;
        isEdge = edge;

        UpdateCollider(parent);

        isInitialized = true;
    }
    public void CreateCollider(TerrainData terrain, float depth, float[] extraSampleOffsets, Transform parent)
    {
        isMeshBased = false;
        ReferenceTerrain = terrain;
        Depth = depth;
        ExtraSampleOffsets = extraSampleOffsets;
        isEdge = true;

        UpdateCollider(parent);

        isInitialized = true;
    }

    public void UpdateCollider(Transform parent=null)
    {
        if (isMeshBased && !ReferenceMesh)
        {
            Debug.LogWarning("AutoPolygonCollider Warning: Missing Reference-Mesh for " + name);
        }
        if (!isMeshBased && !ReferenceTerrain)
        {
            Debug.LogWarning("AutoPolygonCollider Warning: Missing Reference-Terrain for " + name);
        }
        else
        {
            bool update = !updatePosOnly;
            if (!Collider)
            {
                if (isEdge)
                {
                    Collider = new GameObject(name + " Edge Collider").AddComponent<EdgeCollider2D>();
                }
                else
                {
                    Collider = new GameObject(name + " Polygon Collider").AddComponent<PolygonCollider2D>();
                }
                Collider.transform.parent = parent;
                Collider.gameObject.layer = gameObject.layer;
                update = true;
            }

            var pos = transform.position;
            pos.z = Depth;
            Collider.transform.position = pos;

            pos = transform.position;
            transform.position = new Vector3(0, 0, pos.z);

            if (update)
            {
                if (ExtraColliders != null)
                {
                    for (int i = 0; i < ExtraColliders.Count; i++)
                    {
                        if (ExtraColliders[i])
                        {
                            DestroyImmediate(ExtraColliders[i].gameObject);
                        }
                    }
                }

                if (isMeshBased)
                {
                    ExtraColliders = new List<Collider2D>();
                    for (int i = 0; i < ExtraSampleOffsets.Length; i++)
                    {
                        if (isEdge)
                        {
                            ExtraColliders.Add(new GameObject(name + " Edge Collider " + (i)).AddComponent<EdgeCollider2D>());
                        }
                        else
                        {
                            ExtraColliders.Add(new GameObject(name + " Polygon Collider " + (i)).AddComponent<PolygonCollider2D>());
                        }
                        ExtraColliders[i].transform.parent = Collider.transform;
                        ExtraColliders[i].transform.localPosition = Vector3.zero;
                        ExtraColliders[i].gameObject.layer = gameObject.layer;
                    }

                    CreateCollidersForMesh();
                }
                else
                {
                    CreateColliderForTerrain();
                }
            }

            transform.position = pos;
        }
    }

    private void CreateColliderForTerrain() //because a Unity-terrain never has overhangs, the multiple samples can be easily combined in 1 edgecollider, so no need for multiple colliders here.
    {
        var points = new Vector2[ReferenceTerrain.heightmapResolution];

        float y = (Depth - transform.position.z) / ReferenceTerrain.size.z;
        float[] yValues = new float[ExtraSampleOffsets.Length];
        for (int j = 0; j < ExtraSampleOffsets.Length; j++)
        {
            yValues[j] = ((Depth + ExtraSampleOffsets[j]) - transform.position.z) / ReferenceTerrain.size.z;
        }

        for (int i = 0; i < points.Length; i++)
        {
            float x = (float)i / (float)(points.Length - 1);
            //points[i] = new Vector2(x * ReferenceTerrain.size.x, ReferenceTerrain.GetInterpolatedHeight(x, y));
            float y2 = ReferenceTerrain.GetInterpolatedHeight(x, y);
            for (int j = 0; j < ExtraSampleOffsets.Length; j++)
            {
                y2 = Mathf.Max(y2, ReferenceTerrain.GetInterpolatedHeight(x, yValues[j]));
            }
            points[i] = new Vector2(x * ReferenceTerrain.size.x, y2);
        }

        var slice = new List<Vector2>(points);
        slice = Optimise(slice, OptimiseMargin);

        (Collider as EdgeCollider2D).points = slice.ToArray();
    }

    private void CreateCollidersForTerrain() //similar approach as for the mesh colliders (unused)
    {
        for (int j = -1; j < ExtraSampleOffsets.Length; j++)
        {
            float offset = 0;
            if (j >= 0)
            {
                offset += ExtraSampleOffsets[j];
            }

            var points = new Vector2[ReferenceTerrain.heightmapResolution];
            float y = ((Depth + offset) - transform.position.z) / ReferenceTerrain.size.z;
            for (int i = 0; i < points.Length; i++)
            {
                float x = (float)i / (float)(points.Length - 1);
                points[i] = new Vector2(x * ReferenceTerrain.size.x, ReferenceTerrain.GetInterpolatedHeight(x, y));
            }

            var slice = new List<Vector2>(points);
            slice = Optimise(slice, OptimiseMargin);

            if (j < 0)
            {
                (Collider as EdgeCollider2D).points = slice.ToArray();
            }
            else
            {
                (ExtraColliders[j] as EdgeCollider2D).points = slice.ToArray();
            }
        }
    }

    private void CreateCollidersForMesh()
    {
        var vertices = ReferenceMesh.vertices;
        var triangles = ReferenceMesh.triangles;

        AutoWeld(ref vertices, ref triangles, (float)(ReferenceMesh.bounds.size.magnitude * 0.001));

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }

        //if (ExtraSampleOffsets != null)
        {
            for (int j = -1; j < ExtraSampleOffsets.Length; j++)
            {
                float offset = 0;
                if (j >= 0)
                {
                    offset += ExtraSampleOffsets[j];
                }
                var slice = GetSlice(vertices, triangles, Depth + offset);

                //this would work and would be much cleaner than creating multiple colliders, if Unity exposed the setting that makes multiple paths be combined additively instead of boolean:
                //if (!isEdge && ExtraSampleOffsets!=null)
                //{
                //    for (int i = 0; i < ExtraSampleOffsets.Length; i++)
                //    {
                //        slice.AddRange(GetSlice(vertices, triangles, Depth + ExtraSampleOffsets[i]));
                //    }
                //}

                if (slice.Count == 0 || slice[0].Count == 0)
                {
                    if (j >= 0)
                    {
                        DestroyImmediate(ExtraColliders[j].gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("Mesh2DCollider: no vertices found at current depth in " + name);
                    }
                }
                else
                {
                    RemoveDuplicates(slice);

                    Optimise(slice, OptimiseMargin);

                    //if (Extend)
                    //{
                    //    ExtendPaths(slice);
                    //}

                    if (isEdge)
                    {
                        if (j < 0)
                        {
                            (Collider as EdgeCollider2D).points = slice[0].ToArray();
                        }
                        else
                        {
                            (ExtraColliders[j] as EdgeCollider2D).points = slice[0].ToArray();
                        }
                    }
                    else
                    {
                        if (j < 0)
                        {
                            (Collider as PolygonCollider2D).pathCount = slice.Count;
                        }
                        else
                        {
                            (ExtraColliders[j] as PolygonCollider2D).pathCount = slice.Count;
                        }
                        for (int i = 0; i < slice.Count; i++)
                        {
                            if (j < 0)
                            {
                                (Collider as PolygonCollider2D).SetPath(i, slice[i].ToArray());
                            }
                            else
                            {
                                (ExtraColliders[j] as PolygonCollider2D).SetPath(i, slice[i].ToArray());
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < ExtraColliders.Count; i++)  //removing the empty ones
            {
                if (!ExtraColliders[i])
                {
                    ExtraColliders.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private static void AutoWeld(ref Vector3[] verts, ref int[] tris, float threshold)
    {
        float thresholdSqr = threshold * threshold;

        List<Vector3> newVerts = new List<Vector3>();

        int[] newIndices = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            for (int j = 0; j < newVerts.Count; j++)
            {
                if (Vector3.SqrMagnitude(newVerts[j] - verts[i]) <= thresholdSqr) //if (Vector3.Distance(newVert, vert) <= threshold)
                {
                    newIndices[i] = j;
                    goto skipToNext;
                }
            }

            newVerts.Add(verts[i]);
            newIndices[i] = newVerts.Count - 1;

            skipToNext:;
        }

        for (int i = 0; i < tris.Length; ++i)
        {
            tris[i] = newIndices[tris[i]];
        }

        //Remove edge-triangles (triangles with 2 vertices beeing equal)
        List<int> newtris = new List<int>();
        for (int i = 0; i < tris.Length; i += 3)
        {
            if (tris[i] != tris[i + 1] && tris[i] != tris[i + 2] && tris[i + 1] != tris[i + 2])
            {
                newtris.Add(tris[i]);
                newtris.Add(tris[i + 1]);
                newtris.Add(tris[i + 2]);
            }
        }

        verts = newVerts.ToArray();
        tris = newtris.ToArray();
    }

    private List<List<Vector2>> GetSlice(Vector3[] vertices, int[] triangles, float depth)
    {
        List<Vector3> slice = new List<Vector3>();
        List<Edge> edges = new List<Edge>();
        List<Edge> connections = new List<Edge>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            List<int> goodvertices = new List<int>();
            List<int> badvertices = new List<int>();
            List<bool> isgood = new List<bool>();
            for (int j = 0; j < 3; j++)
            {
                if (vertices[triangles[i + j]].z >= depth)
                {
                    if (j == 2 && isgood[0] && !isgood[1]) 
                    {
                        goodvertices.Add(goodvertices[0]);
                        goodvertices[0] = triangles[i + j];
                    }
                    else
                        goodvertices.Add(triangles[i + j]);
                    isgood.Add(true);
                }
                else
                {
                    if (j == 2 && !isgood[0] && isgood[1]) 
                    {
                        badvertices.Add(badvertices[0]);
                        badvertices[0] = triangles[i + j];
                    }
                    else
                        badvertices.Add(triangles[i + j]);
                    isgood.Add(false);
                }
            }
            if (goodvertices.Count == 2)
            {
                float good0Z = vertices[goodvertices[0]].z;
                float good1Z = vertices[goodvertices[1]].z; 
                float bad0Z = vertices[badvertices[0]].z; 
                float factor = (depth - bad0Z) / (good0Z - bad0Z);
                Vector3 newVert1 = Vector3.Lerp(vertices[badvertices[0]], vertices[goodvertices[0]], factor);
                factor = (depth - bad0Z) / (good1Z - bad0Z);
                Vector3 newVert2 = Vector3.Lerp(vertices[badvertices[0]], vertices[goodvertices[1]], factor);// * 0.8f + 0.1f);
                var newEdge1 = new Edge(goodvertices[0], badvertices[0]);
                var newEdge2 = new Edge(goodvertices[1], badvertices[0]);
                var foundEdgeIndex1 = newEdge1.FindMatchIn(edges);
                if (foundEdgeIndex1 < 0)
                {
                    slice.Add(newVert1);
                    edges.Add(newEdge1);
                }
                var foundEdgeIndex2 = newEdge2.FindMatchIn(edges);
                if (foundEdgeIndex2 < 0)
                {
                    slice.Add(newVert2);
                    edges.Add(newEdge2);
                }
                if (foundEdgeIndex1 < 0 && foundEdgeIndex2 >= 0)
                {
                    connections.Add(new Edge(foundEdgeIndex2, slice.Count - 1));
                }
                if (foundEdgeIndex1 >= 0 && foundEdgeIndex2 < 0)
                {
                    connections.Add(new Edge(foundEdgeIndex1, slice.Count - 1));
                }
                if (foundEdgeIndex1 >= 0 && foundEdgeIndex2 >= 0)
                {
                    connections.Add(new Edge(foundEdgeIndex1, foundEdgeIndex2));
                }
                if (foundEdgeIndex1 < 0 && foundEdgeIndex2 < 0)
                {
                    connections.Add(new Edge(slice.Count - 2, slice.Count - 1));
                }
            }
            else if (goodvertices.Count == 1)
            {
                float good0Z = vertices[goodvertices[0]].z; 
                float bad0Z = vertices[badvertices[0]].z;
                float bad1Z = vertices[badvertices[1]].z;
                float factor = (depth - bad0Z) / (good0Z - bad0Z);
                Vector3 newVert1 = Vector3.Lerp(vertices[badvertices[0]], vertices[goodvertices[0]], factor);
                factor = (depth - bad1Z) / (good0Z - bad1Z);
                Vector3 newVert2 = Vector3.Lerp(vertices[badvertices[1]], vertices[goodvertices[0]], factor);// * 0.8f + 0.1f);
                var newEdge1 = new Edge(goodvertices[0], badvertices[0]);
                var newEdge2 = new Edge(goodvertices[0], badvertices[1]);
                var foundEdgeIndex1 = newEdge1.FindMatchIn(edges);
                if (foundEdgeIndex1 < 0)
                {
                    slice.Add(newVert1);
                    edges.Add(newEdge1);
                }
                var foundEdgeIndex2 = newEdge2.FindMatchIn(edges);
                if (foundEdgeIndex2 < 0)
                {
                    slice.Add(newVert2);
                    edges.Add(newEdge2);
                }
                if (foundEdgeIndex1 < 0 && foundEdgeIndex2 >= 0)
                {
                    connections.Add(new Edge(foundEdgeIndex2, slice.Count - 1));
                }
                if (foundEdgeIndex1 >= 0 && foundEdgeIndex2 < 0)
                {
                    connections.Add(new Edge(foundEdgeIndex1, slice.Count - 1));
                }
                if (foundEdgeIndex1 >= 0 && foundEdgeIndex2 >= 0)
                {
                    connections.Add(new Edge(foundEdgeIndex1, foundEdgeIndex2));
                }
                if (foundEdgeIndex1 < 0 && foundEdgeIndex2 < 0)
                {
                    connections.Add(new Edge(slice.Count - 2, slice.Count - 1));
                }
            }
        }

        //RemoveDuplicates(slice);

        //slice = Sort(slice);

        return Sort(slice, connections);
    }

    private List<List<Vector2>> Sort(List<Vector3> slice, List<Edge> connections)
    {
        var done = new List<bool>();
        for (int i = 0; i < slice.Count; i++)
        {
            done.Add(false);
        }

        var sorted = new List<List<Vector2>>();

        while (connections.Count > 0)
        {
            var currentList = new List<Vector2>();
            var currentListindices = new List<int>();
            int current = connections[0].a;
            currentList.Add(slice[current]);
            currentListindices.Add(current);
            int next = connections[0].b;
            bool loop = false;
            while (next >= 0)
            {
                if (currentListindices.Contains(next))
                {
                    loop = true;
                    break;
                }
                currentList.Add(slice[next]);
                currentListindices.Add(next);
                var previous = current;
                current = next;
                next = Edge.GetNext(current, connections, previous);//, out index);
            }
            if (!loop) //then we go back to the start but the other way
            {
                current = connections[0].a;
                next = Edge.GetNext(current, connections, connections[0].b);
                while (next >= 0)
                {
                    currentList.Insert(0, slice[next]);
                    currentListindices.Insert(0, next);
                    var previous = current;
                    current = next;
                    next = Edge.GetNext(current, connections, previous);//, out index);
                }
            }

            sorted.Add(currentList);

            for (int i = 0; i < currentListindices.Count; i++)
            {
                for (int j = 0; j < connections.Count; j++)
                {
                    if (connections[j].a == currentListindices[i] || connections[j].b == currentListindices[i])
                    {
                        connections.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        return sorted;
    }

    private void RemoveDuplicates(List<List<Vector2>> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 1; j < points[i].Count; )
            {
                if (points[i][j] == points[i][j-1])
                {
                    points[i].RemoveAt(j);
                }
                else
                {
                    j++;
                }
            }
        }
    }

    public struct Edge
    {
        public int a, b;

        public Edge(int v1, int v2)
        {
            a = v1;
            b = v2;
        }

        public Edge getReverse()
        {
            return new Edge(a, b);
        }

        public int FindMatchIn(List<Edge> edges)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].IsEqualTo(this))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsEqualTo(Edge edge)
        {
            return (edge.a == a && edge.b == b) ||
                    (edge.a == b && edge.b == a);
        }

        public static int GetNext(int current, List<Edge> edges, int exception)//, out int index)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].a == current && edges[i].b != exception)
                {
                    //index = i;
                    return edges[i].b;
                }
                if (edges[i].b == current && edges[i].a != exception)
                {
                    //index = i;
                    return edges[i].a;
                }
            }
            //index = -1;
            return -1;
        }
    }

    private void ExtendPaths(List<List<Vector2>> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            var points = paths[i];
            float maxDistance = 0;
            for (int j = 1; j < points.Count-1; j++)
            {
                var distance = PointLineDistance(points[j], points[0], points[points.Count - 1]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            if (maxDistance > 0)
            {
                var offset = points[points.Count - 1] - points[0];
                var x = offset.x;
                offset.x = offset.y;
                offset.y = x;
                offset = -offset.normalized * (maxDistance + 0.5f);

                var start = points[0] + offset;
                var end = points[points.Count - 1] + offset;

                points.Insert(0, start);
                points.Add(end);
            }
        }
    }

    private void Optimise(List<List<Vector2>> paths, float margin)
    {
        if (margin <= 0)
        {
            return;
        }

        for (int i = 0; i < paths.Count; i++)
        {
            var points = paths[i];
            if (points.Count > 3)
            {
                var newPoints = DPReduction(points, 0, points.Count - 1, margin);
                newPoints.Insert(0, points[0]);
                newPoints.Add(points[points.Count - 1]);
                paths[i] = newPoints;
            }
        }
    }
    private List<Vector2> Optimise(List<Vector2> points, float margin)
    {
        if (margin <= 0)
        {
            return points;
        }

        if (points.Count > 3)
        {
            var newPoints = DPReduction(points, 0, points.Count - 1, margin);
            newPoints.Insert(0, points[0]);
            newPoints.Add(points[points.Count - 1]);
            points = newPoints;
        }
        return points;
    }

    private List<Vector2> DPReduction(List<Vector2> points, int start, int end, float margin)
    {
        var newPoints = new List<Vector2>();
        if (end - start < 2)
        {
            return newPoints;
        }
        float maxDistance = -1;
        int maxIndex = -1;
        for (int j = start + 1; j < end; j++)
        {
            var distance = Mathf.Abs(PointLineDistance(points[j], points[j - 1], points[j + 1]));
            if (j == 1 || distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = j;
            }
        }
        if (maxDistance > margin)
        {
            newPoints.AddRange(DPReduction(points, start, maxIndex, margin));
            newPoints.Add(points[maxIndex]);
            newPoints.AddRange(DPReduction(points, maxIndex, end, margin));
        }
        return newPoints;
    }

    private float PointLineDistance(Vector3 P, Vector3 L1, Vector3 L2)
    {
        return (float)(((L2.y - L1.y) * P.x - (L2.x - L1.x) * P.y + L2.x * L1.y - L2.y * L1.x) / Math.Sqrt((L2.y - L1.y) * (L2.y - L1.y) + (L2.x - L1.x) * (L2.x - L1.x)));
    }
}
