using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

/** Handles path calls for a single unit.
 * \ingroup relevant
 * This is a component which is meant to be attached to a single unit (AI, Robot, Player, whatever) to handle it's pathfinding calls.
 * It also handles post-processing of paths using modifiers.
 * \see \ref calling-pathfinding
 */
[AddComponentMenu("Pathfinding/Seeker")]
[HelpURL("http://arongranberg.com/astar/docs/class_seeker.php")]
public class Seeker : MonoBehaviour, ISerializationCallbackReceiver {
	//====== SETTINGS ======

	/** Enables drawing of the last calculated path using Gizmos.
	 * The path will show up in green.
	 *
	 * \see OnDrawGizmos
	 */
	public bool drawGizmos = true;

	/** Enables drawing of the non-postprocessed path using Gizmos.
	 * The path will show up in orange.
	 *
	 * Requires that #drawGizmos is true.
	 *
	 * This will show the path before any post processing such as smoothing is applied.
	 *
	 * \see drawGizmos
	 * \see OnDrawGizmos
	 */
	public bool detailedGizmos;

	/** Path modifier which tweaks the start and end points of a path */
	public StartEndModifier startEndModifier = new StartEndModifier();

	/** The tags which the Seeker can traverse.
	 *
	 * \note This field is a bitmask.
	 * \see https://en.wikipedia.org/wiki/Mask_(computing)
	 */
	[HideInInspector]
	public int traversableTags = -1;

	/** Required for serialization backwards compatibility.
	 * \since 3.6.8
	 */
	[UnityEngine.Serialization.FormerlySerializedAs("traversableTags")]
	[SerializeField]
	[HideInInspector]
	protected TagMask traversableTagsCompatibility = new TagMask(-1, -1);

	/** Penalties for each tag.
	 * Tag 0 which is the default tag, will have added a penalty of tagPenalties[0].
	 * These should only be positive values since the A* algorithm cannot handle negative penalties.
	 *
	 * \note This array should always have a length of 32 otherwise the system will ignore it.
	 *
	 * \see Pathfinding.Path.tagPenalties
	 */
	[HideInInspector]
	public int[] tagPenalties = new int[32];

	//====== SETTINGS ======

	/** Callback for when a path is completed.
	 * Movement scripts should register to this delegate.\n
	 * A temporary callback can also be set when calling StartPath, but that delegate will only be called for that path
	 */
	public OnPathDelegate pathCallback;

	/** Called before pathfinding is started */
	public OnPathDelegate preProcessPath;

	/** Called after a path has been calculated, right before modifiers are executed.
	 * Can be anything which only modifies the positions (Vector3[]).
	 */
	public OnPathDelegate postProcessPath;

    /** Waypoint prefab to be drawn along the path */
    public GameObject waypoint;

    /** Holds the waypoints that have been drawn */
    private List<GameObject> waypointList = new List<GameObject>();

	/** Used for drawing gizmos */
	[System.NonSerialized]
	List<Vector3> lastCompletedVectorPath;

	/** Used for drawing gizmos */
	[System.NonSerialized]
	List<GraphNode> lastCompletedNodePath;

	/** The current path */
	[System.NonSerialized]
	protected Path path;

	/** Previous path. Used to draw gizmos */
	[System.NonSerialized]
	private Path prevPath;

	/** Cached delegate to avoid allocating one every time a path is started */
	private readonly OnPathDelegate onPathDelegate;

	/** Temporary callback only called for the current path. This value is set by the StartPath functions */
	private OnPathDelegate tmpPathCallback;

	/** The path ID of the last path queried */
	protected uint lastPathID;

	/** Internal list of all modifiers */
	readonly List<IPathModifier> modifiers = new List<IPathModifier>();

    // Hold list of lines drawn to show current path
    private List<LineRenderer> drawnPath = new List<LineRenderer>();

    private Vector3 currentAudioPos;

    private float switchThreshold = 0.5f;

    private Vector3 cameraPos;

    private int waypointIndex = 0;

    private int pathIndex = 0;

    private int waypointMod = 4;

    // This holds all graph data
    private AstarData data;

    public Vector3 CurrentAudioPos
    {
        get
        {
            return currentAudioPos;
        }

        set
        {
            currentAudioPos = value;
        }
    }

    public List<GameObject> WaypointList
    {
        get
        {
            return waypointList;
        }

        set
        {
            waypointList = value;
        }
    }

    public List<Vector3> LastCompletedVectorPath
    {
        get
        {
            return lastCompletedVectorPath;
        }

        set
        {
            lastCompletedVectorPath = value;
        }
    }

