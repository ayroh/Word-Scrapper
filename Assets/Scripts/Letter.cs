using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Letter : MonoBehaviour
{

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [HideInInspector] public Tile tile = null;
    private Color baseColor;

    public void SetText(string newText = default) => text.text = newText;

    private void Start() => baseColor = image.color;

    public void SetImageColor(Color newColor) => image.color = newColor;

    public void ResetImageColor() => image.color = baseColor;

    public Color GetImageColor() => image.color;

    public void ResetLetter()
    {
        if (baseColor == default)
            return;
        image.color = baseColor;
        text.text = "";
        tile = null;
    }

    public void Release() => PoolManager.instance.ReleaseLetter(this);

}
