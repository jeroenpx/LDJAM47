using System;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[Serializable]
public class LevelRepresentation : System.Object
{
    [SerializeField]
    public int xsize = 0;
    [SerializeField]
    public int ysize = 0;

    public IntGrid height = null;

    public List<GameObject> moveables = null;

    public List<GameObject> buttons = null;

    public int GetFloorHeightAt(int x, int y) {
        int pointHeight = height[x, y];
        if(pointHeight > 10) {
            pointHeight = (pointHeight-100)+1;
        } else {
            pointHeight = pointHeight/2;
        }
        return pointHeight;
    }
}