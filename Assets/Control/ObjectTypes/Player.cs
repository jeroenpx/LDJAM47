using UnityEngine;

public class Player : MonoBehaviour, IGridMoveableObject {
    
    [field: SerializeField]
    public Vector3Int GridPosition {get; set;}

    private Direction nextDirection = Direction.NONE;

    public float moveDuration = .2f;
    public float fallDuration = .1f;

    private void Update() {
        if(Input.GetKeyDown(KeyCode.RightArrow)) {
            nextDirection = Direction.EAST;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            nextDirection = Direction.NORTH;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            nextDirection = Direction.SOUTH;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            nextDirection = Direction.WEST;
        }
    }
    
    public Direction GetWantsToMoveDirection() {
        return nextDirection;
    }

    public void Animate(Direction moveDirection, int fallLevels) {
        Direction wantedToMove = GetWantsToMoveDirection();

        Vector3 goal = new Vector3(GridPosition.x+.5f, transform.position.y, -(GridPosition.y+.5f));
        
        transform.LeanMove(goal, moveDuration);
    }

    public void OnEndFrame() {
        nextDirection = Direction.NONE;
    }

}