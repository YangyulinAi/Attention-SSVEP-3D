using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using Valve.VR;

public class Main : MonoBehaviour
{
    /// <summary>
    ///  For SSVEP Controller
    /// </summary>
    public float ssvepLeftFrequency = 10f;
    public float ssvepMiddleFrequency = 15f;
    public float ssvepRightFrequency = 20f;
    private Image targetImage; // Virtual Controller Background

    /// <summary>
    ///  For Merker Controller
    /// </summary>
    private MarkerController markerController;
    public string IP = "192.168.1.11";
    public int port = 9999;

    /// <summary>
    ///  For Sprite Controller
    /// </summary>
    private SpriteController spriteController;
    public Sprite[] sprites;// Array storing the 6 sprites
    public int stageInterval = 8;
    public int[] selectedSprites = new int[5]{ 3,4,5,0,2};
    public int maxRunTimes = 5;
    private int selectedIndex = 0;
    private int currentRunTimes = 0;

    /// <summary>
    ///  For Number Controller
    /// </summary>
    private NumberController numberController;
    private TextMeshProUGUI centerNumberText, leftNumberText, rightNumberText;
    public int numberMin = 1;
    public int numberMax = 9;
    public float numberShowTime = 3f;
    public float numberHideTimeMax = 3f;

    /// <summary>
    ///  For Arrow Controller
    /// </summary>
    private ArrowController arrowController;
    private GameObject[] blocks = new GameObject[3];

    /// <summary>
    ///  For Vive Gaze-Data Recorder
    /// </summary>
    private ViveGazeDataRecorder recorder;
    public GameObject gazePointPrefab;  // 拖入一个预制体作为注视点的视觉表示
    public RectTransform canvasRectTransform;

    public RectTransform leftImageTransform;  // 三个图片的碰撞体
    public RectTransform middleImageTransform;
    public RectTransform rightImageTransform;

    public Image fillImage;

    /// <summary>
    ///  Local Variables
    /// </summary>
    private bool start = false;
    private bool breakStage = true;
    private bool hasUserPressed = true;// Will be set false in Update()

    public SteamVR_Action_Boolean triggerAction = SteamVR_Actions.default_InteractUI;
    private float lastClickTime = 0f;
    public float doubleClickThreshold = 0.3f; // 双击间隔时间阈值
    private bool hasOneClick = false; // 是否已经有一次点击

    void Start()
    {
        SetNumberController();
        SetSpriteController();
        SetMarkerController();

        // Find each square and set its blinking frequency
        SetSSVEPController("SSVEP Left", ssvepLeftFrequency, "leftNumberText", 0);
        SetSSVEPController("SSVEP Middle", ssvepMiddleFrequency, "centerNumberText", 1);
        SetSSVEPController("SSVEP Right", ssvepRightFrequency, "rightNumberText", 2);

        arrowController = new ArrowController(blocks);
        recorder = new ViveGazeDataRecorder(gazePointPrefab,fillImage, canvasRectTransform, canvasRectTransform.sizeDelta.x, canvasRectTransform.sizeDelta.y, leftImageTransform, middleImageTransform, rightImageTransform, true);

        gazePointPrefab.gameObject.SetActive(false);

        UpdateDirection("Start");

    }

    private void Update()
    {
        recorder.EyeTracking();

        // 开始前是否按下空格，开始一个block
        if (Input.GetKeyDown(KeyCode.Space) && !start)
        {
            numberController.HideAllNumbers();
            StartCoroutine(ChangeStage());
            start = true;
        }

        // Handle user input
        if (!hasUserPressed && UserInputDetected())
        {
            HandleUserInput();
        }

        if (currentRunTimes >= maxRunTimes)
        {
            currentRunTimes = 0;
            selectedIndex++;

            if (selectedIndex >= selectedSprites.Length)
            {
                recorder.StopRecording();
                SceneManager.LoadScene("End");
            }
        }

        
    }
    private bool UserInputDetected()
    {
        // 检测输入设备上的输入事件，如按键、点击等
        if (Input.GetMouseButtonDown(0) || triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            return true;
        }
        return false;
    }

    private void SetSpriteController()
    {
        GameObject imageObject = GameObject.Find("Image");
        if (imageObject != null)
        {
            targetImage = imageObject.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("<color=red>Image object not found in the scene.</color>");
        }


        spriteController = new SpriteController(sprites, ChangeSpriteInImage);
    }

