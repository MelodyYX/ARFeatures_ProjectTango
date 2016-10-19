using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Tango;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// GUI controller controls all the debug overlay to show the data for poses.
/// </summary>
public class FeaturesGUIController : MonoBehaviour
{
	// Constant value for controlling the position and size of debug overlay.
	public GUISkin MyGUISkin;
	public Texture icon_eraser;
	public Texture icon_measure;
	public Texture icon_pen;
	public Texture icon_screenshot;
	public Texture screenshot;
	public GameObject m_prefabLocation1;
	public GameObject screenshotPic;
	public TangoPointCloud m_pointCloud;
	private List<GameObject> m_Locations = new List<GameObject> ();
	private List<GameObject> m_Lines = new List<GameObject>();

	private Rect buttonArea;
	private Rect showTextArea; 
	private Rect button_Pen; 
	private Rect button_measure; 
	private Rect button_screenshot; 

	private bool toolsBar;
	private List<Vector3> penDrawPos= new List<Vector3>();
	private List<GameObject> penDraw = new List<GameObject>();
	private Vector3 penDrawPos0;
	private int functionMode;
	private string myScreenshotName;
	private int i=1;
	private Quaternion quat2;

	public const float BUTTON_GAP=10.0f;
	public const float TOOLS_SIZE_X = 300.0f;
	public const float TOOLS_SIZE_Y = 80.0f;
	public const float UI_ICON_SIZE = 160.0f;
	public const float SECOND_TO_MILLISECOND = 1000.0f;
	public const string UI_FLOAT_FORMAT = "F3";

	private const int TAP_PIXEL_TOLERANCE = 40;
    private const float FPS_UPDATE_FREQUENCY = 1.0f;
    private string m_fpsText;
    private int m_currentFPS;
    private int m_framesSinceUpdate;
    private float m_accumulation;
    private float m_currentTime;

    private TangoApplication m_tangoApplication;
    private TangoARPoseController m_tangoPose;
    private string m_tangoServiceVersion;
    private ARCameraPostProcess m_arCameraPostProcess;
    private Rect Eraser;

	//Unity Start() callback, we set up some initial values here.
    public void Start()
    {
        m_currentFPS = 0;
        m_framesSinceUpdate = 0;
        m_currentTime = 0.0f;
        m_fpsText = "FPS = Calculating";

        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_tangoPose = FindObjectOfType<TangoARPoseController>();
        m_arCameraPostProcess = FindObjectOfType<ARCameraPostProcess>();
        m_tangoServiceVersion = TangoApplication.GetTangoServiceVersion();
		 

		//show screenshot icon after a screenshot
		screenshotPic = GameObject.Find ("screenshotPic");
		this.screenshotPic.SetActive(false);
    }
		
	// Display GUI.
	public void OnGUI()
	{
		GUI.skin = MyGUISkin;

		//Define the buttton area to avoid conflict with drawing and click.
		buttonArea= new Rect((Screen.width - TOOLS_SIZE_X - BUTTON_GAP), BUTTON_GAP, TOOLS_SIZE_X, Screen.height);

		// Showing or hide the tools
		if (GUI.Button (new Rect ((Screen.width - TOOLS_SIZE_X - BUTTON_GAP), BUTTON_GAP, TOOLS_SIZE_X, TOOLS_SIZE_Y), (toolsBar) ? "Hide Tools":"Show Tools")) {
		toolsBar=toolsBar==false?true:false;
		}
		if (toolsBar) {
		    // Click this button to enter free-hand drawing mode
			button_Pen = new Rect((Screen.width- UI_ICON_SIZE -BUTTON_GAP), TOOLS_SIZE_Y+BUTTON_GAP*2, UI_ICON_SIZE, UI_ICON_SIZE);
			if (GUI.Button (button_Pen, icon_pen)) {
				functionMode=1;
			}

			// Click this button to enter measurement mode
			button_measure = new Rect((Screen.width - UI_ICON_SIZE - BUTTON_GAP), TOOLS_SIZE_Y+UI_ICON_SIZE+BUTTON_GAP*3, UI_ICON_SIZE, UI_ICON_SIZE);
			if (GUI.Button (button_measure, icon_measure)) {
				functionMode=2;		
			}

			//Click this button to take a screenshot
			button_screenshot = new Rect((Screen.width - UI_ICON_SIZE - BUTTON_GAP),TOOLS_SIZE_Y+UI_ICON_SIZE*2+BUTTON_GAP*4, UI_ICON_SIZE, UI_ICON_SIZE);
			if(GUI.Button(button_screenshot,icon_screenshot)){
				functionMode=3;
				CaptureByTango();
				StartCoroutine(WaitDesappear());
			}
			
			//Eraser function: Hide all
			Rect Eraser = new Rect((Screen.width - UI_ICON_SIZE - BUTTON_GAP), TOOLS_SIZE_Y+UI_ICON_SIZE*3+BUTTON_GAP*5, UI_ICON_SIZE, UI_ICON_SIZE);
			if (GUI.Button(Eraser,icon_eraser))
			{

				if(functionMode==1){
					foreach (GameObject DrawLine in penDraw){
						Destroy(DrawLine);
					}
				}

				else if(functionMode==2){
					foreach (GameObject MeasrMarker in m_Locations){
						Destroy(MeasrMarker);
					}
					foreach(GameObject lineAndText in m_Lines){
						
						Destroy(lineAndText);
					}				
				}

				else{
					foreach (ARLocationMarker marker in GameObject.FindObjectsOfType<ARLocationMarker>()){
						marker.SendMessage("Hide");
					}
					foreach (GameObject MesMarker in m_Locations){
						Destroy(MesMarker);
					}
					foreach(GameObject lineAndText in m_Lines){	
						Destroy(lineAndText);
					}
					foreach (GameObject DrawLine in penDraw){
						Destroy(DrawLine);
					}
					
				}
				
			}
			//Eraser function- end
		}

	}

