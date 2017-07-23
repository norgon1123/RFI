using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ColorSelector : MonoBehaviour, IInputClickHandler{
	public List<Material> ColorChoices;
	public static ColorSelector Instance;

	private int materialInd;
	private List<GameObject> sphereList = new List<GameObject>();
	private IEnumerator colorSet;
	private float colorR;
	private float colorG;
	private float colorB;
	private bool gazing;

	// Use this for initialization
	void Start () {
		materialInd = 0;
		colorR = 0;
		colorG = 0;
		colorB = 0;
		gazing = false;


		if (Instance == null)
		{
			Instance = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		
	public void OnInputClicked(InputClickedEventData eventData)
	{
		if (materialInd == ColorChoices.Count - 1) {
			materialInd = 0;
		} else {
			materialInd++;
		}
		gameObject.GetComponent<Renderer> ().material = ColorChoices [materialInd];
	}

	public void CreateSphere()
	{
		Vector3 sphereLocation = GazeManager.Instance.HitPosition;
		sphereLocation.y += 0.2f;
		GameObject _sphere = Instantiate(gameObject, sphereLocation, gameObject.transform.rotation);
		_sphere.gameObject.SetActive (true);

		sphereList.Add (_sphere);
	}


	public void DeteleSpheres()
	{
		foreach (GameObject s in sphereList)
		{
			Destroy (s);
		}

		sphereList.Clear ();
	}

	public void GazeEntered()
	{
		Debug.Log ("gazing");
		gazing = true;
		CollisionDetection.Instance.StopDetection ();
	}

	public void GazeExited()
	{
		Debug.Log ("not gazing");
		gazing = false;
		CollisionDetection.Instance.StartDetection ();
	}

	public void ColorSet(float r, float g, float b, int selector)
	{
		if (!gazing) {return;}

		colorR = GetComponent<Renderer> ().material.color.r;
		colorG = GetComponent<Renderer> ().material.color.g;
		colorB = GetComponent<Renderer> ().material.color.b;

		switch (selector)
		{
		case 1:
			//r *= 255;
			GetComponent<Renderer>().material.color = new Color (r, colorG, colorB, 1);
			break;
		case 2:
			//g *= 255;
			GetComponent<Renderer>().material.color = new Color (colorR, g, colorB, 1);
			break;
		default:
			//b *= 255;
			GetComponent<Renderer>().material.color = new Color (colorR, colorG, b, 1);
			break;
		}

		Debug.Log ("r: " + GetComponent<Renderer> ().material.color.r +
		" g: " + GetComponent<Renderer> ().material.color.g +
		" b: " + GetComponent<Renderer> ().material.color.b);


	}
}
