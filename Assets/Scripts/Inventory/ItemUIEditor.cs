using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIEditor : MonoBehaviour
{
    public Image imgSprite;
    public Text text;

    private void Start()
    {
        //imgSprite = transform.Find("Image").gameObject.GetComponent<Image>();
    }

    public void SetImage(Sprite sprite)
    {
        imgSprite.sprite = sprite;
    }

    public void SetText(string value)
    {
        text.text = value;
    }
}
