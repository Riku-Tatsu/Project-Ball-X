using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_EdgeColliders : MonoBehaviour {

	EdgeCollider2D[] edgeCols;

	void OnDrawGizmos()
	{
		for (int i = 0; i < edgeCols.Length; i++)
		{
			for (int j = 0; j < edgeCols[i].pointCount - 1; j++)
			{
				if(j == 0) { Gizmos.color = Color.red; } else { Gizmos.color = Color.green; }
				//if(j < edgeCols[i].pointCount - 2)
				{
					Vector3 pointA = new Vector3(edgeCols[i].points[j].x, edgeCols[i].points[j].y, 0);
					Vector3 pointB = new Vector3(edgeCols[i].points[j+1].x, edgeCols[i].points[j+1].y, 0);

					pointA = edgeCols[i].transform.TransformPoint(pointA);
					pointB = edgeCols[i].transform.TransformPoint(pointB);
					
					Gizmos.DrawLine(pointA, pointB);
				}
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		edgeCols = FindObjectsOfType<EdgeCollider2D>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
