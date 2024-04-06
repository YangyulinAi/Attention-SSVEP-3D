using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshProUGUI
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public Sprite[] sprites;// Array storing the 6 sprites
    

    public float ssvepLeftFrequency = 10f;
    public float ssvepMiddleFrequency = 15f;
    public float ssvepRightFrequency = 20f;

    public int numberMin = 4;
    public int numberMax = 6;
    public float numberShowTime = 3f;
    public float numberHideTimeMax = 3f;

    public string IP = "192.168.1.11";
    public int port = 9999;

    public int stageInterval = 8;
    public int[] selectedSprites = new int[5]{ 3,4,5,0,2};
    public int maxRunTimes = 5;

    private TextMeshProUGUI centerNumberText, leftNumberText, rightNumberText;
    private ArrowController arrowController;
    private NumberController numberController;
    private SpriteController spriteController;
    private MarkerController markerController;
    private GameObject[] blocks = new GameObject[3];

    private Image targetImage;

    private bool start = false;
    private bool breakStage = false;
    private bool userSelection = false;
    private bool hasUserPressed = true;// Will be set false in Update()

    private int selectedIndex = 0;
    private int currentRunTimes = 0;

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

        arrowController = new ArrowController(this);
        numberController = new NumberController(centerNumberText, leftNumberText, rightNumberText);
        spriteController = new SpriteController(sprites, ChangeSpriteInImage);
        markerController = new MarkerController(IP, port);

        // Find each square and set its blinking frequency
        SetSSVEPController("SSVEP Left", ssvepLeftFrequency, "leftNumberText", 0);
        SetSSVEPController("SSVEP Middle", ssvepMiddleFrequency, "centerNumberText", 1);
        SetSSVEPController("SSVEP Right", ssvepRightFrequency, "rightNumberText", 2);

        UpdateDirection("Start");
    }

    private void Update()
    {
        //开始前时候按下空格，开始一个block
        if (Input.GetKeyDown(KeyCode.Space) && !start)
        {
            numberController.HideAllNumbers();
            StartCoroutine(ChangeStage());
            start = true;
        }

        if ((Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6)) && !hasUserPressed)
        {
            string key = Input.inputString;
            if (!string.IsNullOrEmpty(key))
            {
                SendMarker(key);
                SendMarker("UserRes");
                if (numberController.CompareUserInput(key)) SendMarker("True");
                else SendMarker("False");
                Debug.Log("<color=#00FF00>User Res</color>");
            }
            hasUserPressed = true;
        }

        if(currentRunTimes >= maxRunTimes)
        {
            currentRunTimes = 0;
            selectedIndex++;
        }

        if(selectedIndex >= selectedSprites.Length)
        {
            SceneManager.LoadScene("EndOfBlock");
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
                SendMarker("Start"); //encode nullable byte
                Debug.Log("*** Task Stage ***");

                direction = spriteController.ChangeSprite(selectedSprites[selectedIndex]);// Get the current sprite's name/direction

                UpdateDirection(direction);
                SendMarker(direction);

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

    public void SetBlinking(string blockName, bool status)
    {
        
        int index = FindBlock(blockName);

        if (index > -1 && index < 3)
        {
            GameObject block = blocks[index];
            if (block != null)
            {
                block.gameObject.SetActive(status);
            }
        }
        else
            Debug.LogError($"<color=red>Block named {blockName} not found</color>");
    }

    private int FindBlock(string blockName)
    {
        for(int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].name == blockName) return i;
        }
        return -1;
    }

    private void ChangeSpriteInImage(Sprite sprite)
    {
        if (targetImage != null)
        {
            targetImage.sprite = sprite; // 更改Image组件的sprite属性
        }
    }

}