    public int WaypointIndex
    {
        get
        {
            return waypointIndex;
        }

        set
        {
            waypointIndex = value;
        }
    }

    public int PathIndex
    {
        get
        {
            return pathIndex;
        }

        set
        {
            pathIndex = value;
        }
    }

    public enum ModifierPass {
		PreProcess,
		// An obsolete item occupied index 1 previously
		PostProcess = 2,
	}

	public Seeker () {
		onPathDelegate = OnPathComplete;
	}

	/** Initializes a few variables */
	void Awake () {
		startEndModifier.Awake(this);
        CurrentAudioPos = new Vector3(999, 999, 999);
    }

    private void Start()
	{
		data = AstarPath.active.astarData;
    }

    /** Path that is currently being calculated or was last calculated.
	 * You should rarely have to use this. Instead get the path when the path callback is called.
	 *
	 * \see pathCallback
	 */
    public Path GetCurrentPath () {
		return path;
	}

	/** Cleans up some variables.
	 * Releases any eventually claimed paths.
	 * Calls OnDestroy on the #startEndModifier.
	 *
	 * \see ReleaseClaimedPath
	 * \see startEndModifier
	 */
	public void OnDestroy () {
		ReleaseClaimedPath();
		startEndModifier.OnDestroy(this);
	}

	/** Releases the path used for gizmos (if any).
	 * The seeker keeps the latest path claimed so it can draw gizmos.
	 * In some cases this might not be desireable and you want it released.
	 * In that case, you can call this method to release it (not that path gizmos will then not be drawn).
	 *
	 * If you didn't understand anything from the description above, you probably don't need to use this method.
	 *
	 * \see \ref pooling
	 */
	public void ReleaseClaimedPath () {
		if (prevPath != null) {
			prevPath.Release(this, true);
			prevPath = null;
		}
	}

	/** Called by modifiers to register themselves */
	public void RegisterModifier (IPathModifier mod) {
		modifiers.Add(mod);

		// Sort the modifiers based on their specified order
		modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
	}

	/** Called by modifiers when they are disabled or destroyed */
	public void DeregisterModifier (IPathModifier mod) {
		modifiers.Remove(mod);
	}

	/** Post Processes the path.
	 * This will run any modifiers attached to this GameObject on the path.
	 * This is identical to calling RunModifiers(ModifierPass.PostProcess, path)
	 * \see RunModifiers
	 * \since Added in 3.2
	 */
	public void PostProcess (Path p) {
		RunModifiers(ModifierPass.PostProcess, p);
	}

	/** Runs modifiers on path \a p */
	public void RunModifiers (ModifierPass pass, Path p) {
		// Call delegates if they exist
		if (pass == ModifierPass.PreProcess && preProcessPath != null) {
			preProcessPath(p);
		} else if (pass == ModifierPass.PostProcess && postProcessPath != null) {
			postProcessPath(p);
		}

		// Loop through all modifiers and apply post processing
		for (int i = 0; i < modifiers.Count; i++) {
			// Cast to MonoModifier, i.e modifiers attached as scripts to the game object
			var mMod = modifiers[i] as MonoModifier;

			// Ignore modifiers which are not enabled
			if (mMod != null && !mMod.enabled) continue;

			if (pass == ModifierPass.PreProcess) {
				modifiers[i].PreProcess(p);
			} else if (pass == ModifierPass.PostProcess) {
				modifiers[i].Apply(p);
			}
		}
	}

	/** Is the current path done calculating.
	 * Returns true if the current #path has been returned or if the #path is null.
	 *
	 * \note Do not confuse this with Pathfinding.Path.IsDone. They usually return the same value, but not always
	 * since the path might be completely calculated, but it has not yet been processed by the Seeker.
	 *
	 * \since Added in 3.0.8
	 * \version Behaviour changed in 3.2
	 */
	public bool IsDone () {
		return path == null || path.GetState() >= PathState.Returned;
	}

	/** Called when a path has completed.
	 * This should have been implemented as optional parameter values, but that didn't seem to work very well with delegates (the values weren't the default ones)
	 * \see OnPathComplete(Path,bool,bool)
	 */
	void OnPathComplete (Path p) {
		OnPathComplete(p, true, true);
	}

