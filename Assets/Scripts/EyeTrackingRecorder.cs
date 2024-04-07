using System;
using System.IO;
using UnityEngine;
using Tobii.XR;

public class EyeTrackingRecorder : MonoBehaviour
{
    private StreamWriter fileWriter;
    private bool isRecording = true;

    private void Start()
    {
        // 创建文件夹路径
        string directoryPath = Path.Combine(Application.dataPath, "Experiment_Data", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(directoryPath);

        // 打开一个文件用于写入数据
        string filePath = Path.Combine(directoryPath, "GazeData.csv");
        fileWriter = new StreamWriter(filePath, true);
        fileWriter.WriteLine("Timestamp,X,Y");
    }

    private void Update()
    {
        if (isRecording)
        {
            // 检查是否按下了 ESC 键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopRecording();
                return;
            }

            // 获取眼动追踪数据
            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

            // 如果有有效的注视点，则记录数据
            if (eyeTrackingData.GazeRay.IsValid)
            {
                // 转换注视点为屏幕坐标（如果需要）
                Vector3 gazePoint = Camera.main.WorldToScreenPoint(eyeTrackingData.GazeRay.Origin + eyeTrackingData.GazeRay.Direction * 10f); // 示例距离10米

                string dataLine = string.Format("{0},{1},{2}",
                    Time.time, // 使用Unity的时间戳
                    gazePoint.x,
                    gazePoint.y);

                fileWriter.WriteLine(dataLine);
            }
        }
    }

    private void StopRecording()
    {
        // 停止记录数据
        isRecording = false;

        // 关闭并保存文件
        if (fileWriter != null)
        {
            fileWriter.Close();
            fileWriter = null;
        }

        Debug.Log("Recording stopped and data saved.");
    }

    private void OnDestroy()
    {
        // 确保在脚本销毁时关闭文件
        if (fileWriter != null)
        {
            fileWriter.Close();
        }
    }
}
