using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.BetterLit
{
   [ExecuteAlways]
   public class ShapeEffectorGroup : MonoBehaviour
   {
      public List<ShapeEffector> effectors = new List<ShapeEffector>();
      public List<Material> materials = new List<Material>();

      Matrix4x4[] mtxs = new Matrix4x4[8];
      Vector4[] data = new Vector4[8];



      private void Update()
      {
         int count = effectors.Count;
         
         if (count > 8)
         { 
            Debug.LogError("Maximum of 8 effectors per group, additional effectors ignored");
         }
         for (int i = 0; i < count; ++i)
         {
            mtxs[i] = effectors[i].transform.localToWorldMatrix;
            
            data[i].x = (int)effectors[i].shape;
            data[i].y = effectors[i].contrast;
         }

         foreach (var m in materials)
         {
            m.SetMatrixArray("_EffectorMtx", mtxs);
            m.SetVectorArray("_EffectorData", data);
            m.SetFloat("_EffectorCount", count);
         }
      }
   }
}
