using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

public class BodyCollision : MonoBehaviour {
	public float Distance = 1;
	public GameObject Cube;
	public static BodyCollision Instance;
	public Transform CursorTransform;
	public float Radius = 0.5f;

	/// <summary>
	/// Blend value for surface normal to user facing lerp
	/// </summary>
	public float PositionLerpTime = 0.01f;

	/// <summary>
	/// Blend value for surface normal to user facing lerp
	/// </summary>
	public float RotationLerpTime = 0.01f;

	private Vector3 position;
	private RaycastHit hit;
	private Vector3 direction;
	private Vector3 bottomCap;
	private float floorYPos;
	private bool detecting;

	public float FloorYPos {
		get {
			return floorYPos;
		}
		set {
			floorYPos = value - Radius;
		}
	}

	public bool Detecting {
		get {
			return detecting;
		}
		set {
			detecting = value;
		}
	}

	// Use this for initialization
	void Start () {
		detecting = true;

		if (!Instance)
		{
			Instance = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (detecting)
		{
			direction = Camera.main.transform.forward;
			position = Camera.main.transform.position;
			bottomCap = new Vector3(position.x, floorYPos, position.z);

			if (Physics.CapsuleCast (position, bottomCap, Radius, direction, out hit, Distance))
			{
				Vector3 cubePos = direction + position;
				cubePos.y -= .1f;

				// Use the lerp times to blend the position to the target position
				Cube.transform.position = Vector3.Lerp(Cube.transform.position, cubePos, Time.deltaTime / PositionLerpTime);
				Cube.transform.rotation = Quaternion.Lerp(Cube.transform.rotation, Camera.main.transform.rotation, Time.deltaTime / RotationLerpTime);
				Cube.gameObject.SetActive (true);
			}
			else
			{
				Cube.gameObject.SetActive (false);
			}
		}
	}

	public void StopDetection()
	{
		detecting = false;
		Cube.gameObject.SetActive (false);
	}

	public void StartDetection()
	{
		detecting = true;
	}
}
