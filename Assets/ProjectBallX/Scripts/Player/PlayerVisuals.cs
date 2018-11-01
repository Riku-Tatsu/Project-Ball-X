using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour {

	public Rigidbody2D targetBody;
	public Transform model;
	public Light light;
	public float maxVisualRpm = 20.0f;
	public float maxLightRpm = 30.0f;


	MeshRenderer renderer;
	MaterialPropertyBlock matProps;
	Color baseColor;
	Color speedColor;
	float rotation = 0;

	// Use this for initialization
	void Start ()
	{
		renderer = model.GetComponent<MeshRenderer>();
		matProps = new MaterialPropertyBlock();

		baseColor = renderer.sharedMaterial.GetColor("_EmisInsideBase");
		speedColor = renderer.sharedMaterial.GetColor("_EmisInsideGlow");
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		rotation += Mathf.Clamp(targetBody.angularVelocity, -maxVisualRpm * 60, maxVisualRpm * 60) * Time.deltaTime;

		if(model)
		{
			model.localRotation = Quaternion.Euler(0,0,rotation);
			model.position = targetBody.transform.position;
		}

		float glowLerp = Mathf.Clamp01(Mathf.Abs(targetBody.angularVelocity) / (maxVisualRpm * 60));
		matProps.SetFloat("_GlowLerp",  glowLerp);
		renderer.SetPropertyBlock(matProps);

		light.color = Color.Lerp(baseColor, speedColor, glowLerp);
	}
}
