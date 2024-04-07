using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Tobii.XR;

public class GazeDataRecorder : MonoBehaviour
{
    private StreamWriter fileWriter;
    private Thread dataCollectionThread;
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
        fileWriter.WriteLine("Timestamp UNIX,Timestamp Unity, X,Y ");

        // 启动数据收集线程
        dataCollectionThread = new Thread(DataCollectionTask);
        dataCollectionThread.Start();
    }

    private void DataCollectionTask()
    {
        while (keepCollecting)
        {
            // 获取眼动追踪数据
            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

            if (eyeTrackingData.GazeRay.IsValid)
            {
                // 使用系统时间作为时间戳
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var str_timestamp = ConvertTimestampToDateTime(timestamp);

                // 假设我们直接记录世界坐标数据
                string dataLine = string.Format("{0},{1},{2},{3}",
                    timestamp,
                    str_timestamp,
                    eyeTrackingData.GazeRay.Origin.x,
                    eyeTrackingData.GazeRay.Origin.y
                    );

                // 将数据写入文件（注意：文件写入需要处理线程安全问题）
                lock (fileWriter)
                {
                    fileWriter.WriteLine(dataLine);
                }
            }

            // 等待一段时间，对应于采样率
            Thread.Sleep(1000 / 120); // 120 Hz 采样率
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


    private void StopRecording()
    {
        keepCollecting = false;
        dataCollectionThread?.Join();

        // 关闭并保存文件
        if (fileWriter != null)
        {
            fileWriter.Close();
            fileWriter = null;
        }

        Debug.Log("Recording stopped and data saved.");
    }

    private void Update()
    {
        // 检查是否按下了 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopRecording();
        }
    }

    private void OnDestroy()
    {
        keepCollecting = false;
        dataCollectionThread?.Join();

        // 确保在脚本销毁时关闭文件
        if (fileWriter != null)
        {
            fileWriter.Close();
        }
    }
}