	/** Called when a path has completed.
	 * Will post process it and return it by calling #tmpPathCallback and #pathCallback
	 */
	void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {
		if (p != null && p != path && sendCallbacks) {
			return;
		}

		if (this == null || p == null || p != path)
			return;

		if (!path.error && runModifiers) {
			// This will send the path for post processing to modifiers attached to this Seeker
			RunModifiers(ModifierPass.PostProcess, path);
		}

		if (sendCallbacks) {
			p.Claim(this);

			lastCompletedNodePath = p.path;
			lastCompletedVectorPath = p.vectorPath;

			// This will send the path to the callback (if any) specified when calling StartPath
			if (tmpPathCallback != null) {
				tmpPathCallback(p);
			}

			// This will send the path to any script which has registered to the callback
			if (pathCallback != null) {
				pathCallback(p);
			}

			// Recycle the previous path to reduce the load on the GC
			if (prevPath != null) {
				prevPath.Release(this, true);
			}

			prevPath = p;

			// If not drawing gizmos, then storing prevPath is quite unecessary
			// So clear it and set prevPath to null
			if (!drawGizmos) ReleaseClaimedPath();
		}
	}


	/** Returns a new path instance.
	 * The path will be taken from the path pool if path recycling is turned on.\n
	 * This path can be sent to #StartPath(Path,OnPathDelegate,int) with no change, but if no change is required #StartPath(Vector3,Vector3,OnPathDelegate) does just that.
	 * \code var seeker = GetComponent<Seeker>();
	 * Path p = seeker.GetNewPath (transform.position, transform.position+transform.forward*100);
	 * // Disable heuristics on just this path for example
	 * p.heuristic = Heuristic.None;
	 * seeker.StartPath (p, OnPathComplete);
	 * \endcode
	 */
	public ABPath GetNewPath (Vector3 start, Vector3 end) {
		// Construct a path with start and end points
		return ABPath.Construct(start, end, null);
	}

	/** Call this function to start calculating a path.
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 */
	public Path StartPath (Vector3 start, Vector3 end) {
		return StartPath(start, end, null, -1);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 * \param callback	The function to call when the path has been calculated
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
	public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
		return StartPath(start, end, callback, -1);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 * \param callback	The function to call when the path has been calculated
	 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask.
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
	public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask) {
		return StartPath(GetNewPath(start, end), callback, graphMask);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param p			The path to start calculating
	 * \param callback	The function to call when the path has been calculated
	 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask.
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
	 *
	 * \version Since 3.8.3 this method works properly if a MultiTargetPath is used.
	 * It now behaves identically to the StartMultiTargetPath(MultiTargetPath) method.
	 */
	public Path StartPath (Path p, OnPathDelegate callback = null, int graphMask = -1) {
		p.callback += onPathDelegate;

		p.enabledTags = traversableTags;
		p.tagPenalties = tagPenalties;
		p.nnConstraint.graphMask = graphMask;

		StartPathInternal(p, callback);
		return p;
	}

	/** Internal method to start a path and mark it as the currently active path */
	void StartPathInternal (Path p, OnPathDelegate callback) {
		// Cancel a previously requested path is it has not been processed yet and also make sure that it has not been recycled and used somewhere else
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID) {
			path.Error();
			path.LogError("Canceled path because a new one was requested.\n"+
				"This happens when a new path is requested from the seeker when one was already being calculated.\n" +
				"For example if a unit got a new order, you might request a new path directly instead of waiting for the now" +
				" invalid path to be calculated. Which is probably what you want.\n" +
				"If you are getting this a lot, you might want to consider how you are scheduling path requests.");
			// No callback will be sent for the canceled path
		}

		// Set p as the active path
		path = p;
		tmpPathCallback = callback;

		// Save the path id so we can make sure that if we cancel a path (see above) it should not have been recycled yet.
		lastPathID = path.pathID;

		// Pre process the path
		RunModifiers(ModifierPass.PreProcess, path);

		// Send the request to the pathfinder
		AstarPath.StartPath(path);
	}

