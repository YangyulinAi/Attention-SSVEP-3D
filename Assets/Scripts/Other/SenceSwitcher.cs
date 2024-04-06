using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SwitchToScene("Experiment"); 
        }

        // 检测ESC键按下
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitExperiment(); // 终止实验
        }
    }

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitExperiment()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止运行
#else
        Application.Quit(); // 在游戏中退出应用程序
#endif
    }
}
