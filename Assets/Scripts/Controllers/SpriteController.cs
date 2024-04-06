/**
 * Author: Yangyulin Ai
 * Email: Yangyulin-1@student.uts.edu.au
 * Date: 2024-03-18
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;


public class SpriteController
{

    private Sprite[] sprites;
    private Action<Sprite> onSpriteChange;

    public SpriteController(Sprite[] sprites, Action<Sprite> onSpriteChange)
    {
        this.sprites = sprites;
        this.onSpriteChange = onSpriteChange;
    }

    public string ChangeSprite(int index)
    {

        var sprite = sprites[UnityEngine.Random.Range(0, sprites.Length - 1)];
        if (index >= 0 && index < sprites.Length-1)
        {
            sprite = sprites[index];
        }
       
        onSpriteChange?.Invoke(sprite);
        return sprite.name;
    }

    public string ChangeBreakSprite()
    {
        var sprite = sprites[sprites.Length - 1];// Change to the last sprite (e.g., 'X' sign)
        onSpriteChange?.Invoke(sprite);
        return sprite.name;
    }
}
