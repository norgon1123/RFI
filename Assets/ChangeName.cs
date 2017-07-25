using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeName : MonoBehaviour, IInputClickHandler
{
    public string OriginalName = "RFI";
    public string NewName = "Ceiling assembly location off by 3 inches. How to proceed?";

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
        enabled = false;
    }
}
