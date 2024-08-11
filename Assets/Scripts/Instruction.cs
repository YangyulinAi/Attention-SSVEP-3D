using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;
using Valve.VR;
using System;

public class Instruction : MonoBehaviour
{
    public Sprite[] sprites;// Array storing the 6 sprites
    public TextMeshProUGUI hintText;

    private float ssvepLeftFrequency = 10f;
    private float ssvepMiddleFrequency = 15f;
    private float ssvepRightFrequency = 20f;

    public int numberMin = 1;
    public int numberMax = 9;
    private float numberShowTime = 3f;
    private float numberHideTimeMax = 3f;

    private int stageInterval = 8;
    private int[] selectedSprites = new int[10]{ 3,2,0,5,4, 3, 2, 0, 5, 4 };
    private int maxRunTimes = 1;

    private TextMeshProUGUI centerNumberText, leftNumberText, rightNumberText;
    private ArrowController arrowController;
    private NumberController numberController;
    private SpriteController spriteController;
    private GameObject[] blocks = new GameObject[3];

    private Image targetImage;

    private bool start = false;
    private bool calibration = false;
    private bool breakStage = false;
    private bool hasUserPressed = true;// Will be set false in Update()

    private int selectedIndex = 0;
    private int currentRunTimes = 0;
    private int stage = 0;

    /// <summary>
    ///  For ViveGazeDataRecorder
    /// </summary>
    private ViveGazeDataRecorder recorder;
    public GameObject gazePointPrefab; // 拖入一个预制体作为注视点的视觉表示
    public Canvas canvas;

    public RectTransform leftImageTransform; // 三个图片的碰撞体
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    public Image fillImage;


    private Coroutine fadeCoroutine; // 用于存储协程的引用
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;
    public float visibleDuration = 0.1f;
    private bool isHintStart = false;

    public SteamVR_Action_Boolean triggerAction = SteamVR_Actions.default_InteractUI;

    private bool seenIntruction = false;
    public Canvas instruction;
    public Sprite[] instruction_sprites;
    private int instruction_index = 0;

    void Start()
    {
        // 查找场景中名为"CenterNumberText"的对象并获取其TextMeshProUGUI组件
        centerNumberText = GameObject.Find("centerNumberText")?.GetComponent<TextMeshProUGUI>();
        leftNumberText = GameObject.Find("leftNumberText")?.GetComponent<TextMeshProUGUI>();
        rightNumberText = GameObject.Find("rightNumberText")?.GetComponent<TextMeshProUGUI>();

        // 检查是否成功获取了所有组件
        if (centerNumberText == null || leftNumberText == null || rightNumberText == null)
        {
            Debug.LogError("<color=red>One or more TextMeshProUGUI objects not found in the scene.</color>");
        }

        GameObject imageObject = GameObject.Find("Image");
        if (imageObject != null)
        {
            targetImage = imageObject.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("<color=red>Image object not found in the scene.</color>");
        }


        numberController = new NumberController(centerNumberText, leftNumberText, rightNumberText);
        spriteController = new SpriteController(sprites, ChangeSpriteInImage);

        // Find each square and set its blinking frequency
        SetSSVEPController("SSVEP Left", ssvepLeftFrequency, "leftNumberText", 0);
        SetSSVEPController("SSVEP Middle", ssvepMiddleFrequency, "centerNumberText", 1);
        SetSSVEPController("SSVEP Right", ssvepRightFrequency, "rightNumberText", 2);

        arrowController = new ArrowController(blocks);

        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

        recorder = new ViveGazeDataRecorder(gazePointPrefab, fillImage, canvasRectTransform, canvasRectTransform.sizeDelta.x, canvasRectTransform.sizeDelta.y, leftImageTransform, middleImageTransform, rightImageTransform, false);
        UpdateDirection("Start");

        canvasGroup.alpha = 0;

    }

