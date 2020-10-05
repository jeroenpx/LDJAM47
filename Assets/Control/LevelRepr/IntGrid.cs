using System;
using System.Collections;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class IntGridYLayer : SerializableDictionaryBase<int, int> {
}
[System.Serializable]
public class IntGridXLayer : SerializableDictionaryBase<int, IntGridYLayer> {
}

[System.Serializable]
public class IntGrid: System.Object
{
    [SerializeField]
    private IntGridXLayer internalGrid = new IntGridXLayer();

    [field: SerializeField]
    public int DefaultValue {get; set;}

    public int this[int x, int y]
    {
        get {
            if(this.internalGrid.ContainsKey(x) && this.internalGrid[x].ContainsKey(y)) {
                return this.internalGrid[x][y];
            }
            return DefaultValue;
        }
        set { 
            if(!this.internalGrid.ContainsKey(x)) {
                this.internalGrid[x] = new IntGridYLayer();
            }
            this.internalGrid[x][y] = value;
        }
    }
}