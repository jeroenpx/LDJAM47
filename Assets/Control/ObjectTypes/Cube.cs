using UnityEngine;
using System.Collections.Generic;

public class Cube : MonoBehaviour, IGridMoveableObject {

    public List<int> directions = new List<int>();
    private int index = 0;

    public float moveDuration = .3f;
    public float fallDuration = .1f;

    public Transform animationRotation;
    public Transform inverseAnimationRotation;
    public Animator moveAnimator;

    public Animator highlightAnimator;

    public bool highlighted;

    private LTDescr descr;

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
            animationRotation.rotation = Quaternion.AngleAxis(wantedToMove.Index*90 -90, Vector3.up);
            inverseAnimationRotation.localRotation = Quaternion.Inverse(animationRotation.localRotation);

            moveAnimator.SetTrigger("Move");

            descr = transform.LeanMove(goal, moveDuration).setIgnoreTimeScale(true);
            //transform.position = goal;
            /*LeanTween.delayedCall(0.5f, () => {
                transform.position = goal;
                moveAnimator.SetTrigger("Idle");
            });*/
        }
    }

    public void OnEndFrame() {
        index++;
        if(index >= directions.Count) {
            index-=directions.Count;
        }
    }

    public void OnClickStopMove() {
        Debug.Log("Cancelled");
        LeanTween.cancel(descr.id);
    }

    public void Highlight(bool highlight) {
        highlighted = highlight;
        highlightAnimator.SetBool("Highlight", highlight);
    }

    public List<Vector3Int> GetPositionsReachable(LevelRepresentation lvl, int distance) {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach(Direction dir in Direction.AvailableDirections) {
            Vector3Int end = GridPosition + (Vector3Int) dir.Delta;
            if(lvl.GetFloorHeightAt(end.x, end.y) == end.z) {
                // Check if there is no obstacle there :)
                bool obstacle = false;
                foreach(GameObject moveable in lvl.moveables) {
                    IGridMoveableObject other = moveable.GetComponent<IGridMoveableObject>();
                    if(other.GridPosition == end && other!=(IGridMoveableObject)this) {
                        obstacle = true;
                    }
                }
                if(!obstacle) {
                    positions.Add(end);
                }
            }
        }
        
        return positions;
    }
}