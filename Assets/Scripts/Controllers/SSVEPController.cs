/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SSVEPController : MonoBehaviour
{
    // Parameters, setting in Main.cs
    private float frequency = 10f; // Frequency in Hz (Hertz)
    private float switchInterval;

    private GameObject textObject;

    private Image image; // Image component of the panel

    private double lastCheckTime = 0f; // 上一次检查的时间
    private int flashesSinceLastCheck = 0; // 上次检查后的闪烁次数

    private string logFilePath;

    private List<string> logBuffer = new List<string>();

    private Stopwatch stopwatch = new Stopwatch();
    private double lastSwitchTime = 0f;

    private Color color1 = Color.white;
    private Color color2 = Color.black;
    private bool isColor1 = true;


    void Awake()
    {

    }

    void OnEnable()
    {
        stopwatch.Start();
        flashesSinceLastCheck = 0;
        //lastCheckTime = stopwatch.Elapsed.TotalSeconds;
    }

    void Start()
    {
        Time.fixedDeltaTime = 0.005f; // 设置FixedUpdate的调用间隔为5ms

        image = GetComponent<Image>(); // Get the Image component
        switchInterval = 1.0f / (2.0f * frequency); // Calculate the interval for changing colors

        // 设置日志文件路径，可以根据需要更改文件名或路径
        string folderPath = Application.dataPath + "/../Experiment_Data";

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

    void OnDisable()
    {
        stopwatch.Stop();
    }


    void FixedUpdate()
    {
        double elapsedTime = stopwatch.Elapsed.TotalSeconds;

        if (elapsedTime - lastSwitchTime >= switchInterval)
        {
            // 切换颜色
            image.color = isColor1 ? color1 : color2;
            isColor1 = !isColor1;

            lastSwitchTime += switchInterval;
            flashesSinceLastCheck++;
        }

        // 每秒检查一次频率（与 Update 中相同）
    }

    void LateUpdate()
    {
        if (logBuffer.Count > 0)
        {
            var logsToWrite = new List<string>(logBuffer);
            logBuffer.Clear();

            // 异步写入日志，避免阻塞主线程
            System.Threading.Tasks.Task.Run(() => File.AppendAllLines(logFilePath, logsToWrite));
        }
    }

    void Update()
    {

        double elapsedTime = stopwatch.Elapsed.TotalSeconds;

        if (elapsedTime - lastCheckTime >= 1.0)
        {
            // 理论上每秒应该闪烁的次数
            double theoreticalFlashes = frequency * 2.0; // 每秒颜色切换次数

            // 实际闪烁次数
            int actualFlashes = flashesSinceLastCheck;

            // 计算实际频率
            double actualFrequency = (actualFlashes / 2.0) / (elapsedTime - lastCheckTime);

            // 允许的误差范围（可根据需要调整）
            double tolerance = 0.01 * frequency; // 1% 的误差

            if (Mathf.Abs((float)actualFrequency - frequency) > 2 * tolerance)
            {
                // 闪烁频率不在容忍范围内
                Log($"Set frequency: {frequency:F2}Hz  Actual frequency: {actualFrequency:F2}Hz");
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"<color=red>Set frequency: {frequency:F2}Hz  Actual frequency: {actualFrequency:F2}Hz</color>");
#endif
            }
            else if(Mathf.Abs((float)actualFrequency - frequency) >  tolerance)
            {
                // 闪烁频率在2倍容忍范围内
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"<color=yellow>Set frequency: {frequency:F2}Hz  Actual frequency: {actualFrequency:F2}Hz</color>");
#endif
            }
            else
            {
                UnityEngine.Debug.Log($"<color=green>Set frequency: {frequency:F2}Hz  Actual frequency: {actualFrequency:F2}Hz</color>");

            }

            // 更新检查时间和重置闪烁次数
            lastCheckTime = elapsedTime;
            flashesSinceLastCheck = 0;
        }
    }


    public void Log(string message)
    {
        string logEntry = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message;
        logBuffer.Add(logEntry);
    }

    // Getter and Setter
    public void SetFrequency(float frequency)
    {
        this.frequency = frequency; // Provides a public method to set the frequency
        switchInterval = 1f / (2f * this.frequency); // Recalculate switchInterval
    }

    public void SetTextObject(GameObject textObject)
    {
        this.textObject = textObject;
    }

   

}