    private void SetNumberController()
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

        numberController = new NumberController(centerNumberText, leftNumberText, rightNumberText);
    }

    private void SetMarkerController()
    {
        markerController = new MarkerController(IP, port);
    }

    private void SetSSVEPController(string blockName, float frequency, string textName, int index)
    {
        blocks[index] = GameObject.Find(blockName); // GameObject[]
        if (blocks[index] != null)
        {
            SSVEPController controller = blocks[index].GetComponent<SSVEPController>();
            if (controller != null)
            {
                controller.SetFrequency(frequency);
 
                // StartCoroutine(controller.SwitchColorCoroutine());
                // Find and set the Text object
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


    private void HandleUserInput()
    {
        string userAction = "Not Clicked";

        if (recorder.CheckConnection())
        {
            userAction = CheckTriggerClick();
        }
        else
        {
            userAction = CheckMouseClick();
        }

        if (userAction != "Not Clicked")
        {
            SendMarker("UserRes");
            string key = userAction == "single" ? "1" : "0";

            SendMarker(key);

            if (numberController.CompareUserInput(key))
            {
                SendMarker("True");
            }
            else
            {
                SendMarker("False");
            }

            Debug.Log("<color=#00FF00>User Res</color>");
            hasUserPressed = true;
        }
    }



    private string CheckTriggerClick()
    {
        string action = "Not Clicked";
        if (triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand)) //.Any
        {
            if (!hasOneClick)
            {
                // 如果当前没有一次点击的记录
                hasOneClick = true;
                lastClickTime = Time.time;
            }
            else if (Time.time - lastClickTime < doubleClickThreshold)
            {
                // 如果已经有一次点击，且第二次点击在规定时间内
                Debug.Log("Double Click Detected");
                action = "double";
                hasOneClick = false; // 重置点击状态
            }
            else
            {
                // 如果第二次点击超出了规定时间
                lastClickTime = Time.time; // 重置最后点击时间
            }
        }
        else if (hasOneClick && (Time.time - lastClickTime > doubleClickThreshold))
        {
            // 如果只有一次点击，且时间已经超过了双击的阈值
            Debug.Log("Single Click Detected");
            action = "single";
            hasOneClick = false; // 重置点击状态
        }

        return action;
    }

    private string CheckMouseClick()
    {
        string action = "Not Clicked";

        if (Input.GetMouseButtonDown(0)) // 检测鼠标左键按下
        {
            if (!hasOneClick)
            {
                // 如果当前没有一次点击的记录
                hasOneClick = true;
                lastClickTime = Time.time;
            }
            else if (Time.time - lastClickTime < doubleClickThreshold)
            {
                // 如果已经有一次点击，且第二次点击在规定时间内
                Debug.Log("Double Click Detected");
                action = "double";
                hasOneClick = false; // 重置点击状态
            }
            else
            {
                // 如果第二次点击超出了规定时间
                lastClickTime = Time.time; // 重置最后点击时间
            }
        }
        else if (hasOneClick && (Time.time - lastClickTime > doubleClickThreshold))
        {
            // 如果只有一次点击，且时间已经超过了双击的阈值
            Debug.Log("Single Click Detected");
            action = "single";
            hasOneClick = false; // 重置点击状态
        }

        return action;
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
                
                SendMarker("Start"); //encode nullable byte
                SendMarker(direction);
                Debug.Log("*** Task Stage ***");

                // Call ShowRandomNumber using the current sprite's name as a parameter
                ShowRandomNumber(direction); 
                breakStage = true;
                
            }
            else
            {
                currentRunTimes++;
                if (!hasUserPressed)
                {
                    SendMarker("UserNotRes");
                    hasUserPressed = true;
                }               
                SendMarker("End"); //encode nullable byte
                Debug.Log("*** Break Stage ***");
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

        SendMarker(numberController.GetRandomNumber());

        // Display the number based on the direction
        numberController.ShowRandomNumberInDirection(direction);

        // Number Showing Time
        yield return new WaitForSeconds(numberShowTime);

        // Hide all numbers
        numberController.HideAllNumbers();
    }


    


    private void SendMarker(string marker)
    {
        if (markerController.SendMarker(marker))
        {
            //Debug.Log(marker);
        }
        else
        {
            Debug.LogError($"<color=red>Marker {marker} has failed to be sent</color>");
            Application.Quit();
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
