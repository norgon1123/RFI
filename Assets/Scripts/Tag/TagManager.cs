using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA;

public class TagManager : MonoBehaviour, IInputClickHandler {

	/// <summary>
	/// Game object that will control converting user speeck to text.
	/// The text will be used as the tag's name, which the user will then 
	/// be able to use for lookup.
	/// </summary>
	public GameObject DictionFeedback;
	public GameObject Tag;
	public GameObject TagDisplay;
	public bool DictionFeedbackOpen { get; private set; }
	public static TagManager Instance;

	private GameObject dictionFeedbackGameObject;
	private Vector3 tagLocation;
	private Dictionary<string, GameObject> tagList;
	private bool createNewTag;
	private bool tagging;
	private GameObject floor;
	private List<GameObject> tagDisplays;
    private WorldAnchorStore store;
    private bool savedRoot;

    /// <summary>
    /// Manages persisted anchors.
    /// </summary>
    protected WorldAnchorManager anchorManager;

    /// <summary>
    /// Controls spatial mapping.  In this script we access spatialMappingManager
    /// to control rendering and to access the physics layer mask.
    /// </summary>
    protected SpatialMappingManager spatialMappingManager;

    public bool CreateNewTag
    {
        get
        {
            return createNewTag;
        }

        set
        {
            createNewTag = value;
        }
    }

    public Vector3 TagLocation
    {
        get
        {
            return tagLocation;
        }

        set
        {
            tagLocation = value;
        }
    }

