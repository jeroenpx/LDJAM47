using UnityEngine;
using System.Collections.Generic;

public class Cube : MonoBehaviour, IGridMoveableObject {

    public List<int> directions = new List<int>();
    private int index = 0;

    public float moveDuration = .2f;
    public float fallDuration = .1f;

    public Transform animationRotation;
    public Transform inverseAnimationRotation;
    public Animator moveAnimator;

    [field: SerializeField]
    public Vector3Int GridPosition {get; set;}
    
    public Direction GetWantsToMoveDirection() {
        return Direction.AvailableDirections[directions[index]];
    }

    public void Animate(Direction moveDirection, int fallLevels) {
        Direction wantedToMove = GetWantsToMoveDirection();

        Vector3 goal = new Vector3(GridPosition.x+.5f, transform.position.y, -(GridPosition.y+.5f));
        
        if(wantedToMove.Index != -1) {
            // Play move animation
            animationRotation.rotation = Quaternion.AngleAxis(wantedToMove.Index*90, Vector3.up);
            inverseAnimationRotation.localRotation = Quaternion.Inverse(animationRotation.localRotation);

            // moveAnimator
            transform.LeanMove(goal, moveDuration);
        }
    }

    public void OnEndFrame() {
        index++;
        if(index >= directions.Count) {
            index-=directions.Count;
        }
    }
}