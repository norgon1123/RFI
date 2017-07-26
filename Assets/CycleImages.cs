using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleImages : MonoBehaviour, IInputClickHandler
{
    public Material[] images;

    private int count;
    private new Renderer renderer;

    // Use this for initialization
    void Start () {
        count = 0;
        renderer = gameObject.GetComponent<Renderer>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnInputClicked(InputClickedEventData eventData)
    {
        count++;
        renderer.material = images[count];
        if (count == 4)
        {
            count = -1;
        }
    }
}
