using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ParticleManager : MonoBehaviour, IInputClickHandler {
	public GameObject Particles;

	private GazeManager gazeManager;

	// Use this for initialization
	void Start () {
		InputManager.Instance.PushFallbackInputHandler(gameObject);

		gazeManager = GazeManager.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
	{
		Particles.transform.position = gameObject.transform.position;
		Particles.transform.rotation = gameObject.transform.rotation;

		Particles.SetActive (true);
	}
}
