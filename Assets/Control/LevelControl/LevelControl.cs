using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelControl : MonoBehaviour {
    
    private LevelRepresentation lvl;
    private List<IGridMoveableObject> moveables;

    public float timeStep = 0.5f;

    private void Start() {
        // Get the level
        lvl = GetComponent<LevelReader>().GetLevel();
        List<GameObject> moveablego = lvl.moveables;
        moveables = new List<IGridMoveableObject>();
        foreach(GameObject obj in moveablego) {
            moveables.Add(obj.GetComponent<IGridMoveableObject>());
        }

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop() {
        while(true) {
            yield return new WaitForSeconds(timeStep);
            Tick();
        }
    }

    private void Tick() {
        // Ok, let's do an update
        foreach(IGridMoveableObject moveable in moveables) {
            Vector3Int startPos = moveable.GridPosition;
            Direction moveTo = moveable.GetWantsToMoveDirection();

            // Update data
            Vector3Int endPos = startPos + (Vector3Int)moveTo.Delta;
            moveable.GridPosition = endPos;

            // Animate
            moveable.Animate(moveTo, 0);

            // End Frame
            moveable.OnEndFrame();
        }
    }
}