using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeName : MonoBehaviour, IInputClickHandler
{
    public string OriginalName = "RFI";
    public string NewName = "Ceiling assembly location off by 3 inches.\nHow to proceed?";

    private GameObject tagName;
    private GameObject procoreImage;

    void Start()
    {
        tagName = transform.FindChild("Tag Name").gameObject;
        procoreImage = transform.FindChild("Procore Image").gameObject;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        tagName.GetComponent<TextMesh>().text = NewName;
        procoreImage.SetActive(true);
        GetComponent<TextToSpeechOnGaze>().enabled = true;
        GetComponent<TextToSpeechManager>().enabled = true;
        enabled = false;
    }
}
