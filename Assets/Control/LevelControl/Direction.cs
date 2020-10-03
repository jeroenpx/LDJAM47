using UnityEngine;

public class Direction {
    public Vector2Int Delta {get; private set;}
    public int Index {get; private set;}

    private Direction(Vector2Int delta, int index) {
        Delta = delta;
        Index = index;
    }

    public static Direction NONE = new Direction(new Vector2Int(0, 0), -1);
    public static Direction NORTH = new Direction(new Vector2Int(0, -1), 0);
    public static Direction EAST = new Direction(new Vector2Int(1, 0), 1);
    public static Direction SOUTH = new Direction(new Vector2Int(0, 1), 2);
    public static Direction WEST = new Direction(new Vector2Int(-1, 0), 3);

    public static Direction[] AvailableDirections = new Direction[] {
        NONE,
        NORTH,
        EAST,
        SOUTH,
        WEST
    };
}