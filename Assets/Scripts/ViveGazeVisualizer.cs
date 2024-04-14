using UnityEngine;
using ViveSR.anipal.Eye;

public class ViveGazeVisualizer : MonoBehaviour
{
    public GameObject gazePointPrefab;  // 拖入一个预制体作为注视点的视觉表示

    void Start()
    {
        //gazePointIndicator = Instantiate(gazePointPrefab);
        //gazePointIndicator.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            EyeData_v2 eyeData = new EyeData_v2();
            if (SRanipal_Eye_API.GetEyeData_v2(ref eyeData) == ViveSR.Error.WORK)
            {
                Vector3 gazeDirection = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
                Vector3 gazeOrigin = eyeData.verbose_data.combined.eye_data.gaze_origin_mm * 0.001f;  // Convert mm to meters
                Vector3 gazePointInWorld = gazeOrigin + gazeDirection * 10; // Assuming 10 meters in front
                Vector2 gazePosition = Camera.main.WorldToScreenPoint(gazePointInWorld);
                gazePointPrefab.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(gazePosition.x, gazePosition.y, 10));
            }
        }
    }
}
