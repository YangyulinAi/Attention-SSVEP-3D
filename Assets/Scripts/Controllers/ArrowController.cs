/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

/**
 * Dependency Injection
 * 
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowController
{
    // Purpose: Blink certain blocks based on the current arrow direction

    private Main mainController;

    // 构造函数，通过它将Main类的实例传递给ArrowController
    public ArrowController(Main main)
    {
        mainController = main; // 将Main实例保存在私有变量mainController中
    }

    public void UpdateDirection(string currentArrow)
    {
        // 根据传入的箭头方向进行逻辑处理
        switch (currentArrow)
        {
            case "Up":
            case "Down":
                mainController.SetBlinking("SSVEP Middle", true);
                mainController.SetBlinking("SSVEP Left", false);
                mainController.SetBlinking("SSVEP Right", false);
                break;
            case "Left":
            case "Up Left":
                mainController.SetBlinking("SSVEP Middle", true);
                mainController.SetBlinking("SSVEP Left", true);
                mainController.SetBlinking("SSVEP Right", false);
                break;
            case "Right":
            case "Up Right":
                mainController.SetBlinking("SSVEP Middle", true);
                mainController.SetBlinking("SSVEP Left", false);
                mainController.SetBlinking("SSVEP Right", true);
                break;
            default:
                // 默认情况下关闭所有闪烁
                mainController.SetBlinking("SSVEP Middle", false);
                mainController.SetBlinking("SSVEP Left", false);
                mainController.SetBlinking("SSVEP Right", false);
                break;
        }
    }
}
