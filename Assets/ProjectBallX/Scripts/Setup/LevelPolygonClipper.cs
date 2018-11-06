using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPolygonClipper : MonoBehaviour {

	public PolygonCollider2D polygonParent;

	

	// Use this for initialization
	void Start ()
	{


	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.E)) CombinePolygons();
	}

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
				Vector2 worldPoints = polyCols[i].points[j] + new Vector2(polyCols[i].transform.localPosition.x, polyCols[i].transform.localPosition.y);
				points.Add(worldPoints);
			}

			polyPoints.Add(points);
			polyCols[i].gameObject.SetActive(false);
		}

		ClipperHelper clipper = new ClipperHelper();

		List<List<Vector2>> resultPolys = clipper.UniteCollisionPolygons(polyPoints);

		polygonParent.points = new Vector2[resultPolys.Count];

		for (int i = 0; i < resultPolys.Count; i++)
		{
			Vector2[] newPoints = resultPolys[i].ToArray();

			polygonParent.SetPath(i, newPoints);
		}
	}

}
