using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {
	public Material GazeMaterial;
	public Material NormalMaterial;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().material = NormalMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GazeEntered()
	{
		GetComponent<Renderer> ().material = GazeMaterial;
		transform.localScale += new Vector3 (0.03f, 0.03f, 0.03f);
		GetComponentInChildren<TextMesh> ().transform.localScale += new Vector3 (0.03f, 0.03f, 0.03f);
	}

	public void GazeExited()
	{
		GetComponent<Renderer> ().material =  NormalMaterial;
		transform.localScale -= new Vector3 (0.03f, 0.03f, 0.03f);
		GetComponentInChildren<TextMesh> ().transform.localScale -= new Vector3 (0.03f, 0.03f, 0.03f);
	
	}
}
