using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class CustomDocument : ScriptableObject
{
    public string documentName;
    public TabletOrientation documentOrientation;
    public Texture2D[] documentPages;
}
