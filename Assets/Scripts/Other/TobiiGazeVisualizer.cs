using UnityEngine;
using Tobii.XR;

public class TobiiGazeVisualizer : MonoBehaviour
{
    public GameObject gazePointPrefab;  // 拖入一个预制体作为注视点的视觉表示
    private Camera mainCamera; // 一个引用主摄像机的 Camera 对象，用于屏幕坐标计算

    //private GameObject gazePointIndicator;

    void Start()
    {
        //gazePointIndicator = Instantiate(gazePointPrefab);
        //gazePointIndicator.GetComponent<Renderer>().material.color = Color.red;
        mainCamera = Camera.main;
    }

    void Update()
    {
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        if (eyeTrackingData.GazeRay.IsValid)
        {
            Vector3 gazePointInWorld = eyeTrackingData.GazeRay.Origin + eyeTrackingData.GazeRay.Direction * -10f; //这里的z不影响结果
            gazePointPrefab.transform.position = mainCamera.WorldToScreenPoint(gazePointInWorld);

            Debug.Log(gazePointPrefab.transform.position);
        }
    }
}