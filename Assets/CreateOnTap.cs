using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Persistence;

public class CreateOnTap : MonoBehaviour, IInputClickHandler
{
    public GameObject ObjectToBeCreated;
    public static CreateOnTap Instance;
    private WorldAnchorStore store;

    // Use this for initialization
    void Start () {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        WorldAnchorStore.GetAsync(AnchorStoreLoaded);

        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void AnchorStoreLoaded(WorldAnchorStore store)
    {
        this.store = store;
        LoadAnchors();
    }

    /// <summary>
    /// Load all anchors
    /// </summary>
    private void LoadAnchors()
    {
        // Load in tags from previous session
        if (store != null)
        {
            var ids = store.GetAllIds();

            foreach (var id in ids)
            {
                if (id == "duct1")
                {
                    var anchor = store.Load(id, ObjectToBeCreated);
                    ObjectToBeCreated.transform.position = anchor.transform.position;

                    // Rotate this object to face the user.
                    Quaternion toQuat = Camera.main.transform.localRotation;
                    toQuat.x = 0;
                    toQuat.z = 0;
                    gameObject.transform.rotation = toQuat;

                    ObjectToBeCreated.gameObject.SetActive(true);

                    InputManager.Instance.PopFallbackInputHandler();
                    InputManager.Instance.PushFallbackInputHandler(TagManager.Instance.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Create duct and remove 
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        Vector3 pos = GazeManager.Instance.HitPosition;
        pos.y += 2f;
        ObjectToBeCreated.transform.position = pos;

        // Rotate this object to face the user.
        Quaternion toQuat = Camera.main.transform.localRotation;
        toQuat.x = 0;
        toQuat.z = 0;
        gameObject.transform.rotation = toQuat;

        ObjectToBeCreated.gameObject.SetActive(true);

        InputManager.Instance.PopFallbackInputHandler();
        InputManager.Instance.PushFallbackInputHandler(TagManager.Instance.gameObject);
    }

    /// <summary>
    /// Toggle placeability of object
    /// </summary>
    public void Lock()
    {
        if (ObjectToBeCreated != null)
        {
            ObjectToBeCreated.GetComponent<TapToPlace>().Lock();
        }
    }

    /// <summary>
    /// Place duct at gaze position if anchor load position needs
    /// to be reset
    /// </summary>
    public void PlaceHere()
    {
        Vector3 pos = GazeManager.Instance.HitPosition;
        pos.y += 2f;
        ObjectToBeCreated.transform.position = pos;

        // Rotate this object to face the user.
        Quaternion toQuat = Camera.main.transform.localRotation;
        toQuat.x = 0;
        toQuat.z = 0;
        gameObject.transform.rotation = toQuat;

        ObjectToBeCreated.gameObject.SetActive(true);

        InputManager.Instance.PopFallbackInputHandler();
        InputManager.Instance.PushFallbackInputHandler(TagManager.Instance.gameObject);
    }
}
