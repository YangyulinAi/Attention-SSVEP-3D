using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;

public class AfterEnd : MonoBehaviour
{
    public TextMeshProUGUI hintText;

    // UI
    public Button startButton;
    public Button InstructionButton;
    public Button endButton;

    /// <summary>
    ///  For ViveGazeDataRecorder
    /// </summary>
    private ViveGazeDataRecorder recorder;
    public GameObject gazePointPrefab; // 拖入一个预制体作为注视点的视觉表示
    public Canvas canvas;

    public Image fillImage;

    void Start()
    {
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        RectTransform rectTransform1 = startButton.GetComponent<RectTransform>();
        RectTransform rectTransform2 = InstructionButton.GetComponent<RectTransform>();
        RectTransform rectTransform3 = endButton.GetComponent<RectTransform>();

        recorder = new ViveGazeDataRecorder(gazePointPrefab,fillImage, canvasRectTransform, canvasRectTransform.sizeDelta.x, canvasRectTransform.sizeDelta.y, rectTransform1, rectTransform2, rectTransform3);
    }

    private void Update()
    {

        recorder.EyeTracking();
        startButton.gameObject.SetActive(false);
        InstructionButton.gameObject.SetActive(false);
        endButton.gameObject.SetActive(true);

        if (recorder.CheckGaze("3"))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }

    private void OnDestroy()
    {
        recorder.StopRecording();
    }
}
