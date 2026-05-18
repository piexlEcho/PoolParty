using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "Sushi/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public float multiplier;
    public Sprite icon;
    [Range(0f, 1f)]
    public float weight;
}