using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA;
using HoloToolkit.Unity.InputModule;

public class MarkupManager : MonoBehaviour
{
    #region public variables
    public static MarkupManager Instance;
    public GameObject UnwlakableObject;

    public Material VisibleMesh;
    public Material InvisibleMesh;
    #endregion

    #region private variables
    private Dictionary<string, GameObject> markupList;
    private WorldAnchorStore store;
    private int anchorId;
    private bool visible;
    #endregion

    // Use this for initialization
    void Start()
    {
        markupList = new Dictionary<string, GameObject>();
        anchorId = 0;
        visible = false;

        WorldAnchorStore.GetAsync(AnchorStoreLoaded);

        if (Instance == null)
        {
            Instance = this;
        }
    }

    #region public methods

    /// <summary>
    /// Place marker from button call or destroy marker if gazing at one
    /// </summary>
    public void PlaceMarker()
    {
        // If the user is gazing at an existing markup, delete it and return
        if (markupList.ContainsValue(GazeManager.Instance.HitObject))
        {
            markupList.Remove(anchorId.ToString());
            Destroy(GazeManager.Instance.HitObject);
            return;
        }

        // Create new markup at raycast hit
        GameObject _markup = Instantiate(UnwlakableObject, GazeManager.Instance.HitPosition, UnwlakableObject.transform.rotation);
        _markup.gameObject.SetActive(true);
        anchorId++;
        markupList.Add(anchorId.ToString(), _markup);
#if !UNITY_EDITOR
        SaveAnchor(anchorId.ToString(), _markup);
#endif
    }

    /// <summary>
    /// Turn markup visibility on/off
    /// </summary>
    public void ToggleVisibility()
    {
        if (!visible)
        {
            foreach (KeyValuePair<string, GameObject> _markup in markupList)
            {
                _markup.Value.GetComponent<MeshRenderer>().material = VisibleMesh;
            }
        }
        else
        {
            foreach (KeyValuePair<string, GameObject> _markup in markupList)
            {
                _markup.Value.GetComponent<MeshRenderer>().material = InvisibleMesh;
            }
        }
        visible = !visible;
    }
#endregion

#region private methods
    /// <summary>
    /// Delegate called when anchor store is loaded
    /// </summary>
    /// <param name="store"></param>
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
            int result;
            var ids = store.GetAllIds();

            foreach (var id in ids)
            {
                if (!int.TryParse(id, out result))
                { continue; }

                var instance = Instantiate(UnwlakableObject);
                var anchor = store.Load(id, instance);
                instance.transform.position = anchor.transform.position;
                instance.gameObject.SetActive(true);
                markupList.Add(id, instance);

                anchorId++;
            }
        }
    }

    /// <summary>
    /// Load specific anchor
    /// </summary>
    /// <param name="_tag"></param>
    /// <param name="_tagName"></param>
    /// <returns></returns>
    private WorldAnchor LoadAnchor(string _anchorName, GameObject _markup)
    {
        return store.Load(_anchorName, _markup);
    }

    /// <summary>
    /// Save anchor associated with tag
    /// </summary>
    /// <param name="_tag"></param>
    /// <param name="_tagName"></param>
    private void SaveAnchor(string _anchorName, GameObject _markup)
    {
        var anchor = _markup.AddComponent<WorldAnchor>();
        anchor.transform.position = _markup.transform.position;

        // Remove any previous worldanchor saved with the same name so we can save new one
        try
        {
            store.Delete(_anchorName);
            store.Save(_anchorName, anchor);
        }
        catch
        {
            throw new System.Exception("Anchor save failed.");
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
#endregion
}