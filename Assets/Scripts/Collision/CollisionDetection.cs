using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{

    public float Distance;
    public GameObject Cube;
    public static CollisionDetection Instance;
    public bool UseUnscaledTime = true;
    public Status status;
    public Status oldStatus;
    public AudioClip Clip;
    public float Radius = 0.5f;

    public enum Status
    {
        Off,
        On,
        OnlyCube
    }

    /// <summary>
    /// Blend value for surface normal to user facing lerp
    /// </summary>
    public float PositionLerpTime = 0.01f;

    /// <summary>
    /// Blend value for surface normal to user facing lerp
    /// </summary>
    public float RotationLerpTime = 0.01f;

    public Transform CursorTransform;

    private GameObject audioSourceContainer;
    private AudioSource audioSource;
    private float lastPitch;
    private float gazeDis;
    private GazeManager gazeManager;
    private float floorYPos;
    private Vector3 bottomCap;
    private RaycastHit hit;
    private int meshMask;
    private bool castHit;

    /// <summary>
    /// Position, scale and rotational goals for cube
    /// </summary>
    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Vector3 hitPos;
    private Quaternion targetRotation;

    // Use this for initialization
    void Start()
    {
        gazeManager = GazeManager.Instance;
        meshMask = LayerMask.GetMask("SpatialMapping");

        //		string path = SoundManager.Instance.LookupSound ("Pitch 5sec");

        // Create audio source container to move audio source to the detected table
        audioSourceContainer = new GameObject("AudioSourceContainer - CollisionDetection", new Type[] { typeof(AudioSource) });
        audioSource = audioSourceContainer.GetComponent<AudioSource>();
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.dopplerLevel = 0.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.loop = true;
        audioSource.volume = 0.8f;
        audioSource.pitch = 1.5f;
        audioSource.clip = Clip;

        if (!Instance)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Do a capsule cast to find distance to hit object only if running collision detection
        if (status != Status.Off)
        {
            Vector3 direction = Camera.main.transform.forward;
            Vector3 position = Camera.main.transform.position;
            bottomCap = new Vector3(position.x, floorYPos + Radius, position.z);

            // Normalize the distance
            castHit = Physics.CapsuleCast(position, bottomCap, Radius, direction, out hit, Distance, meshMask);
            gazeDis = Vector3.Distance(Camera.main.transform.position, hit.point);
            gazeDis = Math.Abs(gazeDis);
        }

        // If collision detection is running, find if the hit pos is in range and change audio source pitch and show cube
        if (status == Status.On)
        {
            if (!castHit || gazeDis > Distance)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    UpdateCubeTransform(false);
                }
                return;
            }
            else
            {
                gazeDis /= Distance;    // normalize gaze distance
                audioSource.transform.position = Vector3.Lerp(audioSource.transform.position, hit.point, Time.deltaTime / 0.01f);
                audioSource.pitch = Mathf.Lerp(lastPitch, 1.3f - gazeDis, Time.deltaTime / 0.01f);  //Adjust pitch dependant on hit point position
                lastPitch = audioSource.pitch;  // Keep track of audio pitch to be able to smoothly transition between pitches
                hitPos = hit.point;

                UpdateCubeTransform(true);

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }

        // If only showing visual cue, show cube if on collision course
        else if (status == Status.OnlyCube)
        {
            if (gazeDis <= Distance)
            {
                UpdateCubeTransform(true);
            }
            else
            {
                UpdateCubeTransform(false);
            }
        }
    }

    /// <summary>
    /// Update the cube's transform
    /// </summary>
    protected virtual void UpdateCubeTransform(bool activate)
    {
        if (!activate)
        {
            Cube.gameObject.SetActive(false);
            return;
        }

        if (!Cube.gameObject.activeSelf)
        {
            Cube.gameObject.SetActive(true);
        }

        targetScale = Vector3.one;

        float deltaTime = UseUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        // Use the lerp times to blend the position to the target position
        Cube.transform.position = Vector3.Lerp(Cube.transform.position, Camera.main.transform.position + Camera.main.transform.forward, deltaTime / PositionLerpTime);
        Cube.transform.rotation = Quaternion.Lerp(Cube.transform.rotation, Camera.main.transform.rotation, deltaTime / RotationLerpTime);
    }

    /// <summary>
    /// Turns detection on
    /// </summary>
    public void StartDetection()
    {
        SetStatus(Status.On);
    }

    /// <summary>
    /// Turn detection off
    /// </summary>
    public void StopDetection()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        SetStatus(Status.Off);
        UpdateCubeTransform(false);
    }

    /// <summary>
    /// Only show cube without audio source
    /// </summary>
    public void ShowCube()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        SetStatus(Status.OnlyCube);
    }

    /// <summary>
    /// Keeps track of current and previous status
    /// </summary>
    /// <param name="_newStatus">New status.</param>
    public void SetStatus(Status _newStatus)
    {
        oldStatus = status;
        status = _newStatus;

        if (_newStatus == Status.Off || _newStatus == Status.OnlyCube)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if (_newStatus == Status.Off)
            {
                UpdateCubeTransform(false);
            }
        }
    }

    /// <summary>
    /// Sets status to previous status
    /// </summary>
    public void RevertStatus()
    {
        SetStatus(oldStatus);
    }

    /// <summary>
    /// Set floor position of world
    /// </summary>
    /// <param name="pos"></param>
    public void FloorPos(float pos)
    {
        floorYPos = pos;
    }
}