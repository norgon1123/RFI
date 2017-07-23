using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// InteractibleManager keeps tracks of which GameObject
/// is currently in focus.
/// </summary>
public class InteractibleManager : Singleton<InteractibleManager>
{
    public GameObject FocusedGameObject { get; private set; }

    private GameObject oldFocusedGameObject = null;

    private int interactibleLayerMask;


    void Start()
    {
        FocusedGameObject = null;

        interactibleLayerMask = LayerMask.NameToLayer("Interactible");
    }


    void LateUpdate()
    {
        oldFocusedGameObject = FocusedGameObject;

        if (GazeManager.Instance.IsGazingAtObject)
        {
            RaycastHit hitInfo = GazeManager.Instance.HitInfo;
            if (hitInfo.collider != null)
            {
                FocusedGameObject = hitInfo.collider.gameObject;
            }
            else
            {
                FocusedGameObject = null;
            }
        }
        else
        {
            FocusedGameObject = null;
        }

        if (FocusedGameObject != oldFocusedGameObject)
        {
            ResetFocusedInteractible();

            if (FocusedGameObject != null)
            {
                if (FocusedGameObject.GetComponent<Interactible>() != null || FocusedGameObject.layer == interactibleLayerMask)
                {
                    FocusedGameObject.SendMessageUpwards("GazeEntered");
                }
            }
        }
    }

    private void ResetFocusedInteractible()
    {
        if (oldFocusedGameObject != null)
        {
            if (oldFocusedGameObject.GetComponent<Interactible>() != null || oldFocusedGameObject.layer == interactibleLayerMask)
            {
                oldFocusedGameObject.SendMessageUpwards("GazeExited");
            }
        }
    }
}