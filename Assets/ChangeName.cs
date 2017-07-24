using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeName : MonoBehaviour, IInputClickHandler
{
    public string OriginalName = "RFI";
    public string NewName = "Ceiling assembly location off by 3\". How to proceed?";

    private GameObject tagName;

    void Start()
    {
        tagName = transform.FindChild("Tag Name").gameObject;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        tagName.GetComponent<TextMesh>().text = NewName;
        enabled = false;
    }
}
