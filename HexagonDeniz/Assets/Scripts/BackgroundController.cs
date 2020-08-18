using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class BackgroundController : MonoBehaviour
{
    public int Width = 2;
    public int Height = 3;

    private Animator animator;
    private Image[] images;

    void Awake()
    {
        animator = GetComponent<Animator>();
        images = GetComponentsInChildren<Image>();
        
        //Check child image count
        if (images.Length != 2)
        {
            Debug.LogError("Need 2 background images!");

            //Disable animator since we don't have the expected image count
            animator.enabled = false;
            return;
        }

        //Set default backgrounds
        SetBackgroundSprites();
    }

    private Sprite GenerateRandomSprite(int width, int height)
    {
        //Create texture
        var texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        //Set pixels
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Random.ColorHSV(0.5f, 0.8f, 0.1f, 0.3f, 0.4f,0.6f);
        texture.SetPixels(colors);
        texture.Apply();
        
        //Create sprite
        return Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);
    }

    private bool init = false;
    public void SetBackgroundSprites()
    {
        if (!init)
        {
            init = true;

            //Set new sprite
            images[0].sprite = GenerateRandomSprite(Width, Height);
        }
        else
        {
            //Copy sprite from back to front
            Sprite os = images[1].sprite;
            images[0].sprite = Sprite.Create(os.texture, os.rect, os.pivot);
        }

        //Set new sprite at back
        images[1].sprite = GenerateRandomSprite(Width, Height);
    }
}
