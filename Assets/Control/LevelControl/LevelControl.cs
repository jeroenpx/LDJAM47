using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelControl : MonoBehaviour {
    
    private LevelRepresentation lvl;
    private List<IGridMoveableObject> moveables;

    public float timeStep = 0.5f;

    public int stepCounter = 1;

    private bool won = false;

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
            stepCounter++;
        }
    }

    private void Tick() {
        // Ok, let's do an update
        foreach(IGridMoveableObject moveable in moveables) {
            Vector3Int startPos = moveable.GridPosition;
            Direction moveTo = moveable.GetWantsToMoveDirection();

            // Update data
            Vector3Int endPos = startPos + (Vector3Int)moveTo.Delta;
            Direction canMove = Direction.NONE;
            if(lvl.GetFloorHeightAt(endPos.x, endPos.y) == moveable.GridPosition.z) {
                // Same level, OK!
                canMove = moveTo;
            }

            moveable.GridPosition = startPos + (Vector3Int)canMove.Delta;;

            // Animate
            moveable.Animate(canMove, 0);

            // End Frame
            moveable.OnEndFrame();
        }

        CheckWinCondition();
    }

    public void CheckWinCondition() {
        // Check which buttons are pressed? / Update them
        int amountButtons = lvl.buttons.Count;
        int amountButtonsPressed = 0;
        foreach(GameObject btn in lvl.buttons) {
            Button b = btn.GetComponent<Button>();
            bool pressed = false;
            foreach(IGridMoveableObject moveable in moveables) {
                if(b.GridPosition == moveable.GridPosition) {
                    pressed = true;
                }
            }
            b.UpdatePressed(pressed);
            if(pressed) {
                amountButtonsPressed++;
            }
        }
        if(amountButtonsPressed == amountButtons && !won) {
            won = true;
            Debug.Log("Win condition triggered!");
            SendMessage("OnMessageWeWon", SendMessageOptions.DontRequireReceiver);
        }
    }
}