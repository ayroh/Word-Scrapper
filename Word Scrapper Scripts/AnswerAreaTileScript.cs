using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerAreaTileScript : MonoBehaviour
{

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] public TileScript tile = null;
    private Color baseColor;

    public void SetText(string newText = default) => text.text = newText;

    private void Start() => baseColor = image.color;

    public void SetImageColor(Color newColor) => image.color = newColor;

    public void SetImageColor() => image.color = baseColor;

    public Color GetImageColor() => image.color;

}
