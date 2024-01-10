using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private MaterialSO materialSO;
    [SerializeField] private TextMeshPro tmp;
    [SerializeField] private Renderer rend;
    [SerializeField] private BoxCollider col;
    [SerializeField] private Rigidbody rb;

    private Material baseMaterial;
    private Material dimmedMaterial;
    public int currentColumn = -1;

    private void Awake()
    {
        ResetTile();
    }

    public void Init(int Column, char Text) {
        currentColumn = Column;
        tmp.text = Text.ToString();
    }

    public void Init(char Text) => tmp.text = Text.ToString();

    public void Init(int newColumn, Vector3 newPos)
    {
        currentColumn = newColumn;
        transform.position = newPos;
    }

    public char GetFirstChar() {
        if (tmp.text.Length != 0)
            return tmp.text[0];
        else
            return default;
    }

    public void SetText(char Text)
    {
        tmp.text = Text.ToString();
        tmp.gameObject.SetActive(true);
    }

    public void SetMaterial(Material mat, Material dimmedMat = null) {
        baseMaterial = mat;
        dimmedMaterial = dimmedMat;
        rend.sharedMaterial = mat;
    }

    public Material GetMaterial() => rend.sharedMaterial;
    public Color GetColor() => rend.sharedMaterial.color;

    public void AddForceTorque(Vector3 force)
    {
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.VelocityChange);
        rb.AddTorque(force, ForceMode.VelocityChange);
    }

    public void Dim() => rend.sharedMaterial = dimmedMaterial;
    public void Brighten() => rend.sharedMaterial = baseMaterial;

    public void ColliderChoice(bool choice) => col.enabled = choice;

    public void ResetTile()
    {
        currentColumn = -1;
        transform.position = new Vector3(100f,100f,100f);
        transform.rotation = Quaternion.identity;
        rb.isKinematic = true;
        tmp.gameObject.SetActive(false);
        tmp.text = "";
        SetMaterial(materialSO.DefaultMaterial, materialSO.DimmedDefaultMaterial);
    }

    public async void Release(float delay = 0)
    {
        if(delay > 0f)
            await UniTask.WaitForSeconds(delay);

        PoolManager.instance.ReleaseTile(this);
    }
}