	//Updates UI and handles player input.
	public void Update()
	{
		m_currentTime += Time.deltaTime;
		++m_framesSinceUpdate;
		m_accumulation += Time.timeScale / Time.deltaTime;
		if (m_currentTime >= FPS_UPDATE_FREQUENCY)
		{
			m_currentFPS = (int)(m_accumulation / m_framesSinceUpdate);
			m_currentTime = 0.0f;
			m_framesSinceUpdate = 0;
			m_accumulation = 0.0f;
			m_fpsText = "FPS: " + m_currentFPS;
		}

		if(functionMode==1){
			DrawOnScreenFunction();
		}
		if(functionMode==2){
			MeasurementFunction();
		}
			
	}

	// Free hand drawing -begin
	public void DrawOnScreenFunction(){

		if (Input.touchCount == 1) {
			Touch Pen = Input.GetTouch (0);			  
			Vector2 guiPosition = new Vector2 (Pen.position.x, Screen.height - Pen.position.y);
			Camera cam = Camera.main;
			if (showTextArea.Contains (guiPosition) || buttonArea.Contains (guiPosition)) {
				return;
			}

			int closestIndex = m_pointCloud.FindClosestPoint (cam, Pen.position, TAP_PIXEL_TOLERANCE);
			if (closestIndex < 0) {
				return;
			}
			float closestDepth = cam.WorldToScreenPoint (m_pointCloud.m_points [closestIndex]).z;
			Ray touchRay = cam.ScreenPointToRay (new Vector3 (Pen.position [0], Pen.position [1], 0));
			penDrawPos0 = touchRay.origin + (touchRay.direction * closestDepth);

			if (Pen.phase == TouchPhase.Moved) {
				penDrawPos.Add (penDrawPos0);

				if (penDrawPos.Count >= 2) {
					Vector3 penPos1 = penDrawPos[penDrawPos.Count - 1];
					Vector3 penPos2 = penDrawPos[penDrawPos.Count - 2];

					GameObject lineMaterial = (GameObject)Instantiate (GameObject.Find ("Line"));
					var lineRendered = lineMaterial.GetComponent<LineRenderer> ();
					lineRendered.SetPosition (0, penPos1);
					lineRendered.SetPosition (1, penPos2);
					penDraw.Add (lineMaterial);
				}
			}

		} else {
			penDrawPos.Clear();
		}	
	}
	//Free hand drawing -end

	// Measurement Function
	public void MeasurementFunction()
	{
		if (Input.touchCount != 1)
		{
			return;
		}

		Touch T = Input.GetTouch(0);
		Vector2 guiPosition = new Vector2(T.position.x, Screen.height - T.position.y);
		Camera cam = Camera.main;

		if (T.phase != TouchPhase.Began)
		{
			return;
		}
		if (showTextArea.Contains(guiPosition)|| buttonArea.Contains(guiPosition) )
		{
			return;
		}

		int closestIndex = m_pointCloud.FindClosestPoint(cam, T.position, TAP_PIXEL_TOLERANCE);
		if (closestIndex < 0)
		{
			return;
		}

		float closestDepth = cam.WorldToScreenPoint(m_pointCloud.m_points[closestIndex]).z;
		Ray touchRay = cam.ScreenPointToRay(new Vector3(T.position[0], T.position[1], 0));
		Vector3 pos = touchRay.origin + (touchRay.direction * closestDepth);
		Vector3 rot = cam.transform.eulerAngles;
		rot[0] = rot[2] = 0;

		var newLocation = (GameObject)Instantiate(m_prefabLocation1, pos, Quaternion.Euler(rot));
		m_Locations.Add (newLocation);

		if (m_Locations.Count >= 2) {
			GameObject placedLocation1 = m_Locations[m_Locations.Count-1];
			GameObject placedLocation2 = m_Locations[m_Locations.Count-2];

			Vector3 p1 = placedLocation1.transform.position;
			Vector3 p2 = placedLocation2.transform.position;

			GameObject lineObj = (GameObject) Instantiate(GameObject.Find ("Line"));
			var lineRendered = lineObj.GetComponent<LineRenderer> ();
			lineRendered.SetPosition(0, p1);
			lineRendered.SetPosition(1, p2);
			m_Lines.Add(lineObj);

			var texture = lineRendered.transform.Find ("DistanceText").GetComponent<TextMesh> ();

			var dist = Vector3.Distance(p2, p1);
			texture.text = string.Format("{0}m", Mathf.Round(dist * 100f) / 100f);

			var midPoint = (p2 - p1) * .5f + p1;
			texture.transform.position = midPoint + new Vector3(0, 0.04f, 0);		
			texture.transform.LookAt(Camera.main.transform);
			texture.transform.Rotate(Vector3.up - new Vector3(0,180,0));	
		}
	}
	// measurment function -end


