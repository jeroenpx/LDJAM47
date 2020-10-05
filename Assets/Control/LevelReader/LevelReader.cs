using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Xml.Linq;

public class LevelReader : MonoBehaviour
{
    public string levelFileName = "Level_";

    public bool readFromFileSystem = false;

    [SerializeField]
    private LevelRepresentation lvl = null;

    public List<GameObject> prefabsGround;
    public GameObject prefabWall;
    public GameObject prefabPoint;
    public GameObject prefabCorner;
    public GameObject prefabCliff;
    public GameObject prefabBlock;
    public GameObject prefabTarget;
    public GameObject prefabPlayer;

    [NaughtyAttributes.Button("(Re-)import Level")]
    [ContextMenu("(Re-)import Level")]
    void Import()
    {
        ReadLevel(levelFileName);
    }

    public LevelRepresentation GetLevel() {
        if(lvl == null || lvl.xsize == 0) {
            Import();
        }
        return lvl;
    }

    Vector2Int? DecodeTile(XElement tile) {
        int value = int.Parse(tile.Attribute("gid").Value) - 1;
        if(value==-1) {
            return null;
        } else {
            int y = value / 6;
            int x = value - y*6;
            return new Vector2Int(x, y);
        }
    }

    GameObject DoInstantiatePart(GameObject prefab, Vector3 position, Quaternion rotation) {
        #if UNITY_EDITOR
            UnityEngine.Object o = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
            GameObject inst = ((GameObject)o);
            inst.transform.position = position;
            inst.transform.rotation = rotation;
            return inst;
        #else
            GameObject inst = GameObject.Instantiate(prefab, position, rotation);
            inst.transform.parent = transform;
            return inst;
        #endif
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

    Vector3 GetFloorInstantiatePos(int x, int y) {
        return new Vector3(x+.5f, lvl.GetFloorHeightAt(x, y), -(y+.5f));
    }

    void ReadLevel(string fullPath)
    {
        // Clean up any children we have
        DoClear();

        // Init random
        Rnd.InitState(1);

        string path = "Assets/Levels/Resources/" + levelFileName + ".tmx";
        if(readFromFileSystem) {
            path = System.IO.Path.GetFullPath(".")+"/"+ levelFileName + ".tmx";
        }

        XDocument doc = XDocument.Load(path);
        lvl = new LevelRepresentation();

        // Get width & height
        lvl.xsize = int.Parse(doc.Root.Attribute("width").Value);
        lvl.ysize = int.Parse(doc.Root.Attribute("height").Value);

        lvl.height = new IntGrid();
        lvl.height.DefaultValue = -2;

        lvl.moveables = new List<GameObject>();

        // Read tiles
        // Find Layers
        IEnumerable<XElement> objects = null;
        IEnumerable<XElement> ground = null;
        foreach (XElement layer in doc.Root.Descendants("layer"))
        {
            XAttribute nameAttr = layer.Attribute("name");
            string name = nameAttr.Value;
            IEnumerable<XElement> children = layer.Descendants("data").Descendants("tile");
            if (name == "Ground")
            {
                ground = children;
            }
            if (name == "Objects")
            {
                objects = children;
            }
        }

        int index;
        // Fill level height
        index = 0;
        foreach (XElement tile in ground)
        {
            int y = index / lvl.xsize;
            int x = index - y * lvl.xsize;
            Vector2Int? parsedTile = DecodeTile(tile);

            // Ok, what does this tile mean in practice?
            if(parsedTile.HasValue) {
                if(parsedTile.Value.x > 1 || parsedTile.Value.y > 1) {
                    Debug.LogWarning("Unexpected tile on ground layer at ("+x+", "+y+")!");
                } else {
                    if(parsedTile.Value.x == 0) {
                        lvl.height[x, y] = 100 + parsedTile.Value.y;// Wall!
                    } else {
                        lvl.height[x, y] = parsedTile.Value.y * 2;// Ground levels!
                    }
                }
            }

            index++;
        }

        // Create Objects
        index = 0;
        foreach (XElement tile in objects)
        {
            int y = index / lvl.xsize;
            int x = index - y * lvl.xsize;
            Vector2Int? parsedTile = DecodeTile(tile);

            // Ok, what does this tile mean in practice?
            if(parsedTile.HasValue) {
                if(parsedTile.Value.x == 0 && parsedTile.Value.y == 2) {
                    // Block
                    GameObject blockObj = DoInstantiatePart(prefabBlock, GetFloorInstantiatePos(x, y), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));
                    
                    lvl.moveables.Add(blockObj);
                    blockObj.GetComponent<IGridMoveableObject>().GridPosition = new Vector3Int(x, y, lvl.GetFloorHeightAt(x, y));
                } else if (parsedTile.Value.x == 1 && parsedTile.Value.y == 2) {
                    // Target
                    DoInstantiatePart(prefabTarget, GetFloorInstantiatePos(x, y), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));
                } else if (parsedTile.Value.x == 0 && parsedTile.Value.y == 3) {
                    // Player
                    GameObject playerObj = DoInstantiatePart(prefabPlayer, GetFloorInstantiatePos(x, y), Quaternion.AngleAxis(90, Vector3.up));

                    lvl.moveables.Add(playerObj);
                    playerObj.GetComponent<IGridMoveableObject>().GridPosition = new Vector3Int(x, y, lvl.GetFloorHeightAt(x, y));
                } else {
                    Debug.LogWarning("Unexpected tile on objects layer at ("+x+", "+y+")!");
                }
            }

            index++;
        }

