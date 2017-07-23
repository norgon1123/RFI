using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

public class RefreshScan : MonoBehaviour
{

    private IEnumerator refresh;
    private IEnumerator scan;
    private float lastScanTime;
    private float scanWaitTime;
    private int baseTriangleCount;
    private int triangleMult;
    private bool scanning;
    private bool scanRoutine;

    public static RefreshScan Instance;

    // Use this for initialization
    void Start()
    {
        scan = ScanGrid();

        baseTriangleCount = 50000;
        triangleMult = 0;

        lastScanTime = 0;
        scanWaitTime = 0;

        scanning = true;
        scanRoutine = false;

        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!scanning || Time.time - lastScanTime < 3)
        {
            return;
        }
#else
        if (!scanning || Time.time - lastScanTime < 10)
        {
            return;
        }
#endif
        List<Mesh> meshes = SpatialMappingManager.Instance.GetMeshes();
        int triangles = 0;
        foreach (Mesh m in meshes)
        {
            triangles += m.triangles.Length;
        }

        // Only rescan if the room size has increased enough or there is no
        // current a* grid
        if (triangles > baseTriangleCount * triangleMult)
        {
            triangleMult = Mathf.RoundToInt(triangles / 50000) + 1;
            StopScan();
        }
    }

    private void StopScan()
    {
        scanning = false;
        PlaySpaceManager.Instance.StopScan();
    }

    public void StartScan()
    {
        SpatialMappingManager.Instance.StartObserver();
        PlaySpaceManager.Instance.StartScan();
        scanning = true;

        lastScanTime = Time.time;

        if (!scanRoutine)
        {
            StartCoroutine(scan);
        }
    }

    private IEnumerator ScanGrid()
    {
        scanRoutine = true;
        while (true)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(3);
#else
            yield return new WaitForSeconds(10);
#endif

            if (!AstarPath.active.isScanning)
            {
                AstarPath.active.Scan();
            }
        }
    }
}
