using UnityEngine;
using System.Collections.Generic;

public class Spreader : MonoBehaviour {
    public List<GameObject> prefabs;

    public LevelReader reader;

    public int minAmountPerTile = 7;
    public int maxAmountPerTile = 10;

    public int seed = 1;

    GameObject DoInstantiatePart(GameObject prefab, Vector3 position) {
        #if UNITY_EDITOR
            UnityEngine.Object o = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
            GameObject inst = ((GameObject)o);
            inst.transform.position = position;
        #else
            GameObject inst = GameObject.Instantiate(prefab, position, Quaternion.identity);
            inst.transform.parent = transform;
        #endif
        Randomizer r = inst.GetComponent<Randomizer>();
        if(r!=null) {
            r.RandomizeAll();
        }
        return inst;
    }

    [NaughtyAttributes.Button("Clean Up")]
    void DoClear() {
        #if UNITY_EDITOR
            List<Transform> transforms = new List<Transform>();
            foreach (Transform child in transform) {
                transforms.Add(child);
            }
            foreach (Transform child in transforms) {
                GameObject.DestroyImmediate(child.gameObject);
            }
        #else
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
        #endif
    }

    [NaughtyAttributes.Button]
    public void Spread() {
        // Clean up any children we have
        DoClear();

        // Init random
        Random.InitState(seed);

        LevelRepresentation lvl = reader.GetLevel();
        for(int x = 0;x < lvl.xsize; x++) {
            for(int y = 0;y < lvl.ysize; y++) {
                int height = lvl.height[x, y];
                if(height>10) {
                    // wall, ignore
                } else if (height >= 0) {
                    // Ground tile
                    int amount = Random.Range(minAmountPerTile, maxAmountPerTile);
                    Vector3 center = new Vector3(x+.5f, lvl.GetFloorHeightAt(x, y), -(y+.5f));
                    for(int i=0;i<amount;i++) {
                        Vector3 goal = center+ new Vector3(Random.value - .5f, 0, Random.value - .5f);
                        DoInstantiatePart(prefabs[Random.Range(0, prefabs.Count)], goal);
                    }
                }
            }
        }
    }
}