        for(int x=0;x < lvl.xsize;x++)
        {
            for(int y=0;y < lvl.ysize;y++)
            {
                int heightNorth = lvl.height[x, y-1];
                int heightSouth = lvl.height[x, y+1];
                int heightEast = lvl.height[x+1, y];
                int heightWest = lvl.height[x-1, y];
                int height = lvl.height[x, y];
                if(height < 0) {
                    // Nothingness
                } else if(height<100) {
                    // Floor
                    GameObject prefabGround = prefabsGround[Rnd.Range(0, prefabsGround.Count)];
                    GameObject inst = DoInstantiatePart(prefabGround, GetFloorInstantiatePos(x, y), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));

                    // Walls around floor?
                    for(int i=heightNorth/2;i<height/2;i++) {
                        DoInstantiatePart(prefabCliff, new Vector3(x+.5f, i, -(y)), Quaternion.AngleAxis(90, Vector3.up));
                    }
                    for(int i=heightSouth/2;i<height/2;i++) {
                        DoInstantiatePart(prefabCliff, new Vector3(x+.5f, i, -(y+1)), Quaternion.AngleAxis(-90, Vector3.up));
                    }
                    for(int i=heightEast/2;i<height/2;i++) {
                        DoInstantiatePart(prefabCliff, new Vector3(x+1, i, -(y+.5f)), Quaternion.AngleAxis(180, Vector3.up));
                    }
                    for(int i=heightWest/2;i<height/2;i++) {
                        DoInstantiatePart(prefabCliff, new Vector3(x, i, -(y+.5f)), Quaternion.AngleAxis(0, Vector3.up));
                    }
                } else {
                    // Wall
                    int amountWalls = (heightNorth>10?1:0) + (heightSouth>10?1:0) + (heightEast>10?1:0) + (heightWest>10?1:0);
                    int vertical = (heightNorth>10?1:0) + (heightSouth>10?1:0);
                    int horizontal = (heightEast>10?1:0) + (heightWest>10?1:0);
                    if(amountWalls == 2 && vertical == 2) {
                        for(int i=-1;i<=height-100;i++) {
                            GameObject inst = DoInstantiatePart(prefabWall, new Vector3(x+.5f, i, -(y+.5f)), Quaternion.AngleAxis(Rnd.Range(0,2)*180, Vector3.up));
                        }
                    } else if(amountWalls == 2 && horizontal == 2) {
                        for(int i=-1;i<=height-100;i++) {
                            GameObject inst = DoInstantiatePart(prefabWall, new Vector3(x+.5f, i, -(y+.5f)), Quaternion.AngleAxis(Rnd.Range(0,2)*180 + 90, Vector3.up));
                        }
                    } else {
                        if(amountWalls == 0) {
                            for(int i=-1;i<=height-100;i++) {
                                GameObject inst = DoInstantiatePart(prefabPoint, new Vector3(x+.5f, i, -(y+.5f)), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));
                            }
                        } else {
                            /*for(int i=-1;i<=height-100;i++) {
                                GameObject inst = DoInstantiatePart(prefabCorner, new Vector3(x+.5f, i, -(y+.5f)), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));
                            }*/
                            DoInstantiatePart(prefabCorner, new Vector3(x+.5f, -1, -(y+.5f)), Quaternion.AngleAxis(Rnd.Range(0,4)*90, Vector3.up));
                        }
                    }
                    // if((heightNorth<10 || heightEast<10) && )
                }
            }
        }

        // Read block paths!
        IEnumerable<XElement> links = doc.Root.Descendants("objectgroup").Descendants("object").Descendants("polyline");
        foreach (XElement link in links)
        {
            int pixelx = Mathf.FloorToInt(float.Parse(link.Parent.Attribute("x").Value));
            int pixely = Mathf.FloorToInt(float.Parse(link.Parent.Attribute("y").Value));
            string[] points = link.Attribute("points").Value.Split(new char[] { ' ' });
            bool first = true;
            Vector2Int startPoint = new Vector2Int(0, 0);
            List<int> directions = new List<int>();
            Vector2Int previousPoint = new Vector2Int(0, 0);
            foreach (string point in points)
            {
                string[] parts = point.Split(new char[] { ',' });
                int x = (pixelx + Mathf.RoundToInt(float.Parse(parts[0]))) / 16;
                int y = (pixely + Mathf.RoundToInt(float.Parse(parts[1]))) / 16;
                Vector2Int pointVect = new Vector2Int(x, y);
                if(first) {
                    startPoint = pointVect;
                    first = false;
                } else {
                    Vector2Int diff = pointVect-previousPoint;
                    Debug.DrawLine(new Vector3(previousPoint.x+.5f, .5f, -(previousPoint.y+.5f)), new Vector3(pointVect.x+.5f, .5f, -(pointVect.y+.5f)), Color.red, 1f);
                    for(int i=0;i<Direction.AvailableDirections.Length;i++) {
                        Direction dir = Direction.AvailableDirections[i];
                        if(diff == dir.Delta) {
                            directions.Add(i);
                        }
                    }
                }
                previousPoint = pointVect;
            }
            foreach(GameObject obj in lvl.moveables) {
                Cube cube = obj.GetComponent<Cube>();
                if(cube!=null) {
                    if((Vector2Int)cube.GridPosition == startPoint) {
                        cube.directions = directions;
                    }
                }
            }
        }
    }
}
