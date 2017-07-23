using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ButtonManager : MonoBehaviour, IInputClickHandler {
	public string Entry;

	public Actions action;

	public EntryType entryType;

	public enum EntryType
	{
		EnterChar,
		Action
	}

	public enum Actions
	{
		Enter,
		Clear,
		None
	}

	public void OnInputClicked(InputClickedEventData eventData)
	{
		if (entryType == EntryType.EnterChar)
		{
			GetComponentInParent<TextMesh>().text += Entry;
			GetComponentInParent<AudioSource>().Play();
		}
		else if (entryType == EntryType.Action)
		{
			if (action == Actions.Clear)
			{
				GetComponentInParent<TextMesh>().text = "";
			}
			else if (action == Actions.Enter)
			{
				GetComponentInParent<IPConfigManager>().Enter();
			}
		}

	}

	public void TestThing()
	{
		GetComponentInParent<AudioSource>().Play();
		if (entryType == EntryType.EnterChar)
		{
			Debug.Log(Entry);
			GetComponentInParent<TextMesh>().text += Entry;
		}
		else if (entryType == EntryType.Action)
		{
			if (action == Actions.Clear)
			{
				GetComponent<TextMesh>().text = "";
			}
			else if (action == Actions.Enter)
			{
				IPConfigManager.Instance.Enter();
			}
		}

	}
}