    /// <summary>
    /// Draw path in world using line segments
    /// </summary>
    public void DrawPath()
    {
        if (lastCompletedVectorPath == null)
        {
            return;
        }

        // Remove old path
        DeletePath();
		DeleteWaypoints();

        // Draw lines between each segment
        if (lastCompletedVectorPath != null)
        {
            for (int i = 0; i <= lastCompletedVectorPath.Count-1; i++)
            {
				// Draw waypoints every 4 segments and at the end of the path
				if (i == lastCompletedVectorPath.Count - 1 || i % waypointMod == 0)
                {
                    Vector3 waypointLocation = lastCompletedVectorPath[i];
					waypointLocation.y = Camera.main.transform.position.y - 0.2f;
                    GameObject _waypoint = Instantiate(waypoint, waypointLocation, waypoint.transform.rotation);
					if (i == 0)
					{
						_waypoint.gameObject.SetActive (false);
					}

					// Keep track of waypoints in a list
                    waypointList.Add(_waypoint);

					// Draw lines between waypoints
					if (i != 0)
					{
						LineRenderer line = new GameObject().AddComponent<LineRenderer>();

						line.material = gameObject.GetComponent<LineRenderer>().material;
						line.startWidth = gameObject.GetComponent<LineRenderer>().startWidth;
						line.endWidth = gameObject.GetComponent<LineRenderer>().endWidth;

						Vector3 startPos = waypointList[waypointList.Count - 2].transform.position;
						Vector3 endPos = waypointList[waypointList.Count - 1].transform.position;
		
		                // Set line starting and ending points
		                line.SetPosition(0, startPos);
		                line.SetPosition(1, endPos);
		                drawnPath.Add(line);

					}
                }
            }
			WaypointOrientation();
        }
    }

	/// <summary>
	/// Orient the waypoints to face the next one.
	/// The last waypoint should face the previous one.
	/// </summary>
	public void WaypointOrientation()
	{
		if (waypointList.Count > 0)
		{
			for (int i=0; i<waypointList.Count; i++)
			{
				if (i < waypointList.Count -1)
				{
					waypointList[i].transform.LookAt(waypointList[i+1].transform);
				}
				else 
				{
					waypointList[i].transform.LookAt(waypointList[i-1].transform);
				}
			}
		}
	}

    /// <summary>
    /// Delete current path in world
    /// </summary>
    public void DeletePath()
    {
        // Remove old line
        if (drawnPath.Count > 0)
        {
            foreach (LineRenderer line in drawnPath)
            {
                Destroy(line.gameObject);
                Destroy(line.material);
            }
            drawnPath.Clear();
            pathIndex = 0;
        }
    }


    /// <summary>
    /// Delete current waypoints from path in world
    /// </summary>
    public void DeleteWaypoints()
    {
        if (waypointList.Count > 0)
        {
            foreach (GameObject waypoint in waypointList)
            {
                Destroy(waypoint);
            }
            waypointList.Clear();
            waypointIndex = 0;
        }
    }

    /// <summary>
    /// Move audio source in front of the user to follow down the path
    /// Stop audio when the user reaches the end
    /// </summary>
    /// <returns></returns>
    public Vector3 MoveAudio()
    {
        // Reset the camera location info. Set the camera y position to the floor location.
        // This gives the switch threshold distance calculation more accuracy.
        cameraPos = Camera.main.transform.position;
		Vector3 audioPos = WaypointList [waypointIndex].transform.position;
        CurrentAudioPos = audioPos;
        cameraPos.y = CurrentAudioPos.y;
        SetWaypointSize(WaypointList[waypointIndex]);
        //cameraPos = new Vector3(cameraPos.x, data.gridGraph.center.y, cameraPos.z);

        // set audio position to first node initially.
        if (CurrentAudioPos == new Vector3(999, 999, 999))
        {
			audioPos = waypointList[waypointIndex].transform.position;
            CurrentAudioPos = audioPos;
            SetWaypointColor(waypointList[waypointIndex]);
            SetWaypointSize(waypointList[waypointIndex], true);
            SetLineColor(waypointIndex);
            return CurrentAudioPos;
        }

        // move audio position up in front of the user
		else if (waypointIndex < waypointList.Count - 1)
        {
            //Set waypoint color
            if (Vector3.Distance(cameraPos, waypointList[waypointIndex].transform.position) < switchThreshold
                && waypointIndex < waypointList.Count -1 )
            {
                waypointIndex++;
                SetWaypointColor(waypointList[waypointIndex]);
				GetComponent<AudioSource> ().Play();
                SetWaypointSize(waypointList[waypointIndex], true);
                SetLineColor(waypointIndex);
            }

            // If user is close enough to the audio position, move it forward.
            if (Vector3.Distance(cameraPos, CurrentAudioPos) < switchThreshold)
            {
				audioPos = waypointList[waypointIndex].transform.position;
                CurrentAudioPos = audioPos;
                return CurrentAudioPos;
            }
            // If the user is not close enough, leave the audio source where it is.
            else
            {
                return CurrentAudioPos;
            }
        }

        // If the user is within 5 nodes of the end, set the audio source position to the end
        else
        {
            //Set waypoint color
            if (Vector3.Distance(cameraPos, waypointList[waypointIndex].transform.position) < switchThreshold
                && waypointIndex < waypointList.Count - 1)
            {
                waypointIndex++;
                SetWaypointColor(waypointList[waypointIndex]);
				GetComponent<AudioSource> ().Play();
                SetWaypointSize(waypointList[waypointIndex], true);
                SetLineColor(waypointIndex);
            }

            pathIndex = drawnPath.Count - 1;
            // Check how close the user is to the last node (closer than normal), return a key value if it is to turn
            // off the audio source and delete the path when completed.
            if ((Vector3.Distance(cameraPos, CurrentAudioPos) < switchThreshold))
            {
                CurrentAudioPos = new Vector3(999, 999, 999);
                return CurrentAudioPos;
            }
            // If the user is not close enough, leave the audio source where it is.
            else
            {
				audioPos = WaypointList[waypointIndex].transform.position;
                CurrentAudioPos = audioPos;
                return CurrentAudioPos;
            }
        }
    }

