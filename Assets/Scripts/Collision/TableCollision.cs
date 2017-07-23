using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.SpatialMapping;

public class TableCollision : MonoBehaviour {

    public float Distance = 1;
	public static TableCollision Instance;

    private Vector3 position;
    private int tableMask;
    private int meshMask;
    private RaycastHit hit;
    private Vector3 direction;
    private GameObject audioSourceContainer;
    private List<GameObject> tables;
    private AudioSource audioSource;
    private Vector3 bottomCap;
    private bool detecting;
    private float floorYPos;
    private bool reset;

    private void Start()
    {
        // Set variables for collision logic
        tableMask = LayerMask.GetMask("Table");
        meshMask = LayerMask.GetMask("SpatialMapping");
        detecting = false;
        tables = new List<GameObject>();
        reset = false;

//		string path = SoundManager.Instance.LookupSound ("Colission");

        // Create audio source container to move audio source to the detected table
        audioSourceContainer = new GameObject("AudioSourceContainer - TableCollision", new Type[] { typeof(AudioSource) });
        audioSource = audioSourceContainer.GetComponent<AudioSource>();
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.dopplerLevel = 0.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.loop = true;
        audioSource.volume = 1;
		audioSource.clip = GetComponent<AudioSource>().clip;

		if (Instance == null)
		{
			Instance = this;
		}
    }

// Update is called once per frame
    void Update ()
    {
        if (detecting)
        {
            direction = Camera.main.transform.forward;
            position = Camera.main.transform.position;
            bottomCap = new Vector3(position.x, floorYPos, position.z);
            // Use a capsule that extends from the camera to the user's feet extended by 3 meters if
            // they are on a collision course with a table
            if (Physics.CapsuleCast(position, bottomCap, 0.4f, direction, out hit, Distance, tableMask))
            {
                float hitDistance = Vector3.Distance(Camera.main.transform.position, hit.collider.gameObject.transform.position);
                hitDistance -= 0.2f;
                // Return if the table is on the other side of a wall
                if (Physics.CapsuleCast(position, bottomCap, 0.4f, direction, hitDistance, meshMask))
                {
                    return;
                }
                if (hit.collider.gameObject.transform.position != audioSourceContainer.transform.position)
                {
                    SetAllTablesVisible(false);
                    SetTableVisible(hit.collider.gameObject.transform.position);
                    audioSource.Stop();
                    audioSourceContainer.transform.position = hit.collider.gameObject.transform.position;
                    audioSource.Play();
                    reset = true;
                }
            }
            else if (reset)
            {
                SetAllTablesVisible(false);
                audioSource.Stop();
                audioSourceContainer.transform.position = Vector3.zero;
                reset = false;
            }
        }
    }


    /// <summary>
    /// Start detecting tables and playing audio.
    /// Set to true by default.
    /// </summary>
    public void StartDetection()
    {
        detecting = true;
        SetAllTablesVisible(false);
    }

    /// <summary>
    /// Stops table detection and stop audio if it is playing.
    /// Resets the audio source container
    /// </summary>
    public void StopDetection()
    {
        detecting = false;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSourceContainer.transform.position = Vector3.zero;
        }
        SetAllTablesVisible(false);
    }

    /// <summary>
    /// Get all tables found from the mesh
    /// </summary>
    public void GetTables(List<GameObject> ActivePlanes)
    {
        tables.Clear();

        foreach (GameObject plane in ActivePlanes)
        {
            SurfacePlane surfacePlane = plane.GetComponent<SurfacePlane>();
            if (surfacePlane.PlaneType == PlaneTypes.Table)
            {
                tables.Add(plane);
            }
            else if (surfacePlane.PlaneType == PlaneTypes.Floor)
            {
                floorYPos = surfacePlane.transform.position.y;
            }
        }

        detecting = true;

//		BodyCollision.Instance.FloorYPos = floorYPos;
//		BodyCollision.Instance.Detecting = true;
    }

    /// <summary>
    /// Set all table renderers inactive
    /// </summary>
    public void SetAllTablesVisible(bool value)
    {
        if (tables.Count > 0)
        {
            foreach (GameObject table in tables)
            {
                table.GetComponent<Renderer>().enabled = value;
            }
        }
    }

    /// <summary>
    /// Set a specific table visible based on it's location
    /// </summary>
    /// <param name="position"></param>
    public void SetTableVisible(Vector3 position)
    {
        foreach (GameObject table in tables)
        {
            if (table.transform.position == position)
            {
                table.GetComponent<Renderer>().enabled = true;
                break;
            }
        }
    }

	public void ClearTables()
	{
		tables.Clear ();
	}
}
