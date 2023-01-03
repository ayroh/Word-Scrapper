using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    [SerializeField] private TextMeshPro tmp;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material dimmedMaterial;
    private Renderer rend;
    private BoxCollider col;
    public int currentColumn = -1;

    private void Awake() {
        rend = (MeshRenderer)GetComponent("MeshRenderer");
        col = (BoxCollider)GetComponent("BoxCollider");
    }

    public void Init(int Column, char Text) {
        currentColumn = Column;
        tmp.text = Text.ToString();
    }

    public void Init(char Text) => tmp.text = Text.ToString();

    public void Init(int Column) => currentColumn = Column;

    public char GetText() {
        if (tmp.text.Length != 0)
            return tmp.text[0];
        else
            return default;
    }

    public void SetText(char Text) => tmp.text = Text.ToString();

    public void SetMaterial(Material mat, Material dimmedMat = null) {
        baseMaterial = mat;
        dimmedMaterial = dimmedMat;
        rend.sharedMaterial = mat;
    }

    public Material GetMaterial() => rend.sharedMaterial;
    public Color GetColor() => rend.sharedMaterial.color;

    public void Dim() => rend.sharedMaterial = dimmedMaterial;
    public void Brighten() => rend.sharedMaterial = baseMaterial;

    public void ColliderChoice(bool choice) => col.enabled = choice;


}
