using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity;

public class ScanningMenuManager : Singleton<ScanningMenuManager>, IInputClickHandler, ISourceStateHandler
{

    public TextMesh Message;
    // public Button DoneButton;

    // private Renderer buttonRenderer;
    private int trackedHandsCount = 0;

    // Use this for initialization
    void Start ()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        // buttonRenderer = DoneButton.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (trackedHandsCount > 0)
        {
            Message.color = Color.green;
        }
        else
        {
            Message.color = Color.white;
        }
    }

    void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
    {
        SpatialMappingManager.Instance.StopObserver();
        PlaySpaceManager.Instance.StopScan();
        gameObject.SetActive(false);
    }

    void ISourceStateHandler.OnSourceDetected(SourceStateEventData eventData)
    {
        trackedHandsCount++;
    }

    void ISourceStateHandler.OnSourceLost(SourceStateEventData eventData)
    {
        trackedHandsCount--;
    }

    public void StopScan()
    {
        PlaySpaceManager.Instance.StopScan();
        SpatialMappingManager.Instance.StopObserver();
        gameObject.SetActive(false);
    }
}
