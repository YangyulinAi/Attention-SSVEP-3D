/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

using System;
using System.Collections;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SSVEPController : MonoBehaviour
{
    //Parameters, setting in Main.cs
    private float frequency = 10f; // Frequency in Hz (Hertz)
    private float switchInterval;

    private GameObject textObject;

    private Image image; // Image component of the panel

    private float lastCheckTime = 0f; // 上一次检查的时间
    private int flashesSinceLastCheck = 0; // 上次检查后的闪烁次数

    private string logFilePath;



    void Start()
    {
        image = GetComponent<Image>(); // Get the Image component
        switchInterval = 1f / (2f * frequency); // Calculate the interval for changing colors

        // 设置日志文件路径，可以根据需要更改文件名或路径
        string folderPath = Application.dataPath + "/Experiment_Data";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 生成当前时间的文件名
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(folderPath, "log_" + timestamp + ".txt");

        // 创建日志文件并写入初始内容
        File.WriteAllText(logFilePath, "Log Start\n");

        // 写入启动时的日志
        Log("Logging started.");

    }

    public IEnumerator SwitchColorCoroutine()
    {
        float nextSwitchTime = Time.unscaledTime + switchInterval; // 计算下一次颜色切换的目标时间

        while (true)
        {
            image.enabled = true;

            // Determine if it's time to switch colors
            if (Time.unscaledTime >= nextSwitchTime)
            {
                // Reset the next switch time to ensure the interval is consistent
                nextSwitchTime += switchInterval;

                // Switch colors
                image.color = (image.color == Color.white) ? Color.black : Color.white;

                flashesSinceLastCheck++; // 记录自上次检查后的闪烁次数
            }

            // 等待下一帧 (一秒÷当前帧率）帧率越高，影响越小
            yield return null;
        }
    }

    void Update()
    {
        // 每秒检查一次
        if (Time.time - lastCheckTime >= 1f)
        {
            // 理论上每秒应该闪烁的次数
            int theoreticalFlashes = Mathf.RoundToInt(frequency);

            if(theoreticalFlashes != flashesSinceLastCheck / 2 && flashesSinceLastCheck != 0)
            {
                // 检查是否达到设定频率
                Debug.Log("Set frequency: " + theoreticalFlashes + "  Actual frequency: " + flashesSinceLastCheck / 2);
                Log("Set frequency: " + theoreticalFlashes + "  Actual frequency: " + flashesSinceLastCheck / 2);
            }
            
            // 更新检查时间和重置闪烁次数
            lastCheckTime = Time.time;
            flashesSinceLastCheck = 0;
        }
    }

    // 记录日志的方法
    public void Log(string message)
    {
        // 使用StreamWriter写入日志
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message);
        }
    }

    //Getter and Setter
    public void SetFrequency(float frequency)
    {
        this.frequency = frequency; // Provides a public method to set the frequency
        switchInterval = 1f / (2f * this.frequency); // Recalculate switchInterval
    }

    public void SetTextObject(GameObject textObject)
    {
        this.textObject = textObject;
    }

    public void OnEnable()
    {
        flashesSinceLastCheck = 0;
    }

}
