using UnityEngine;
using ViveSR.anipal.Eye;

public class ViveGazeVisualizer : MonoBehaviour
{
    public GameObject gazePointPrefab;  // 拖入一个预制体作为注视点的视觉表示
    public RectTransform canvasRectTransform;

    void Update()
    {
        Vector3 localPoint;
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            EyeData_v2 eyeData = new EyeData_v2();
            
            if (SRanipal_Eye_API.GetEyeData_v2(ref eyeData) == ViveSR.Error.WORK)
            {
                Vector3 gazeDirection = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
                Vector3 gazeOrigin = eyeData.verbose_data.combined.eye_data.gaze_origin_mm * 0.001f;  // Convert mm to meters
                Vector3 ModifiedGazeDirection = new Vector3(-gazeDirection.x, gazeDirection.y, gazeDirection.z);

                Vector3 gazePointLocal = gazeOrigin + ModifiedGazeDirection;

                // 根据 Canvas 尺寸和实际距离计算映射比例
                float scaleFactorX = 350f;
                float scaleFactorY = 175f;

                //Debug.Log("Gaze Direction:" + gazeDirection);
                //Debug.Log("Gaze Origin:" + gazeOrigin);

                localPoint = new Vector3(gazePointLocal.x * scaleFactorX, gazePointLocal.y * scaleFactorY, 0);
                //Debug.Log("Gaze point:" + localPoint);

                gazePointPrefab.transform.localPosition = localPoint;
            }
        }
       
        Vector3[] wc = new Vector3[4];
        canvasRectTransform.GetLocalCorners(wc);
        //Debug.Log("BottomLeft:" + wc[0] + " TopLeft:" + wc[1] + " TopRight:" + wc[2] + " BottomRight:" + wc[3]);

    }
}