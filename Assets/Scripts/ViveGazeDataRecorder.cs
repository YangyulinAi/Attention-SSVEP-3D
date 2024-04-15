using System;
using System.IO;
using UnityEngine.UI;
using System.Threading;
using UnityEngine;
//using UnityEngine.UIElements;
using ViveSR.anipal;
using ViveSR.anipal.Eye;  // SRanipal SDK

public class ViveGazeDataRecorder : MonoBehaviour
{
    public GameObject gazePointPrefab;  // 拖入一个预制体作为注视点的视觉表示
    public RectTransform canvasRectTransform;

    // 三个图片的碰撞体
    public RectTransform leftImageTransform;
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    private StreamWriter fileWriter;
    private Thread dataCollectionThread; // 一个后台线程，用于定期收集和写入眼动追踪数据

    private Camera mainCamera; // 一个引用主摄像机的 Camera 对象，用于屏幕坐标计算
    private bool keepCollecting = true; // 一个布尔值，控制数据收集线程的运行

    private Vector3 gazePosition;
    private int gazeAtLeft, gazeAtMiddle, gazeAtRight;
    private bool gazePositionUpdated = false;
    float leftPupilDiameter;
    float rightPupilDiameter;

    public Image fillImage;  // 拖拽你的进度条Image组件到这里
    public float gazeTime = 5f;  // 需要注视的时间，5秒
    private float timer = 0f;

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
                Vector3 gazeDirection = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
                Vector3 gazeOrigin = eyeData.verbose_data.combined.eye_data.gaze_origin_mm * 0.001f;  // Convert mm to meters
                Vector3 ModifiedGazeDirection = new Vector3(-gazeDirection.x, gazeDirection.y, gazeDirection.z);

                gazePosition = gazeOrigin + ModifiedGazeDirection;

                float scaleFactorX = 350f;
                float scaleFactorY = 175f;

                gazePosition = new Vector3(gazePosition.x * scaleFactorX, gazePosition.y * scaleFactorY, 0);
                gazePointPrefab.transform.localPosition = gazePosition;

                gazePositionUpdated = true;

                // 记录眼睛瞳孔直径信息
                leftPupilDiameter = eyeData.verbose_data.left.pupil_diameter_mm;
                rightPupilDiameter = eyeData.verbose_data.right.pupil_diameter_mm;

                gazeAtLeft = IsGazeInsideRectTransform(gazePosition, leftImageTransform, "Left") ? 1 : 0;
                gazeAtMiddle = IsGazeInsideRectTransform(gazePosition, middleImageTransform, "Middle") ? 1 : 0;
                gazeAtRight = IsGazeInsideRectTransform(gazePosition, rightImageTransform, "Right") ? 1 : 0;

                Debug.Log($"Left Pupil Diameter: {leftPupilDiameter}, Right Pupil Diameter: {rightPupilDiameter}");
               
            }
        }
        else
        {
            if (Input.GetMouseButton(0))  // 当鼠标左键被点击
            {
                // 获取鼠标位置
                Vector3 mouseScreenPosition = Input.mousePosition;
                mouseScreenPosition.z = 500;  // 设置 Z 坐标为 500

                Vector2 offset;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(middleImageTransform, mouseScreenPosition, Camera.main, out offset);
                Debug.Log("Local Offset in Canvas: " + offset);

                gazePosition = new Vector3(offset.x, offset.y, 0);
                gazePointPrefab.transform.localPosition = gazePosition;
                

            }
            else
            {
                gazePosition = new Vector3(0,200,0);
                gazePointPrefab.transform.localPosition = gazePosition;
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
                    "'" + timestamp.ToString(), "'" + str_timestamp, gazePosition.x, gazePosition.y,
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


    private bool IsGazeInsideRectTransform(Vector3 gazeLocalPosition, RectTransform rectTransform, string name)
    {

        Vector3 imageLocalPosition = rectTransform.localPosition;

        bool isInside = false;

        // 检查 gazePosition 是否在 rectTransform 内
        if (gazeLocalPosition.x >= (imageLocalPosition.x - 22.5) && gazeLocalPosition.x <= (imageLocalPosition.x + 22.5))
        {
            if (gazeLocalPosition.y >= (imageLocalPosition.y - 22.5) && gazeLocalPosition.y <= (imageLocalPosition.y + 22.5))
            {
                isInside = true;
            }
        }
      
        // 创建一个用于存储角点坐标的数组
        Vector3[] localCorners = new Vector3[4];
        rectTransform.GetLocalCorners(localCorners);

        return isInside;
    }

    public bool CheckGaze(string name)
    {
        RectTransform rectTransform = middleImageTransform;
        if (name == "left")
        {
            rectTransform = leftImageTransform;
        }
        else if(name == "right") 
        {
            rectTransform = rightImageTransform;
        }


        bool isGazing = IsGazeInsideRectTransform(gazePosition, rectTransform, name);

        if (isGazing)
        {
            // 更新计时器
            timer += Time.unscaledDeltaTime;
            fillImage.fillAmount = timer / gazeTime;  // 更新进度条
            Debug.Log(name + " " + timer);
            if (timer >= gazeTime)
            {
                Debug.Log("Completed!");
                fillImage.fillAmount = 1;
                return true;
            }

            // 这里仅用于调试输出
            // Debug.Log($"Gaze Position: {gazeLocalPosition}, is gazing: {isGazing}");
        }
        else
        {
            // 重置计时器和进度条
            timer = 0;
            fillImage.fillAmount = 0;
            return false;
        }

        return false;
       
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
