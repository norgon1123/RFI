//
//	  UnityOSC - Example of usage for OSC receiver
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;
using HoloToolkit.Unity.InputModule;

public class oscControl : MonoBehaviour
{

    public static oscControl Instance;
    public string TargetAddr;
    public int OutGoingPort = 9000;
    public int InComingPort = 8000;
    [HideInInspector]
    public bool connectedToInternet = false;

    private Dictionary<string, ServerLog> servers;
    private Dictionary<string, ClientLog> clients;
    private GazeManager gazeManager;
    private GameObject focusedObject;

    private OSCHandler oscHandler;

    private bool started = false;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Do not attempt connection if the device is not connected to the internet
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                connectedToInternet = false;
                return;
            default:
                connectedToInternet = true;
                break;
        }

        string savedIP = GetComponent<TouchOscAddress>().Load("IpAddress", ".txt");

        // Load IP address if one is saved and start update method
        if (savedIP != null && savedIP != "")
        {
            TargetAddr = savedIP;

            OSCHandler.Instance.Init("TouchOSC Bridge", TargetAddr, 9000, 8000);

            servers = new Dictionary<string, ServerLog>();
            clients = new Dictionary<string, ClientLog>();

            gazeManager = GazeManager.Instance;
            started = true;
        }
    }

    // NOTE: The received messages at each server are updated here
    // Hence, this update depends on your application architecture
    // How many frames per second or Update() calls per frame?
    void Update()
    {
        if (!connectedToInternet || !started)
        { return; }

        // Send debug info to osc
        float distance = Vector3.Distance(Camera.main.transform.position, gazeManager.HitPosition);

        //OSCHandler.Instance.SendMessageToClient("TouchOSC Bridge", "/2/label12", Math.Round(Camera.main.transform.rotation.x, 2).ToString());
        //OSCHandler.Instance.SendMessageToClient("TouchOSC Bridge", "/2/label13", Math.Round(Camera.main.transform.rotation.y, 2).ToString());
        //OSCHandler.Instance.SendMessageToClient("TouchOSC Bridge", "/2/label14", Math.Round(distance, 2).ToString());

        OSCHandler.Instance.UpdateLogs();

        servers = OSCHandler.Instance.Servers;
        clients = OSCHandler.Instance.Clients;

        // Iterate through data received from another OSC device
        foreach (KeyValuePair<string, ServerLog> item in servers)
        {
            // If we have received at least one packet,
            // show the last received from the log in the Debug console
            if (item.Value.log.Count > 0)
            {

                int lastPacketIndex = item.Value.packets.Count - 1;

                // Skip to next value if the button wasn't pressed
                if (item.Value.packets[lastPacketIndex].Data[0].ToString() == "0" ||
                    item.Value.packets[lastPacketIndex].Address == "/1/toggle999")
                {
                    continue;
                }

                ReceivedAction(item.Value.packets[lastPacketIndex].Address);
                item.Value.packets[lastPacketIndex].Address = "/1/toggle999";
            }
        }

        // Iterate through sent messages
        //foreach( KeyValuePair<string, ClientLog> item in clients )
        //{
        //}
    }

    /// <summary>
    /// Check the received data and decide if an action should be taken
    /// </summary>
    /// <param name="Address"></param>
    private void ReceivedAction(string Address)
    {
        // Navigate to gaze
        if (Address == "/AugSense/push1")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            AIPath.Instance.OnGesture();
        }

        // Call cognitive API
        else if (Address == "/AugSense/push2")
        {
            //GetComponent<AudioSource> ().Play ();
            GazeGestureManager.Instance.TakePhoto();
        }

        //// Enter explorer mode
        //else if (Address == "/AugSense/push3")
        //{
        //    GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
        //    GetComponent<AudioSource>().Play();
        //    Discover.Instance.StartExplorer();
        //}

        //// Stop explorer mode
        //else if (Address == "/AugSense/push4")
        //{
        //    GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
        //    GetComponent<AudioSource>().Play();
        //    Discover.Instance.StopExplorer();
        //}

        // Create new tag using diction feedback
        else if (Address == "/AugSense/push5")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            TagManager.Instance.OpenDictionFeedback(true);
        }

        // Navigate to tag using diction feedback
        else if (Address == "/AugSense/push6")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            TagManager.Instance.OpenDictionFeedback(false);
        }

        // Delete tag
        else if (Address == "/AugSense/push7")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            TagManager.Instance.DeleteTag();
        }

        // Change surface mesh material
        else if (Address == "/2/push8")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            PlaySpaceManager.Instance.SwitchMeshMaterial();
        }

        // Toggle mesh visibility
        else if (Address == "/AugSense/push10")
        {
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();
            PlaySpaceManager.Instance.ToggleMesh();
        }
    }

    /// <summary>
    /// Re-runs the start function
    /// Used if a new IP address is used after launch
    /// </summary>
    public void ReStart()
    {
        // Do not attempt connection if the device is not connected to the internet
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                connectedToInternet = false;
                return;
            default:
                connectedToInternet = true;
                break;
        }

        // Close previous connections
        OSCHandler.Instance.OnApplicationQuit();

        OSCHandler.Instance.Init("TouchOSC Bridge", TargetAddr, 9000, 8000);

        servers = new Dictionary<string, ServerLog>();
        clients = new Dictionary<string, ClientLog>();

        gazeManager = GazeManager.Instance;

        started = true;
    }

    /// <summary>
    /// Converts a collection of object values to a concatenated string.S
    /// </summary>
    /// <param name="data">
    /// A <see cref="List<System.Object>"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
    private float DataToInt(List<object> data)
    {
        float num = 0;

        for (int i = 0; i < data.Count; i++)
        {
            num = float.Parse(num.ToString() + data[i].ToString());
        }

        return num;
    }
}