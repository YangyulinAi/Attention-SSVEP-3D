using System;
using System.IO;
using System.Threading;
using UnityEngine;
using ViveSR.anipal;
using ViveSR.anipal.Eye;  // SRanipal SDK

public class ViveGazeDataRecorder : MonoBehaviour
{
    // 三个图片的碰撞体
    public RectTransform leftImageTransform;
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    private StreamWriter fileWriter;
    private Thread dataCollectionThread; // 一个后台线程，用于定期收集和写入眼动追踪数据

    private Camera mainCamera; // 一个引用主摄像机的 Camera 对象，用于屏幕坐标计算
    private bool keepCollecting = true; // 一个布尔值，控制数据收集线程的运行

    private Vector2 latestGazePosition;
    private int gazeAtLeft, gazeAtMiddle, gazeAtRight;
    private bool gazePositionUpdated = false;
    float leftPupilDiameter;
    float rightPupilDiameter;

    private void Start()
    {
        // 设置应用程序的目标帧率为 120 FPS
        Application.targetFrameRate = 120; 

        // 文件夹创建
        string directoryPath = Path.Combine(Application.dataPath, "Experiment_Data", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(directoryPath);

        // 打开一个文件用于写入数据
        string filePath = Path.Combine(directoryPath, "GazeDatabyVive.csv");
        fileWriter = new StreamWriter(filePath, true);
        fileWriter.WriteLine("Timestamp UNIX,Timestamp Local, X,Y, Left Pupil Diameter, Right Pupil Diameter, Gaze on Left, Gaze on Middle, Gaze on Right");

        // 初始化一个新的线程来处理数据收集，然后启动这个线程
        dataCollectionThread = new Thread(DataCollectionTask);
        dataCollectionThread.Start();

        // 获取引用主摄像机
        mainCamera = Camera.main;

        // 初始化眼动追踪模块
        if (SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero) != ViveSR.Error.WORK)
        {
            //Debug.LogError("SRanipal Eye initialization failed.");
            Debug.Log("SRanipal Eye initialization failed.");
        }
    }

    private void Update()
    {
        // 首先检查 SRanipal 眼动追踪框架的状态是否为 WORKING。
        // 这确保了只有当眼动追踪硬件正常工作并且SDK正常初始化后，才会执行获取数据的操作
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            EyeData_v2 eyeData = new EyeData_v2();//  EyeData_v2 类型的变量，用来存储获取的眼动数据。EyeData_v2 是存储详细眼动追踪信息的结构体，包括瞳孔直径、注视点坐标等。

            // 通过调用 SRanipal_Eye_API.GetEyeData_v2 方法获取眼动数据，传入 eyeData 作为引用参数。此方法返回 ViveSR.Error 类型的状态值，这里检查是否返回 WORK，表示数据成功获取。
            if (SRanipal_Eye_API.GetEyeData_v2(ref eyeData) == ViveSR.Error.WORK)
            {
                // 计算注视点在世界坐标中的位置。这里使用了眼动数据中的 gaze_origin_mm（注视起点）和 gaze_direction_normalized（规范化的注视方向），通过将方向乘以一个标量（此处为10米）来估算注视点的位置。
                Vector3 gazePointInWorld = eyeData.verbose_data.combined.eye_data.gaze_origin_mm + eyeData.verbose_data.combined.eye_data.gaze_direction_normalized * 10f; //这里的z不影响结果
                latestGazePosition = mainCamera.WorldToScreenPoint(gazePointInWorld);

                gazePositionUpdated = true;
                // 记录眼睛瞳孔直径信息
                leftPupilDiameter = eyeData.verbose_data.left.pupil_diameter_mm;
                rightPupilDiameter = eyeData.verbose_data.right.pupil_diameter_mm;

                gazeAtLeft = IsGazeInsideRectTransform(latestGazePosition, leftImageTransform) ? 1 : 0;
                gazeAtMiddle = IsGazeInsideRectTransform(latestGazePosition, middleImageTransform) ? 1 : 0;
                gazeAtRight = IsGazeInsideRectTransform(latestGazePosition, rightImageTransform) ? 1 : 0;

                Debug.Log($"Left Pupil Diameter: {leftPupilDiameter}, Right Pupil Diameter: {rightPupilDiameter}");
                // 其他眼动数据的处理...
            }
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
                string str_timestamp = ConvertTimestampToDateTime(timestamp);

                // 构建数据行
                string dataLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    "'" + timestamp.ToString(), "'" + str_timestamp, latestGazePosition.x, latestGazePosition.y,
                    leftPupilDiameter, rightPupilDiameter, gazeAtLeft, gazeAtMiddle, gazeAtRight);

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

        // 添加毫秒数来获取UTC时间
        DateTime utcDateTime = epoch.AddMilliseconds(timestamp);

        // 转换UTC时间为悉尼时间
        TimeZoneInfo sydneyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"); // 或者 "E. Australia Standard Time"，具体ID可以通过TimeZoneInfo.GetSystemTimeZones()获取
        DateTime sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, sydneyTimeZone);

        // 转换为标准的日期时间格式字符串，包含毫秒
        return sydneyTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    private bool IsGazeInsideRectTransform(Vector2 gazeScreenPosition, RectTransform rectTransform)
    {
        // 将屏幕坐标转换为Canvas坐标，确保使用正确的Camera参数
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, gazeScreenPosition, mainCamera);
    }

    private void CheckGaze(Vector2 gazePosition)
    {
        bool gazeAtLeft = IsGazeInsideRectTransform(gazePosition, leftImageTransform);
        bool gazeAtMiddle = IsGazeInsideRectTransform(gazePosition, middleImageTransform);
        bool gazeAtRight = IsGazeInsideRectTransform(gazePosition, rightImageTransform);

        // 这里仅用于调试输出
        Debug.Log($"Gaze Position: {gazePosition}, Left: {gazeAtLeft}, Middle: {gazeAtMiddle}, Right: {gazeAtRight}");
    } 
    private void StopRecording()
    {
        // 在对象销毁时，停止数据收集线程并等待其结束
        keepCollecting = false;
        dataCollectionThread?.Join();

        // 关闭 StreamWriter 并释放资源
        if (fileWriter != null)
        {
            fileWriter.Close();
            fileWriter = null;
        }
        // 释放眼动追踪模块资源
        if (SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2) != ViveSR.Error.WORK)
        {
            Debug.LogError("SRanipal Eye release failed.");
        }
        else
            Debug.Log("Recording stopped and data saved.");
    }

    private void OnDestroy()
    {
        StopRecording();
    }

}
