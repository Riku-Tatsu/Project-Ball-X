using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPolygonClipper : MonoBehaviour {

	public PolygonCollider2D polygonParent;

	void Start ()
	{
		CombinePolygons();
	}
	
	// void Update ()
	// {
	// 	if(Input.GetKeyDown(KeyCode.E)) CombinePolygons();
	// }

	void CombinePolygons()
	{

		List<List<Vector2>> polyPoints = new List<List<Vector2>>();
		PolygonCollider2D[] polyCols = polygonParent.GetComponentsInChildren<PolygonCollider2D>();
		

		for (int i = 0; i < polyCols.Length; i++)
		{
			if(polyCols[i] == polygonParent) continue;

			List<Vector2> points = new List<Vector2>();
			//polyPoints.Add(polyCols[i].points);
			for (int j = 0; j < polyCols[i].points.Length; j++)
			{
				Vector3 transPoint = polyCols[i].transform.TransformPoint(new Vector3(polyCols[i].points[j].x, polyCols[i].points[j].y, 0));
				Vector2 worldPoint = new Vector2(transPoint.x, transPoint.y);// polyCols[i].points[j] + new Vector2(polyCols[i].transform.localPosition.x, polyCols[i].transform.localPosition.y);
				points.Add(worldPoint);
			}

			polyPoints.Add(points);
			polyCols[i].gameObject.SetActive(false);
		}

		//DrawListPoints(polyPoints, -3, Color.white, false);

		ClipperHelper clipper = new ClipperHelper();

		List<List<Vector2>> resultPolys = clipper.UniteCollisionPolygons(polyPoints);

		//DrawListPoints(resultPolys, -4, Color.green, true);


		//IMPORTANT: only set pathCount!!!
		polygonParent.pathCount = resultPolys.Count;

		for (int i = 0; i < resultPolys.Count; i++)
		{
			Vector2[] newPoints = resultPolys[i].ToArray();

			polygonParent.SetPath(i, newPoints);
		}
	}

	void DrawListPoints(List<List<Vector2>> list, float zOffset, Color color, bool randomColor)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Color newCol = randomColor ? new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f), 1) : color;
			for (int j = 0; j < list[i].Count - 1; j++)
			{
				
				Vector3 pointA = new Vector3(list[i][j].x, list[i][j].y, zOffset);
				Vector3 pointB = new Vector3(list[i][j+1].x, list[i][j+1].y, zOffset);

				Debug.DrawLine(pointA, pointB, newCol, 10, false);
			}
		}
	}

}