    private void Update()
    {

        if (!start)
        {
            if(!seenIntruction)
            {
                canvas.enabled = false;
                instruction.enabled = true;

                Image image = instruction.GetComponent<Image>();
                if (image != null)
                {
                    if (instruction_index < instruction_sprites.Length)  // 确保不会出现数组越界
                    {
                        image.sprite = instruction_sprites[instruction_index];
                    }
                    else
                    {
                        seenIntruction = true;
                    }

                    if(Input.GetKeyDown(KeyCode.Space) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                    {
                        instruction_index++;
                    }
                }
                else
                {
                    Debug.Log("No image has been found");
                }
                
            }
            else
            {
                canvas.enabled = true;
                instruction.enabled = false;

                if (false)   //!calibration)
                {
                    hintText.text = "Eye Gaze calibration\n Please gazing the moving cube";
                    if (recorder.EyeCalibration())
                    {
                        calibration = true;
                    }

                }
                else
                {
                    recorder.EyeTracking();
                    hintText.text = "Welcome to Attention Experiment\n Please move your eye gaze to the blinking area";

                    if (!isHintStart)
                    {
                        canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(0, 35, 0);
                        StartHint();
                    }
                    else
                    {
                        if (recorder.GazeTouchStd2() == "2")
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(0, 0, 0);
                            hintText.text = "Welcome to Attention Experiment\n Please move your eye gaze to the blinking area";
                        }
                        if (recorder.HasGazeOn("2"))
                        {
                            StopHint();
                            hintText.text = "Great!";
                            numberController.HideAllNumbers();
                            StartCoroutine(ChangeStage());
                            start = true;
                            recorder.ResetTimer();
                        }
                    }
                }
            }

        }
        else
        {
            recorder.EyeTracking();

            if ((Input.GetKeyDown(KeyCode.Space) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand)) && !hasUserPressed)
            {
                string key = Input.inputString;
                if (!string.IsNullOrEmpty(key))
                {
                    Debug.Log("<color=#00FF00>User Res</color>");
                }
                hasUserPressed = true;
            }

            if (currentRunTimes >= maxRunTimes)
            {
                currentRunTimes = 0;
                selectedIndex++;
            }

            if (selectedIndex >= selectedSprites.Length)
            {
                SceneManager.LoadScene("Experiment");
            }

            if (Time.timeScale == 0)
            {
                if (stage == 0)
                {                  
                    if (recorder.HasGazeOn("3"))
                    {
                        stage++;
                        hintText.text = "Amazing!";
                        Time.timeScale = 1;
                        recorder.ResetTimer();
                    }
                    else
                    {
                        if (!isHintStart)
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(200, 0, 0);
                            StartHint();
                        }
                        hintText.text = "Right arrow, please move eyes to the right blinking cube.";
                    }
                }
                else if (stage == 1)
                {                   
                    if (recorder.HasGazeOn("1"))
                    {
                        hintText.text = "X symbol, please move eyes to the centre and relax.";
                        Time.timeScale = 1;
                        stage++;
                        recorder.ResetTimer();
                    }
                    else
                    {
                        if (!isHintStart)
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(-200, 0, 0);
                            StartHint();
                        }
                        hintText.text = "Left arrow, please move your eyes to the left blinking cube.";
                    }
                }
                else if (stage == 2)
                {                  
                    if (recorder.HasGazeOn("2"))
                    {
                        hintText.text = "Down arrow, move eyes to the middle cube, and when finish, press the trigger.";
                        if (Input.GetKeyDown(KeyCode.Space) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                        {
                            numberController.SetAllTextToGreen();
                            hintText.text = "Odd number press trigger once, even number press trigger twice.";
                            Time.timeScale = 1;
                            stage++;
                            recorder.ResetTimer();
                        }
                    }
                    else
                    {
                        if (!isHintStart)
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(0, 0, 0);
                            StartHint();
                        }
                        hintText.text = "Down arrow, move eyes to the middle cube, and when finish, press the trigger.";
                    }
                }
                else if (stage == 3)
                {                   
                    if (recorder.HasGazeOn("2"))
                    {
                        hintText.text = "Down right arrow, please keep eyes to the middle cube, and covertly monitoring the number on the right and press the trigger.";
                        if (Input.GetKeyDown(KeyCode.Space) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                        {
                            numberController.SetAllTextToGreen();
                            hintText.text = "Again, Odd number press trigger once, even number press trigger twice.";
                            Time.timeScale = 1;
                            stage++;
                            recorder.ResetTimer();
                        }
                    }
                    else
                    {
                        if (!isHintStart)
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(0, 0, 0);
                            StartHint();
                        }
                        hintText.text = "Down right arrow, please keep eyes to the middle cube, and covertly monitoring the number on the right and press the trigger.";
                    }
                }
                else if (stage == 4)
                {
                    if (recorder.HasGazeOn("2"))
                    {
                        hintText.text = "Down left arrow, please keep eyes to the middle cube, and covertly monitoring the number on the left and press the trigger";
                        if (Input.GetKeyDown(KeyCode.Space) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                        {
                            numberController.SetAllTextToGreen();
                            hintText.text = "Incredible, please practice one more time before we start";
                            Time.timeScale = 1;
                            stage++;
                            recorder.ResetTimer();
                        }
                    }
                    else
                    {
                        if (!isHintStart)
                        {
                            canvasGroup.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector3(0, 0, 0);
                            StartHint();
                        }
                        hintText.text = "Down left arrow, please keep eyes to the middle cube, and covertly monitoring the number on the left and press the trigger";
                    }
                }
                else
                {
                    hintText.text = "Now, please try by youself.";
                    Time.timeScale = 1;
                }

            }
            else
            {
                StopHint();
            }
        } 
    }

    private void StartHint()
    {
        fadeCoroutine = StartCoroutine(FadeRoutine());
        isHintStart = true;
    }

    private void StopHint()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine); // 停止当前协程
            fadeCoroutine = null;
        }
        // 重置透明度或进行其他更新
        canvasGroup.alpha = 0;
        isHintStart = false;
    }
    private IEnumerator FadeRoutine()
    {
        while (true) // 创建一个无限循环
        {
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSecondsRealtime(visibleDuration);
            yield return StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / fadeDuration; // 每帧逐渐增加透明度
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / fadeDuration; // 每帧逐渐减少透明度
            yield return null;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>

    private IEnumerator ChangeStage()
    {
        string direction;
        while (true)
        {

            if (!breakStage)
            {
                hasUserPressed = false;
                direction = spriteController.ChangeSprite(selectedSprites[selectedIndex]);// Get the current sprite's name/direction
                UpdateDirection(direction);
                ShowRandomNumber(direction);
                breakStage = true;
                
            }
            else
            {
                currentRunTimes++;
                if (!hasUserPressed)
                {          
                    hasUserPressed = true;
                }                         
                direction = spriteController.ChangeBreakSprite();// Change to the last sprite (e.g., 'X' sign)
                UpdateDirection(direction);
                breakStage = false;
            }
            
            yield return new WaitForSeconds(stageInterval); // Wait for 5 seconds by default
           
        }
    }

    public void UpdateDirection(string direction)
    {
        arrowController.UpdateDirection(direction);
    }

    public void ShowRandomNumber(string direction)
    {
        StartCoroutine(HideThenShowNumber(direction));
    }


    IEnumerator HideThenShowNumber(string direction)
    {
        /*
         * Hide Time --> Show Time (Start) --> Hide(End)
         */

        // Random hide time, from 1 to max seconds
        float hideTime = UnityEngine.Random.Range(1f, numberHideTimeMax);
        yield return new WaitForSeconds(hideTime);

        // Randomly select a number
        numberController.SetRandomNumber(numberMin, numberMax + 1);

        // Display the number based on the direction
        if (stage >= 2) numberController.ShowRandomNumberInDirection(direction);

        Time.timeScale = 0;

        // Number Showing Time
        yield return new WaitForSeconds(numberShowTime);

        // Hide all numbers
        numberController.HideAllNumbers();
    }


    private void SetSSVEPController(string blockName, float frequency, string textName, int index)
    {
        blocks[index] = GameObject.Find(blockName);
        if (blocks[index] != null)
        {
            SSVEPController controller = blocks[index].GetComponent<SSVEPController>();
            if (controller != null)
            {
                controller.SetFrequency(frequency);
                StartCoroutine(controller.SwitchColorCoroutine());

                //  Find and set the Text object
                GameObject textObject = GameObject.Find(textName);
                if (textObject != null)
                {
                    controller.SetTextObject(textObject);
                }
                else
                {
                    Debug.LogError($"<color=red>Text object named {textName} not found</color>");
                }
            }
            else
            {
                Debug.LogError($"<color=red>SSVEPController not found on {blockName}</color>");
            }
        }
        else
        {
            Debug.LogError($"<color=red>Block named {blockName} not found</color>");
        }
    }

    private void ChangeSpriteInImage(Sprite sprite)
    {
        if (targetImage != null)
        {
            targetImage.sprite = sprite; // 更改Image组件的sprite属性
        }
    }


    private void OnDestroy()
    {
        recorder.StopRecording();
    }
}
