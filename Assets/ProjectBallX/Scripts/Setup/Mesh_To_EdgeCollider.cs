using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class Mesh_To_EdgeCollider : MonoBehaviour {

	public Mesh sourceMesh;
	public EdgeCollider2D edgeCollider;

	public List<Vector2> points = new List<Vector2>();

	[ContextMenu("Set Collider Points")]
	void SetCollider()
	{
		if(points.Count < 2)
		{
			Debug.Log("Needs a minimum of 2 points");
			return;
		}


		//Vector2[] newPoints = new Vector2[sourceMesh.vertexCount];
		// for (int i = 0; i < newPoints.Length; i++)
		// {
		// 	newPoints[i] = new Vector2(sourceMesh.vertices[i].x, sourceMesh.vertices[i].y); 
		// }
		// edgeCollider.points = newPoints;

		Vector2[] newPoints = new Vector2[points.Count];
		for (int i = 0; i < points.Count; i++)
		{
			newPoints[i] = points[i];
		}

		edgeCollider.points = newPoints;

		//Debug.Log("Set points for " + edgeCollider.name);
	}

	[ContextMenu("Populate Point List")]
	void MeshToList()
	{
		points.Clear();
		for (int i = 0; i < sourceMesh.vertexCount; i++)
		{
			points.Add( new Vector2(sourceMesh.vertices[i].x, sourceMesh.vertices[i].y) ); 
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		for (int i = 0; i < points.Count - 1; i++)
		{
			Vector3 pointA = new Vector3(points[i].x, points[i].y, 0);
			Vector3 pointB = new Vector3(points[i+1].x, points[i+1].y, 0);

			transform.TransformPoint(pointA);
			transform.TransformPoint(pointB);
			Gizmos.DrawLine(pointA, pointB);
		}
	}
}
