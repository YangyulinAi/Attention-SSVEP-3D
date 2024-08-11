/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

using System.Collections;
using UnityEngine;
using TMPro;

public class NumberController
{
    // Purpose: Control the random number, delay, etc., for the Text

    private TextMeshProUGUI centerNumberText; // TextMeshPro component for the center square
    private TextMeshProUGUI leftNumberText;   // TextMeshPro component for the left square
    private TextMeshProUGUI rightNumberText;  // TextMeshPro component for the right square

    private string randomNumber = "0";

    public NumberController(TextMeshProUGUI centerText, TextMeshProUGUI leftText, TextMeshProUGUI rightText)
    {
        centerNumberText = centerText;
        leftNumberText = leftText;
        rightNumberText = rightText;
    }

    public void SetRandomNumber(int min, int max)
    {
        this.randomNumber = Random.Range(min, max).ToString();
    }

    public string GetRandomNumber()
    {
        switch (this.randomNumber)
        {
            case "1": return "One";
            case "2": return "Two";
            case "3": return "Three";
            case "4": return "Four";
            case "5": return "Five";
            case "6": return "Six";
            case "7": return "Seven";
            case "8": return "Eight";
            case "9": return "Nine";
        }
        return "null";
    }


    public bool CompareUserInput(string input)
    {
        if(input != null)
        {
            if(int.Parse(randomNumber)%2 == int.Parse(input)) //假设奇数按，那么相等情况下就是奇数
            {
                return true;
            }
        }
        return false;
    }

    public void ShowRandomNumberInDirection(string direction)
    {
        switch (direction)
        {
            case "Up":
                centerNumberText.color = Color.red;
                centerNumberText.text = randomNumber;
                centerNumberText.gameObject.SetActive(true);
                break;
            case "Left":
            case "Up Left":
                leftNumberText.color = Color.red;
                leftNumberText.text = randomNumber;
                leftNumberText.gameObject.SetActive(true);
                break;
            case "Right":
            case "Up Right":
                rightNumberText.color = Color.red;
                rightNumberText.text = randomNumber;
                rightNumberText.gameObject.SetActive(true);
                break;
        }
    }

    public void HideAllNumbers()
    {
        centerNumberText.gameObject.SetActive(false);
        leftNumberText.gameObject.SetActive(false);
        rightNumberText.gameObject.SetActive(false);
    }

    public void SetAllTextToGreen()
    {
        centerNumberText.color = Color.green;
        leftNumberText.color = Color.green;
         rightNumberText.color = Color.green;
    }
}
