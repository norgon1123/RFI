using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenuManager : MonoBehaviour {
    /// <summary>
    /// Create the three menu panels and set them 
    /// in front of the user
    /// </summary>
    public void CreateMenu()
    {
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.75f;
        transform.Translate(.45f, 0f, 0f, Camera.main.transform);
        transform.rotation = Camera.main.transform.rotation;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Remove the three menu panels from the world
    /// </summary>
    public void DeleteMenu()
    {
        gameObject.SetActive(false);
    }
	
}
