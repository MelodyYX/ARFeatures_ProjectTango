using UnityEngine;
using System.Collections;

public class ARMeasureMarker : MonoBehaviour {

	public int m_type = 0;

	public float m_timestamp = -1.0f;

	public Matrix4x4 m_deviceTMarker = new Matrix4x4();

    /// <summary>
    /// The animation playing.
    /// </summary>
    private Animation m_anim;

    public void Start()
    {
        m_anim = GetComponent<Animation>();
        m_anim.Play("Show", PlayMode.StopAll);
    }

    public void Hide()
    {
        m_anim.Play("Hide", PlayMode.StopAll);
    }

    public void HideDone()
    {
        Destroy(gameObject);
    }
}
