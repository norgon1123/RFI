using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDistance : MonoBehaviour {
    public TextMesh Display;

    private Vector3 cameraPos;
    private Vector3 gazePos;
    private float distance;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        //cameraPos = Camera.main.transform.position;
        //gazePos = GazeManager.Instance.HitPosition;

        //distance = Vector3.Distance(cameraPos, gazePos);
        //Display.text = distance.ToString();

        Display.text = "Active Graphs\n" + AstarPath.active.graphs.Length.ToString();
	}
}