    private void Start()
	{
		tagging = false;
		tagList = new Dictionary<string, GameObject>();
		tagDisplays = new List<GameObject> ();

        WorldAnchorStore.GetAsync(AnchorStoreLoaded);

        if (!Instance)
		{
			Instance = this;
		}
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
                if (id != "duct1")
                {
                    var instance = Instantiate(Tag);
                    var anchor = store.Load(id, instance);
                    instance.transform.position = anchor.transform.position;
                    tagList.Add(id, instance);
                }
            }
        }
    }

    /// <summary>
    /// Load specific anchor
    /// </summary>
    /// <param name="_tag"></param>
    /// <param name="_tagName"></param>
    /// <returns></returns>
    private WorldAnchor LoadAnchor(GameObject _tag, string _tagName)
    {
        return store.Load(_tagName, _tag);
    }

    /// <summary>
    /// Save anchor associated with tag
    /// </summary>
    /// <param name="_tag"></param>
    /// <param name="_tagName"></param>
    private void SaveAnchor(GameObject _tag, string _tagName)
    {
        bool retTrue;
        var anchor = _tag.AddComponent<WorldAnchor>();
        anchor.transform.position = _tag.transform.position;

        // Remove any previous worldanchor saved with the same name so we can save new one
        this.store.Delete(_tagName);
        retTrue = this.store.Save(_tagName, anchor);
        
        if (!retTrue)
        {
            Debug.Log("Anchor save failed.");
        }
    }

    /// <summary>
    /// Destroy anchor associated with tag
    /// </summary>
    /// <param name="_tag"></param>
    private void ClearAnchor(string _tagName)
    {
        // remove any world anchor component from the game object so that it can be moved
        store.Delete(_tagName);
    }

    /// <summary>
    /// Opens diction feedback to create new tag or
    /// navigate to existing one
    /// </summary>
    /// <param name="_createNewTag">If set to <c>true</c> create new tag.</param>
    public void OpenDictionFeedback(bool _createNewTag)
	{
		if (!tagging)
		{
			CreateNewTag = _createNewTag;

			// Hold the positiong of the location to be tagged
			TagLocation = GazeManager.Instance.HitPosition;
			DictionFeedbackOpen = true;
			CollisionDetection.Instance.StopDetection ();
			dictionFeedbackGameObject = Instantiate(DictionFeedback);
			tagging = true;
		}
	}

	/// <summary>
	/// Creates new tag using diction feedback
	/// </summary>
	/// <param name="_tagName">Tag name.</param>
	public void CreateTag(string _tagName)
	{
		if (!tagList.ContainsKey(_tagName))
		{
			GameObject _tag = Instantiate(Tag, TagLocation, transform.rotation);
			tagList.Add(_tagName, _tag);
            SaveAnchor(_tag, _tagName);

			ShowTag(_tag);
		}
		DestroyDiction();
	}

	/// <summary>
	/// Navigates to tag specified by the user
	/// </summary>
	/// <param name="_tagName">Tag name.</param>
	public void NavigateToTag(string _tagName)
	{
		GameObject _tag;

		if (tagList.TryGetValue(_tagName, out _tag))
		{
			AIPath.Instance.OnTag(_tag);
			GetComponent<AudioSource> ().transform.position = Camera.main.transform.position;
			GetComponent<AudioSource> ().Play ();
		}

		DestroyDiction();
	}

	/// <summary>
	/// Removes diction feedback game object
	/// </summary>
	public void DestroyDiction()
	{
		if (dictionFeedbackGameObject != null)
			Destroy(dictionFeedbackGameObject);
		tagging = false;
	}

	/// <summary>
	/// Adds tag from list of speech commands
	/// </summary>
	/// <param name="name">Name.</param>
	public void PresetTag(string _tagName)
	{
		// Hold the positiong of the location to be tagged
		Vector3 hitPos = GazeManager.Instance.HitPosition;
		hitPos.y += 0.1f;
		TagLocation = hitPos;
		GameObject _tag = Instantiate (Tag, TagLocation, transform.rotation);

		if (!tagList.ContainsKey (_tagName))
		{
			tagList.Add (_tagName, _tag);
            SaveAnchor(_tag, _tagName);
            GetComponent<AudioSource> ().transform.position = Camera.main.transform.position;
			GetComponent<AudioSource> ().Play ();
		}
		else
		{
			GameObject oldTag;

			// Destroy old tag in dictionary
			if (tagList.TryGetValue(_tagName, out oldTag))
			{
				// Destroy old Tag Display
				foreach (GameObject tag in tagDisplays)
				{
					if (_tagName == tag.transform.FindChild("Tag Name").gameObject.GetComponent<TextMesh>().text)
					{
						tagDisplays.Remove(tag);
						Destroy(tag);
						break;
					}
				}
				Destroy(oldTag);

			}

			tagList.Remove (_tagName);
            SaveAnchor(_tag, _tagName);
			tagList.Add (_tagName, _tag);
			GetComponent<AudioSource> ().transform.position = Camera.main.transform.position;
			GetComponent<AudioSource> ().Play ();
		}

        GameObject tagDisplay = ShowTag(_tag);
        if (_tagName == "RFI")
        {
            tagDisplay.GetComponent<TextToSpeechManager>().enabled = true;
            tagDisplay.GetComponent<TextToSpeechOnGaze>().enabled = true;
            tagDisplay.GetComponent<ChangeName>().enabled = true;
        }
    }

	/// <summary>
	/// Adds tag from cognitive API
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="hitPos">Hit position.</param>
	public void PresetTag(string _tagName, Vector3 hitPos)
	{
		// Hold the positiong of the location to be tagged
		hitPos.y += 0.1f;
		TagLocation = hitPos;
		GameObject _tag = Instantiate (Tag, TagLocation, transform.rotation);

		if (!tagList.ContainsKey (_tagName))
		{
			tagList.Add (_tagName, _tag);
            SaveAnchor(_tag, _tagName);
			ShowTag(_tag);
		}
		else
		{
			tagList.Remove (_tagName);
			tagList.Add (_tagName, _tag);
            SaveAnchor(_tag, _tagName);
			ShowTag(_tag);
		}
	}

	/// <summary>
	/// Navigate to user gaze on tap gesture
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnInputClicked(InputClickedEventData eventData)
	{
		AIPath.Instance.OnGesture ();
	}

	/// <summary>
	/// Shows all tags, used in explorer mode
	/// </summary>
	public void ShowAllTags()
	{
		RemoveAllTags ();
		if (tagList.Count > 0)
		{
			foreach (KeyValuePair<string, GameObject> tag in tagList)
			{
				GameObject _tagDisplay = Instantiate (TagDisplay, tag.Value.transform.position, TagDisplay.transform.rotation);

				GameObject tagName = _tagDisplay.transform.FindChild("Tag Name").gameObject;
				GameObject otherTags = _tagDisplay.transform.FindChild("Other Tags").gameObject;

				tagName.GetComponent<TextMesh>().text = "";
				otherTags.GetComponent<TextMesh>().text = "";

				string[] tagList = tag.Key.Split(',');

				if (tagList.Length > 4)
				{
					for (int i=0; i<5; i++)
					{
						if (i<4)
						{
							tagName.GetComponent<TextMesh>().text += tagList[i] + ", ";
						}
						else
						{
							tagName.GetComponent<TextMesh>().text += tagList[i];
						}
					}
				}
				else
				{
					for (int i=0; i<tagList.Length; i++)
					{
						if (i<tagList.Length-1)
						{
							tagName.GetComponent<TextMesh>().text += tagList[i] + ", ";
						}
						else
						{
							tagName.GetComponent<TextMesh>().text += tagList[i];
						}
					}
				}
				tagDisplays.Add (_tagDisplay);
			}
		}
	}

	/// <summary>
	/// Removes all tags from view
	/// </summary>
	public void RemoveAllTags()
	{
		Debug.Log("removed all");
		if (tagDisplays.Count > 0)
		{
			foreach (GameObject tag in tagDisplays)
			{
				Destroy (tag);
			}
		}
		tagDisplays.Clear ();
	}

	/// <summary>
	/// Show specific tag
	/// </summary>
	/// <param name="selectedTag">Selected tag.</param>
	public GameObject ShowTag(GameObject selectedTag)
	{
		if (tagList.Count > 0)
		{
			foreach (KeyValuePair<string, GameObject> tag in tagList)
			{
				if (tag.Value == selectedTag)
				{
					GameObject _tagDisplay = Instantiate (TagDisplay, tag.Value.transform.position, TagDisplay.transform.rotation);

					GameObject tagName = _tagDisplay.transform.FindChild("Tag Name").gameObject;
					GameObject otherTags = _tagDisplay.transform.FindChild("Other Tags").gameObject;

					tagName.GetComponent<TextMesh>().text = "";
					otherTags.GetComponent<TextMesh>().text = "";

					string[] tagList = tag.Key.Split(',');

                    if (tagList.Length > 4)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (i < 4)
                            {
                                tagName.GetComponent<TextMesh>().text += tagList[i] + ", ";
                            }
                            else
                            {
                                tagName.GetComponent<TextMesh>().text += tagList[i];
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < tagList.Length; i++)
                        {
                            if (i < tagList.Length - 1)
                            {
                                tagName.GetComponent<TextMesh>().text += tagList[i] + ", ";
                            }
                            else
                            {
                                tagName.GetComponent<TextMesh>().text += tagList[i];
                            }
                        }
                    }
					tagDisplays.Add (_tagDisplay);
                    return _tagDisplay;
				}
			}
		}
        return null;
	}

	/// <summary>
	/// Keeps tag in view while navigating
	/// </summary>
	/// <param name="selectedTag">Selected tag.</param>
	public void KeepTag(GameObject selectedTag)
	{
		if (tagDisplays.Count > 0)
		{
			foreach (GameObject tag in tagDisplays)
			{
				if (tag == selectedTag)
				{
					continue;
				}
				Destroy (tag);
			}
		}
	}

	/// <summary>
	/// Remos tag from dictionary
	/// </summary>
	/// <param name="selectedTag">Selected tag.</param>
	public void RemoveTag(GameObject selectedTag)
	{
		if (tagList.Count > 0)
		{
			foreach (KeyValuePair<string, GameObject> tag in tagList)
			{
				if (tag.Value == selectedTag)
				{
					Destroy (tag.Value);
					tagDisplays.Remove (tag.Value);
				}
			}
		}
	}

	/// <summary>
	/// Delete tag that the user is gazing at.
	/// Also removes tag display from list and tag from dictionary
	/// </summary>
	public void DeleteTag()
	{
		GameObject focusedObject = GazeManager.Instance.HitObject;
		if (focusedObject.tag == "Tag")
		{
            string keyToBeRemoved = "";
            GameObject valueToBeRemoved = null;
			tagDisplays.Remove(focusedObject);
			Destroy(focusedObject);

			foreach (KeyValuePair<string, GameObject> tag in tagList)
			{
				if (tag.Value.transform.position == focusedObject.transform.position)
				{
					keyToBeRemoved = tag.Key;
                    valueToBeRemoved = tag.Value;
                    break;
				}
			}

            ClearAnchor(keyToBeRemoved);
            tagList.Remove(keyToBeRemoved);
            Destroy(valueToBeRemoved);
        }
    }

    /// <summary>
    /// Delete all tags, tag displays, and anchors
    /// </summary>
    public void DeleteAllTags()
    {
        foreach (KeyValuePair<string, GameObject> tag in tagList)
        {
            Destroy(tag.Value);
        }
        tagList.Clear();

        foreach (GameObject tag in tagDisplays)
        {
            Destroy(tag);
        }
        tagDisplays.Clear();

        WorldAnchorManager.Instance.RemoveAllAnchors();
    }
}