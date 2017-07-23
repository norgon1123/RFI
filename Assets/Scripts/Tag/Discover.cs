using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

public class Discover : MonoBehaviour {
	public static Discover Instance;
	public AudioClip activated;

	public bool Exploring {
		get {
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
	private bool created;

	private GameObject _circleObj;
	private GameObject focusedObject;

	private Transform lastFocusedObjectPos;

	private GazeManager gazeManager;

	private RaycastHit hit;

	// Use this for initialization
	void Start () {
		hasData = false;
		created = false;
		exploring = false;
		ceiling = false;
		floor = false;

		startExpTime = 0;
		stopExpTime = 0;

		focusedObject = null;
		lastFocusedObjectPos = null;

		gazeManager = GazeManager.Instance;

		spatialNavLayer = LayerMask.NameToLayer("SpatialMapping");

//		string path = SoundManager.Instance.LookupSound ("click");
//		GetComponent<AudioSource> ().clip = Resources.Load<AudioClip> (path);

		if (Instance == null)
		{
			Instance = this;
		}
	}

	// Update is called once per frame
	void Update ()
	{
        // The user can cancel the mode by looking down
        if (exploring)
        {
            if (Camera.main.transform.rotation.x >= 0.55f)
            {
                TagManager.Instance.RemoveAllTags();
                playerOriginalPos = new Vector3(999, 999, 999);
                CollisionDetection.Instance.RevertStatus();
                exploring = false;
                created = false;
                return;
            }

            // Keep track of what tag the user is looking at
            if (gazeManager.HitObject != null && gazeManager.HitObject.GetComponent<TextToSpeechOnGaze>() != null)
            {
                focusedObject = gazeManager.HitObject;
                lastFocusedObjectPos = focusedObject.transform;
            }

            Vector3 currentPos = Camera.main.transform.position;
            currentPos = new Vector3(currentPos.x, 0, currentPos.z);

            // If the user steps towards the tag, draw a path
            if (Vector3.Distance(currentPos, playerOriginalPos) > 0.5f)
            {
                playerOriginalPos = new Vector3(999, 999, 999);
                exploring = false;
                created = false;
                CollisionDetection.Instance.RevertStatus();
                if (focusedObject != null)
                {
                    AIPath.Instance.OnTag(focusedObject);
                    TagManager.Instance.KeepTag(focusedObject);
                    focusedObject = null;
                }
                // If the user has not selected a tag, exit the mode
                else
                {
                    TagManager.Instance.RemoveAllTags();
                }
            }
        }
	}

	/// <summary>
	/// Get the floor and ceiling position after the planes have been created
	/// </summary>
	/// <param name="ActivePlanes">Active planes.</param>
	public void SetPositions(List<GameObject> ActivePlanes)
	{
		foreach (GameObject plane in ActivePlanes)
		{
			SurfacePlane surfacePlane = plane.GetComponent<SurfacePlane>();

			if (surfacePlane.PlaneType == PlaneTypes.Floor)
			{
				floorYPos = surfacePlane.transform.position.y;
				floorYPos += 0.1f;
				floor = true;
			}
			if (surfacePlane.PlaneType == PlaneTypes.Ceiling)
			{
				ceilingYPos = surfacePlane.transform.position.y;
				ceilingYPos -= 0.6f;
				ceiling = true;
			}
			if (ceiling && floor)
			{
				hasData = true;
				break;
			}
		}
	}

	/// <summary>
	/// Starts explorer mode from voice command
	/// </summary>
	public void StartExplorer()
	{
		if (Time.time - startExpTime > 2)
        {
			startExpTime = Time.time;
            CollisionDetection.Instance.StopDetection();

            TagManager.Instance.ShowAllTags();

            GetComponent<AudioSource>().clip = activated;
            GetComponent<AudioSource>().transform.position = Camera.main.transform.position;
            GetComponent<AudioSource>().Play();

            playerOriginalPos = Camera.main.transform.position;
            playerOriginalPos = new Vector3(playerOriginalPos.x, 0, playerOriginalPos.z);
            created = true;
            exploring = true;
        }
	}

	/// <summary>
	/// Stops explorer mode from voice command
	/// </summary>
	public void StopExplorer()
	{
		if (Time.time - stopExpTime > 2)
        {
			stopExpTime = Time.time;
            created = false;
			exploring = false;
            TagManager.Instance.RemoveAllTags();
            playerOriginalPos = new Vector3(999, 999, 999);
            CollisionDetection.Instance.RevertStatus();
        }
    }

	/// <summary>
	/// Navigates to tag last gazed at from button
	/// </summary>
	public void NavToLastTag()
	{
		if (exploring && focusedObject != null)
		{
			playerOriginalPos = new Vector3(999,999,999);

			exploring = false;
			created = false;

			CollisionDetection.Instance.RevertStatus();
			AIPath.Instance.OnTag (focusedObject);
			TagManager.Instance.KeepTag(focusedObject);

			focusedObject = null;
		}
		else if (lastFocusedObjectPos != null)
		{
			AIPath.Instance.OnTagTransform(lastFocusedObjectPos);
		}
	}
		
}
