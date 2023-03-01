using UnityEngine;

namespace JBooth.BetterLit
{
   public class ShapeEffector : MonoBehaviour
   {
      public enum Shape
      {
         Plane,
         Sphere
      }

      public Shape shape = Shape.Sphere;
      [Range(0.001f,1000)]
      public float contrast = 1;

      static Mesh sphere;
      static Mesh quad;

      private void OnDrawGizmosSelected()
      {
         if (sphere == null)
         {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere = go.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(go);
         }
         if (quad == null)
         {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad = go.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(go);
         }
         Gizmos.color = new Color(0, 1, 1, 0.3f);
         if (shape == Shape.Sphere)
         {
            Gizmos.DrawMesh(sphere, 0, transform.position, transform.rotation,
               new Vector3(transform.lossyScale.x, transform.lossyScale.x, transform.lossyScale.x));
         }
         else
         {
            Gizmos.DrawMesh(quad, 0, transform.position, transform.rotation, new Vector3(10,10,10));
            Gizmos.DrawMesh(quad, 0, transform.position, transform.rotation, new Vector3(-10, 10, 10));
         }
      }
   }
}
