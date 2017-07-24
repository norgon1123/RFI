using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

public class Discover : MonoBehaviour
{
    public static Discover Instance;
    public AudioClip activated;

    public bool Exploring
    {
        get
        {
            return exploring;
        }
    }

    private Vector3 playerOriginalPos;

    private int spatialNavLayer;

    private float floorYPos;
    private float ceilingYPos;
    private float startExpTime;
    private float stopExpTime;

    private bool hasData;
    private bool exploring;
    private bool ceiling;
    private bool floor;

    private GameObject _circleObj;
    private GameObject focusedObject;

    private Transform lastFocusedObjectPos;

    private GazeManager gazeManager;

    private RaycastHit hit;

    // Use this for initialization
    void Start()
    {
        hasData = false;
        exploring = true;
        ceiling = false;
        floor = false;

        startExpTime = 0;
        stopExpTime = 0;

        focusedObject = null;
        lastFocusedObjectPos = null;

        gazeManager = GazeManager.Instance;

        spatialNavLayer = LayerMask.NameToLayer("SpatialMapping");

        if (Instance == null)
        {
            Instance = this;
        }
    }
}