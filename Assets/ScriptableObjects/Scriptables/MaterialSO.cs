using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Material Scriptable Object")]
public class MaterialSO : ScriptableObject
{
    public Material DefaultMaterial;
    public Material DimmedDefaultMaterial;
    public Material TotalWrongMaterial;
    public Material CorrectMaterial;
    public Material OrderWrongMaterial;
    public Material DimmedOrderWrongMaterial;
}
