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
            case "Down": // Condition B - 基线
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", false);
                SetBlinking("SSVEP Right", false);
                break;
            case "Left Close": 
            case "Right Close": // Condition C - 短距离，无Active SSVEP
                SetBlinking("SSVEP Middle", false);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", true);
                SetDistance("SSVEP Left", true);
                SetDistance("SSVEP Right", true);
                break;
            case "Up Left Close":
            case "Up Right Close":// Condition D - 短距离，有Active SSVEP
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", true);
                SetDistance("SSVEP Left", true);
                SetDistance("SSVEP Right", true);
                break;
            case "Left Far":
            case "Right Far": // Condition E - 长距离，无Active SSVEP
                SetBlinking("SSVEP Middle", false);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", true);
                SetDistance("SSVEP Left", false);
                SetDistance("SSVEP Right", false);

                break;
            case "Up Left Far":
            case "Up Right Far":// Condition F - 长距离，有Active SSVEP
                SetBlinking("SSVEP Middle", true);
                SetBlinking("SSVEP Left", true);
                SetBlinking("SSVEP Right", true);
                SetDistance("SSVEP Left", false);
                SetDistance("SSVEP Right", false);
                break;
            default:
                // 默认情况下关闭所有闪烁 - Condition A (静息)
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

    public void SetDistance(string blockName, bool isClose)
    {
        int index = FindBlock(blockName);

        if (index > -1 && index < 3)
        {
            GameObject block = blocks[index];
            Vector3 oldPos = block.transform.localPosition;

            // 取出当前 x
            float oldX = oldPos.x;
            float absX = Mathf.Abs(oldX);

            // 判断当前处于close(110)还是far(220)，并根据 isClose 决定是否要改
            if (absX != 110f && absX != 220f)
            {
                // 如果进到这里，说明它既不是110也不是220，可按需求处理
                //Debug.LogWarning($"[SetDistance] {blockName} 的绝对X={absX}，不是预期的110或220，无法确认当前距离。");
                return;
            }

            // 保留正负号
            float sign = (oldX < 0) ? -1f : 1f;

            if (isClose && absX == 220f)
            {
                // 从 far(220) 变成 close(110)
                oldPos.x = sign * 110f;
                block.transform.localPosition = oldPos;
            }
            else if (!isClose && absX == 110f)
            {
                // 从 close(110) 变成 far(220)
                oldPos.x = sign * 220f;
                block.transform.localPosition = oldPos;
            }
            else
            {
                // 已经是想要的状态就不用改
                //Debug.Log($"[SetDistance] {blockName} 已经是 {(isClose ? "Close" : "Far")} 距离，无需修改。");
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
