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
            case "4": return "Four";
            case "5": return "Five";
            case "6": return "Six";
        }
        return "null";
    }


    public bool CompareUserInput(string input)
    {
        return input != null && input == randomNumber;
    }

    public void ShowRandomNumberInDirection(string direction)
    {
        switch (direction)
        {
            case "Up":
                centerNumberText.text = randomNumber;
                centerNumberText.gameObject.SetActive(true);
                break;
            case "Left":
            case "Up Left":
                leftNumberText.text = randomNumber;
                leftNumberText.gameObject.SetActive(true);
                break;
            case "Right":
            case "Up Right":
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
}
