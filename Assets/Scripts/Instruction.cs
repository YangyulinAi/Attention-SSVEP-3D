using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;

public class Instruction : MonoBehaviour
{
    public Sprite[] sprites;// Array storing the 6 sprites
    public TextMeshProUGUI hintText;

    private float ssvepLeftFrequency = 10f;
    private float ssvepMiddleFrequency = 15f;
    private float ssvepRightFrequency = 20f;

    public int numberMin = 4;
    public int numberMax = 6;
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
    public RectTransform canvasRectTransform;

    public RectTransform leftImageTransform; // 三个图片的碰撞体
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    public Image fillImage;

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
        recorder = new ViveGazeDataRecorder(gazePointPrefab, canvasRectTransform, leftImageTransform, middleImageTransform, rightImageTransform, fillImage);
        UpdateDirection("Start");

    }

    private void Update()
    {

        if (!start)
        {
            if (!calibration)
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
                hintText.text = "Welcome to Attention Experiment\n Please move your eye gaze to the center of the screen";
                if (recorder.CheckGaze("middle"))
                {
                    hintText.text = "Well done!";
                    numberController.HideAllNumbers();
                    StartCoroutine(ChangeStage());
                    start = true;
                }
            }
            
            
        }
        else
        {
            recorder.EyeTracking();

            if ((Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6)) && !hasUserPressed)
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
                    if (recorder.CheckGaze("right"))
                    {
                        stage++;
                        hintText.text = "Good job!";
                        Time.timeScale = 1;
                    }
                    else
                    {
                        hintText.text = "When you see only a right arrow, please move your eye gaze to the right cube";
                    }
                }
                else if (stage == 1)
                {
                    if (recorder.CheckGaze("left"))
                    {
                        hintText.text = "Smart!";
                        Time.timeScale = 1;
                        stage++;
                    }
                    else
                    {
                        hintText.text = "When you see only a left arrow, please move your eye gaze to the left cube";
                    }
                }
                else if (stage == 2)
                {
                    if (recorder.CheckGaze("middle"))
                    {
                        hintText.text = "Excellent!";
                        Time.timeScale = 1;
                        stage++;
                    }
                    else
                    {
                        hintText.text = "When you see only a up arrow, please move your eye gaze to the middle cube";
                    }
                }
                else if (stage == 3)
                {
                    if (recorder.CheckGaze("middle"))
                    {
                        hintText.text = "Please do not move your eye, and covertly monitoring the number and press it!";
                        if ((Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6)))
                        {
                            hintText.text = "Brilliant!";
                            Time.timeScale = 1;
                            stage++;
                        }
                    }
                    else
                    {
                        hintText.text = "When you see up arrow with a right arrow, please keep your eye gaze to the middle cube";
                    }
                }
                else if (stage == 4)
                {
                    if (recorder.CheckGaze("middle"))
                    {
                        hintText.text = "Still do not move your eye, and covertly monitoring the number and press it!";
                        if ((Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6)))
                        {
                            hintText.text = "Incredible! Now, please try by yourself";
                            Time.timeScale = 1;
                            stage++;
                        }
                    }
                    else
                    {
                        hintText.text = "When you see up arrow with a left arrow, please keep your eye gaze to the middle cube";
                    }
                }
                else
                {
                    hintText.text = " ";
                    Time.timeScale = 1;
                }

            }
        } 
    }

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
        float hideTime = Random.Range(1f, numberHideTimeMax);
        yield return new WaitForSeconds(hideTime);

        // Randomly select a number
        numberController.SetRandomNumber(numberMin, numberMax + 1);

        // Display the number based on the direction
        numberController.ShowRandomNumberInDirection(direction);

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