	//Screenshot function. Capture the current screen and save as png. Pictures path: /Android/data/com.Company.trAR/files/
	public void CaptureByTango()
	{    
		myScreenshotName = "screenshot" + i + ".png";
		Application.CaptureScreenshot(myScreenshotName);	
		i++;
		return; 
	}

	// Delay: Screenshot icon appears then desapear after 1 seconds.
	IEnumerator WaitDesappear(){  	
		yield return new WaitForSeconds(1);
		this.screenshotPic.SetActive(true);
		yield return new WaitForSeconds(1.5f); 
		this.screenshotPic.SetActive(false);
	}
	//Screenshot function -end
    
    // Convert a 3D bounding box into a 2D Rect.
    // <returns>The 2D Rect in Screen coordinates.</returns>
    // <param name="cam">Camera to use.</param>
    // <param name="bounds">3D bounding box.</param>
    private Rect WorldBoundsToScreen(Camera cam, Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        Bounds screenBounds = new Bounds(cam.WorldToScreenPoint(center), Vector3.zero);

        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, -extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, +extents.z)));
        screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)));
        return Rect.MinMaxRect(screenBounds.min.x, screenBounds.min.y, screenBounds.max.x, screenBounds.max.y);
    }
		
	// Construct readable string from TangoPoseStatusType.
    // <param name="status">Pose status from Tango.</param>
    // <returns>Readable string corresponding to status.</returns>
    private string _GetLoggingStringFromPoseStatus(TangoEnums.TangoPoseStatusType status)
    {
        string statusString;
        switch (status)
        {
        case TangoEnums.TangoPoseStatusType.TANGO_POSE_INITIALIZING:
            statusString = "initializing";
            break;
        case TangoEnums.TangoPoseStatusType.TANGO_POSE_INVALID:
            statusString = "invalid";
            break;
        case TangoEnums.TangoPoseStatusType.TANGO_POSE_UNKNOWN:
            statusString = "unknown";
            break;
        case TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID:
            statusString = "valid";
            break;
        default:
            statusString = "N/A";
            break;
        }
        return statusString;
    }

	// Reformat string from vector3 type for data logging.
    // <param name="vec">Position to display.</param>
    // <returns>Readable string corresponding to vec.</returns>
    private string _GetLoggingStringFromVec3(Vector3 vec)
    {
        if (vec == Vector3.zero)
        {
            return "N/A";
        }
        else
        {
            return string.Format("{0}, {1}, {2}",
                                 vec.x.ToString(UI_FLOAT_FORMAT),
                                 vec.y.ToString(UI_FLOAT_FORMAT),
                                 vec.z.ToString(UI_FLOAT_FORMAT));
        }
    }

	//  Reformat string from quaternion type for data logging.
    // <param name="quat">Quaternion to display.</param>
    // <returns>Readable string corresponding to quat.</returns>
    private string _GetLoggingStringFromQuaternion(Quaternion quat)
    {
        if (quat == Quaternion.identity)
        {
            return "N/A";
        }
        else
        {
            return string.Format("{0}, {1}, {2}, {3}",
                                 quat.x.ToString(UI_FLOAT_FORMAT),
                                 quat.y.ToString(UI_FLOAT_FORMAT),
                                 quat.z.ToString(UI_FLOAT_FORMAT),
                                 quat.w.ToString(UI_FLOAT_FORMAT));
        }
    }

	// Return a string to the get logging from frame count.
    // <returns>The get logging string from frame count.</returns>
    // <param name="frameCount">Frame count.</param>
    private string _GetLoggingStringFromFrameCount(int frameCount)
    {
        if (frameCount == -1.0)
        {
            return "N/A";
        }
        else
        {
            return frameCount.ToString();
        }
    }

	// Return a string to get logging of FrameDeltaTime.
    // <returns>The get loggin string from frame delta time.</returns>
    // <param name="frameDeltaTime">Frame delta time.</param>
    private string _GetLogginStringFromFrameDeltaTime(float frameDeltaTime)
    {
        if (frameDeltaTime == -1.0)
        {
            return "N/A";
        }
        else
        {
            return (frameDeltaTime * SECOND_TO_MILLISECOND).ToString(UI_FLOAT_FORMAT);
        }
    }
}
