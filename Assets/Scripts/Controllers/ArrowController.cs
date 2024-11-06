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
    private GameObject[] blocks; // SSVEP Controller x 3
    public ArrowController(GameObject[] blocks)
    {
        this.blocks = blocks;
    }

    public void UpdateDirection(string currentArrow)
    {
        // 根据传入的箭头方向进行逻辑处理
        switch (currentArrow)
        {
            case "Up":
            case "Down":
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", false);
                SetBlinking("SSVEP Right", false);
                break;
            case "Left": // 隐蔽观察左边方块，中间不闪烁
                SetBlinking("SSVEP Middle", false);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", false);
                break;
            case "Up Left":// 隐蔽观察左边方块，中间闪烁
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", false);
                break;
            case "Right": // 隐蔽观察右边方块，中间不闪烁
                SetBlinking("SSVEP Middle", false);
                SetBlinking("SSVEP Left", false);
                SetBlinking("SSVEP Right", true);
                break;
            case "Up Right":// 隐蔽观察右边方块，中间闪烁
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", false);
                SetBlinking("SSVEP Right", true);
                break;
            default:
                // 默认情况下关闭所有闪烁
                SetBlinking("SSVEP Middle", false);
                SetBlinking("SSVEP Left", false);
                SetBlinking("SSVEP Right", false);
                break;
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
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].name == blockName) return i;
        }
        return -1;
    }
}