	/// <summary>
	/// Set the color of the next line segment
	/// </summary>
	/// <param name="endPos">End position.</param>
    public void SetLineColor(int endPos)
    {
//        endPos *= waypointMod;
//        int startPos = endPos - waypointMod;

        for (int i = 0; i <= drawnPath.Count-1; i++)
        {
            if (i == endPos-1)
            {
				drawnPath[i].GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                drawnPath[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Set next waypoint to green while navigating, otherwise set
    /// them all white
    /// </summary>
    /// <param name="currentWaypoint"></param>
    /// <param name="navigating"></param>
    void SetWaypointColor(GameObject currentWaypoint)
    {
        // Sets current waypoint to green
        foreach (GameObject point in waypointList)
        {
            if (point == currentWaypoint)
            {
				point.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                point.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Increase the current waypoint as the user gets closer
    /// </summary>
    /// <param name="currentWaypoint"></param>
    /// <param name="resetSize"></param>
    void SetWaypointSize(GameObject currentWaypoint, bool resetSize = false)
    {
        float distance = Vector3.Distance(Camera.main.transform.position, currentWaypoint.transform.position);
        distance = Mathf.Abs(distance);

        // normalize between 0,1
        distance = distance / 1.5f;
        distance = distance * 0.125f;

        if (distance >= 0.02f && distance <= 0.125f)
        {
			currentWaypoint.transform.localScale = new Vector3(0.1875f - distance, 0.1875f - distance, 0.1875f - distance);
        }

        if (resetSize)
        {
            foreach (GameObject point in waypointList)
            {
				point.transform.localScale = new Vector3(0.0625f, 0.0625f, 0.0625f);
            }
        }
    }

	/// <summary>
	/// Exit navigation if the user steps away from the path
	/// 0.5f is roughly one step
	/// </summary>
	/// <returns><c>true</c>, if navigation was exited, <c>false</c> otherwise.</returns>
    public bool ExitNavigation()
    {
        float distance = 999f;
        cameraPos = Camera.main.transform.position;

        if (lastCompletedVectorPath.Count > 0)
        {
            for (int i = 0; i < lastCompletedVectorPath.Count - 1; i++)
            {
                if (Vector3.Distance(cameraPos, lastCompletedVectorPath[i]) < distance)
                {
                    distance = Vector3.Distance(cameraPos, lastCompletedVectorPath[i]);
                }
            }
			if (distance >= 2)
			{
				return true;
			}
        }
		return false;
    }

    /** Draws gizmos for the Seeker */
    public void OnDrawGizmos () {
		if (lastCompletedNodePath == null || !drawGizmos) {
			return;
		}

		if (detailedGizmos) {
			Gizmos.color = new Color(0.7F, 0.5F, 0.1F, 0.5F);

			if (lastCompletedNodePath != null) {
				for (int i = 0; i < lastCompletedNodePath.Count-1; i++) {
					Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i+1].position);
				}
			}
		}

		Gizmos.color = new Color(0, 1F, 0, 1F);

		if (lastCompletedVectorPath != null) {
			for (int i = 0; i < lastCompletedVectorPath.Count-1; i++) {
				Gizmos.DrawLine(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
			}
		}
	}

	/** Handle serialization backwards compatibility */
	void ISerializationCallbackReceiver.OnBeforeSerialize () {
	}

	/** Handle serialization backwards compatibility */
	void ISerializationCallbackReceiver.OnAfterDeserialize () {
		if (traversableTagsCompatibility != null && traversableTagsCompatibility.tagsChange != -1) {
			traversableTags = traversableTagsCompatibility.tagsChange;
			traversableTagsCompatibility = new TagMask(-1, -1);
		}
	}
}
