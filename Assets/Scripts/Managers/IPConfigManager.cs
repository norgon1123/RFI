using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPConfigManager : MonoBehaviour {
	public static IPConfigManager Instance;

	public void Start()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}


    /// <summary>
    /// Creats the numpad to enter in a new IP address to touchOSC
    /// </summary>
	public void Create()
	{
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
		transform.rotation = Camera.main.transform.rotation;

        if (oscControl.Instance.TargetAddr != null)
        {
            GetComponentInChildren<TextMesh>().text = oscControl.Instance.TargetAddr;
        }

		gameObject.SetActive(true);
	}


    /// <summary>
    /// Deletes game object
    /// </summary>
	public void Remove()
	{
		gameObject.SetActive(false);
	}

    /// <summary>
    /// Sets the new address and restarts oscControl
    /// </summary>
	public void Enter()
	{
        TouchOscAddress.Instance.Save("IpAddress", ".txt", GetComponentInChildren<TextMesh>().text);
		oscControl.Instance.TargetAddr = GetComponentInChildren<TextMesh>().text;
        oscControl.Instance.ReStart();

        gameObject.SetActive(false);
	}
}