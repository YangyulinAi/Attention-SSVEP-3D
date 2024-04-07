using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Tobii.XR;

public class GazeDataRecorder : MonoBehaviour
{
    public RectTransform leftImageTransform;
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    private StreamWriter fileWriter;
    private Thread dataCollectionThread;

    private Camera mainCamera;
    private Vector2 latestGazePosition;
    private int gazeAtLeft, gazeAtMiddle, gazeAtRight;
    private bool gazePositionUpdated = false;
    private bool keepCollecting = true;

    private void Start()
    {
        // 设置Unity目标帧率为120 FPS以匹配Vive Pro Eye的采样率
        Application.targetFrameRate = 120; 

        // 创建文件夹路径
        string directoryPath = Path.Combine(Application.dataPath, "Experiment_Data", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(directoryPath);

        // 打开一个文件用于写入数据
        string filePath = Path.Combine(directoryPath, "GazeData.csv");
        fileWriter = new StreamWriter(filePath, true);
        fileWriter.WriteLine("Timestamp UNIX,Timestamp Local, X,Y, Gaze on Left, Gaze on Middle, Gaze on Right");

        // 启动数据收集线程
        dataCollectionThread = new Thread(DataCollectionTask);
        dataCollectionThread.Start();

        mainCamera = Camera.main;
    }

    private void Update()
    {
        // 获取眼动追踪数据
        //var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local); // 或直接使用 TobiiXR.GetEyeTrackingData() 如果不需要指定跟踪空间
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);


        if (eyeTrackingData.GazeRay.IsValid)
        {
            // 假设注视点在眼睛前10米的直线上
            Vector3 gazePointInWorld = eyeTrackingData.GazeRay.Origin + eyeTrackingData.GazeRay.Direction * 10f; //这里的z不影响结果
            latestGazePosition = mainCamera.WorldToScreenPoint(gazePointInWorld);

            gazePositionUpdated = true;

            // 计算是否注视每个图片
            gazeAtLeft = IsGazeInsideRectTransform(latestGazePosition, leftImageTransform) ? 1 : 0;
            gazeAtMiddle = IsGazeInsideRectTransform(latestGazePosition, middleImageTransform) ? 1 : 0;
            gazeAtRight = IsGazeInsideRectTransform(latestGazePosition, rightImageTransform) ? 1 : 0;
        }

        // ESC键停止记录
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopRecording();
        }
    }

    private void DataCollectionTask()
    {
        while (keepCollecting)
        {
            if (gazePositionUpdated)
            {
                // 使用系统时间作为时间戳
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var str_timestamp = ConvertTimestampToDateTime(timestamp);

                // 构建数据行
                string dataLine = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    timestamp, str_timestamp, latestGazePosition.x, latestGazePosition.y,
                    gazeAtLeft, gazeAtMiddle, gazeAtRight);

                lock (fileWriter)
                {
                    fileWriter.WriteLine(dataLine);
                }

                gazePositionUpdated = false;
            }

            Thread.Sleep(1000 / 120); // 适应采样率
        }
    }

    private string ConvertTimestampToDateTime(long timestamp)
    {
        // 创建一个从Unix纪元开始的DateTime
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 添加毫秒数来获取正确的时间
        DateTime targetTime = epoch.AddMilliseconds(timestamp);

        // 转换为标准的日期时间格式字符串，包含毫秒
        return targetTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    private void CheckGaze(Vector2 gazePosition)
    {
        bool gazeAtLeft = IsGazeInsideRectTransform(gazePosition, leftImageTransform);
        bool gazeAtMiddle = IsGazeInsideRectTransform(gazePosition, middleImageTransform);
        bool gazeAtRight = IsGazeInsideRectTransform(gazePosition, rightImageTransform);

        // 这里仅用于调试输出
        Debug.Log($"Gaze Position: {gazePosition}, Left: {gazeAtLeft}, Middle: {gazeAtMiddle}, Right: {gazeAtRight}");
    }

    private bool IsGazeInsideRectTransform(Vector2 gazePosition, RectTransform rectTransform)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, gazePosition, Camera.main);
    }

    private void StopRecording()
    {
        keepCollecting = false;
        dataCollectionThread?.Join();

        if (fileWriter != null)
        {
            fileWriter.Close();
            fileWriter = null;
        }

        Debug.Log("Recording stopped and data saved.");
    }

    private void OnDestroy()
    {
        keepCollecting = false;
        dataCollectionThread?.Join();

        if (fileWriter != null)
        {
            fileWriter.Close();
        }
    }
}
