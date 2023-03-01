//////////////////////////////////////////////////////
// Barycentric mesh processor
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.BetterLit
{
   // Stuffs tessellation dampening into UV0.z as 0-1
   // Stuffs barycentric data into UV0.w as packed colors
   public class BetterLitMeshProcessor : EditorWindow
   {
      [MenuItem("Window/Better Lit Shader/Mesh Processor")]
      public static void ShowWindow()
      {
         var window = GetWindow<BetterLitMeshProcessor>();
         window.titleContent = new GUIContent("Better Lit Mesh Processor"); 
         window.Show();
      }
      public GameObject obj;
      public bool barycentrics = false;
      public bool tessdampening = false;
      public float weldThreshold = 0.001f;

      public void OnGUI()
      {
         obj = (GameObject)EditorGUILayout.ObjectField("Game Object", obj, typeof(GameObject), false);
         EditorGUILayout.HelpBox("Barycentrics can increase the vertex count, so only use if you plan to use wireframe rendering", MessageType.Info);
         barycentrics = EditorGUILayout.Toggle("Barycentrics (wireframe)", barycentrics);
         tessdampening = EditorGUILayout.Toggle("Tessellation Dampening", tessdampening);
         if (tessdampening)
         {
            EditorGUI.indentLevel++;
            weldThreshold = EditorGUILayout.Slider("Weld Threshold", weldThreshold, 0.00001f, 0.1f);
            EditorGUI.indentLevel--;
         }
         if (GUILayout.Button("Process"))
         {
            if (barycentrics == false && tessdampening == false)
            {
               Debug.LogError("No data selected to add to the mesh, skipping");
               return;
            }
            string path = "";
            var assets = new List<Mesh>();
            if (obj != null)
            {
               path = AssetDatabase.GetAssetPath(obj);
               var mfs = obj.GetComponentsInChildren<MeshFilter>();
               for (int i = 0; i < mfs.Length; ++i)
               {
                  Mesh m = mfs[i].sharedMesh;
                  if (m != null)
                  {
                     Mesh result = Process(m);
                     if (result != null)
                        assets.Add(result);
                  }
               }
            }

            if (assets.Count > 0)
            {

               path = path.Substring(0, path.IndexOf("."));
               path += "_betterlit.asset";

               AssetDatabase.CreateAsset(assets[0], path);
               for (int i = 1; i < assets.Count; ++i)
               {
                  AssetDatabase.AddObjectToAsset(assets[i], path);
               }
               AssetDatabase.SaveAssets();
            }
         }
      }

      static float EncodeToFloat (Color enc)
      {
         var ex = (uint)(enc.r * 255);
         var ey = (uint)(enc.g * 255);
         var ez = (uint)(enc.b * 255);
         var ew = (uint)(enc.a * 255);
         var v = (ex << 24) + (ey << 16) + (ez << 8) + ew;
         return v / (256.0f * 256.0f * 256.0f * 256.0f);
      }

      public Mesh Process(Mesh mesh)
      {
         // cache data for speed
         List<Vector3> positions = new List<Vector3>(mesh.vertices);
         List<Vector2> uv0 = new List<Vector2>(positions.Count);
         List<Vector4> uv1 = new List<Vector4>(positions.Count);
         List<Vector4> uv2 = new List<Vector4>(positions.Count);
         List<Vector4> uv3 = new List<Vector4>(positions.Count);
         List<Color> colors = new List<Color>(positions.Count);
         List<BoneWeight> boneWeights = new List<BoneWeight>(positions.Count);
         int[] triangles = mesh.triangles;
         mesh.GetUVs(0, uv0);
         mesh.GetUVs(1, uv1);
         mesh.GetUVs(2, uv2);
         mesh.GetUVs(3, uv3);
         mesh.GetColors(colors);
         mesh.GetBoneWeights(boneWeights);
         if (uv0 == null || uv0.Count == 0)
         {
            uv0 = new List<Vector2>(positions.Count);
            for (int i = 0; i < positions.Count; ++i)
            {
               uv0.Add(Vector2.zero);
            }
         }
         List<Vector4> packedData = new List<Vector4>(uv0.Count);
         var normals = new List<Vector3>(mesh.normals);
         var tangents = new List<Vector4>(mesh.tangents);
         var faces = new List<int>(mesh.triangles);

         if (barycentrics)
         {
            var barys = new List<Color>(mesh.uv.Length);

            for (int i = 0; i < positions.Count; ++i)
               barys.Add(Color.black);

            Color[] markings = new Color[3] { Color.red, Color.green, Color.blue };
            bool[] state = new bool[3];
            // go through data structure and mark colors, adding new splits when necissary
            int tcount = faces.Count;
            for (int i = 0; i < tcount; i = i + 3)
            {
               state[0] = false;
               state[1] = false;
               state[2] = false;

               int[] fIdx = new int[] { faces[i], faces[i + 1], faces[i + 2] };

               // mark currently used colors in face
               for (int x = 0; x < 3; ++x)
               {
                  int index = fIdx[x];
                  Color c = barys[index];
                  if (c == Color.red)
                  {
                     state[0] = true;
                  }
                  else if (c == Color.green)
                  {
                     state[1] = true;
                  }
                  else if (c == Color.blue)
                  {
                     state[2] = true;
                  }
               }
               for (int x = 0; x < 3; ++x)
               {
                  int index = fIdx[x];
                  Color c = barys[index];
                  if (c == Color.black)
                  {
                     for (int y = 0; y < 3; ++y)
                     {
                        if (state[y] == false)
                        {
                           state[y] = true;
                           barys[index] = markings[y];
                           break;
                        }
                     }
                  }
               }
               for (int x = 0; x < 3; ++x)
               {
                  int index = fIdx[x];
                  Color c = barys[index];
                  Color c0 = barys[fIdx[0]];
                  Color c1 = barys[fIdx[1]];
                  Color c2 = barys[fIdx[2]];

                  // out of colors? make a new index and map this triangle to use it
                  if (c == Color.black ||
                        ((x == 0 && (c == c1 || c == c2)) ||
                        (x == 1 && (c == c0 || c == c2)) ||
                        (x == 2 && (c == c0 || c == c1))))
                  {
                     int origLen = positions.Count;
                     int newIdx = positions.Count;
                     positions.Add(positions[index]);
                     faces[i + x] = newIdx;

                     if (normals != null && normals.Count == origLen)
                     {
                        normals.Add(normals[index]);
                     }
                     if (tangents != null && tangents.Count == origLen)
                     {
                        tangents.Add(tangents[index]);
                     }
                     if (uv0 != null && uv0.Count == origLen)
                     {
                        uv0.Add(uv0[index]);
                     }
                     if (uv1 != null && uv1.Count == origLen)
                     {
                        uv1.Add(uv1[index]);
                     }
                     if (uv2 != null && uv2.Count == origLen)
                     {
                        uv2.Add(uv2[index]);
                     }
                     if (uv3 != null && uv3.Count == origLen)
                     {
                        uv3.Add(uv3[index]);
                     }
                     if (colors != null && colors.Count == origLen)
                     {
                        colors.Add(colors[index]);
                     }
                     if (boneWeights != null && boneWeights.Count == origLen)
                     {
                        boneWeights.Add(boneWeights[index]);
                     }

                     // figure out which color we can use
                     // add so we get something like 1, 1, 0

                     Color cc = Color.red;
                     if (c0 == cc || c1 == cc || c2 == cc)
                     {
                        cc = Color.green;
                        if (c0 == cc || c1 == cc || c2 == cc)
                           cc = Color.blue;
                     }

                     fIdx[x] = newIdx;
                     barys.Add(cc);
                  }


               }
            }
            
            for (int i = 0; i < barys.Count; ++i)
            {
               Color toEncode = barys[i];
               toEncode.a = 0;
               float encoded = EncodeToFloat(toEncode);
               packedData.Add(new Vector4(uv0[i].x, uv0[i].y, 0, encoded));
            }
         }
         else
         {
            for (int i = 0; i < uv0.Count; ++i)
            {
               packedData.Add(new Vector4(uv0[i].x, uv0[i].y, 0, 0));
            }
         }


         if (tessdampening)
         {
            // now do tess dampening
            List<int> search = new List<int>();
            for (int x = 0; x < positions.Count; ++x)
            {
               Vector3 orig = positions[x];
               Vector2 uv = uv0[x];
               float dampen = 0;
               int count = 1;
               // index, count
               for (int y = 0; y < positions.Count; ++y)
               {
                  Vector3 comp = positions[y];
                  if (x != y && (Vector3.Distance(orig, comp) < weldThreshold))
                  {
                     count++;
                     if (Vector2.Distance(uv, uv0[y]) > weldThreshold)
                     {
                        dampen = 1;
                     }
                  }
               }
               if (count > 4 && dampen > 0)
               {
                  search.Add(x);
               }
               packedData[x] = new Vector4(packedData[x].x, packedData[x].y, dampen, packedData[x].w);
            }
            foreach (var index in search)
            {
               for (int t = 0; t < triangles.Length; t = t + 3)
               {
                  int idx0 = triangles[t];
                  int idx1 = triangles[t + 1];
                  int idx2 = triangles[t + 2];

                  if (idx0 == index || idx1 == index || idx2 == index)
                  {
                     packedData[idx0] = new Vector4(packedData[idx0].x, packedData[idx0].y, 1, packedData[idx0].w);
                     packedData[idx1] = new Vector4(packedData[idx1].x, packedData[idx1].y, 1, packedData[idx1].w);
                     packedData[idx2] = new Vector4(packedData[idx2].x, packedData[idx2].y, 1, packedData[idx2].w);
                  }
               }
            }
         }

         Mesh m = new Mesh();
         
         m.Clear();
         m.indexFormat = mesh.indexFormat;
         m.vertices = positions.ToArray();
         
         m.SetUVs(0, packedData);
         m.bindposes = mesh.bindposes;
         m.bounds = mesh.bounds;
         if (boneWeights != null && boneWeights.Count > 0)
         {
            m.boneWeights = boneWeights.ToArray();
         }
         if (colors != null && colors.Count > 0)
         {
            m.colors = colors.ToArray();
         }
         if (uv1 != null && uv1.Count > 0)
         {
            m.SetUVs(1, uv1);
         }
         if (uv2 != null && uv2.Count > 0)
         {
            m.SetUVs (2, uv2);
         }
         if (uv3 != null && uv3.Count > 0)
         {
            m.SetUVs(3, uv1);
         }
         m.triangles = faces.ToArray();
         m.normals = normals.ToArray();
         m.tangents = tangents.ToArray();

         m.name = mesh.name;
         m.RecalculateBounds();
         m.UploadMeshData(false);
         return m;

      }
   }
}