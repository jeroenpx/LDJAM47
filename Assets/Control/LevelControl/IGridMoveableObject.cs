using UnityEngine;

public interface IGridMoveableObject {
    /**
     * Position of this object.
     * Requested & adapted by LevelControl
     */
    Vector3Int GridPosition {get; set;}

    /**
     * Does this object want to move on its own?
     */
    Direction GetWantsToMoveDirection();

    /**
     * (Force) move the object in a certain direction
     */
    void Animate(Direction moveDirection, int fallLevels);

    /**
     * Move to the end frame
     */
    void OnEndFrame();
}