using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelControl : MonoBehaviour {
    
    private LevelRepresentation lvl;
    private List<MovingObject> moveables;

    public float timeStep = 0.5f;

    public int stepCounter = 1;

    private bool won = false;

    public AudioSource source;
    public AudioClip win;

    private void Start() {
        // Get the level
        lvl = GetComponent<LevelReader>().GetLevel();
        List<GameObject> moveablego = lvl.moveables;
        moveables = new List<MovingObject>();

        source = GetComponent<AudioSource>();

        foreach(GameObject obj in moveablego) {
            moveables.Add(new MovingObject(obj.GetComponent<IGridMoveableObject>()));
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

    class MovingObject {
        public IGridMoveableObject moveable;

        public Vector3Int startPos;

        public Direction moveTo;

        public Vector3Int predictedEndPos;

        public Direction pushedDirection;

        public bool canMoveNoWall;

        public Direction finalMoveTo;

        public MovingObject(IGridMoveableObject moveable) {
            this.moveable = moveable;
        }
    }

    class Cell {
        public List<IGridMoveableObject> goingTo;
    }

    private void Tick() {
        // Init
        SortedSet<int> levels = new SortedSet<int>();
        foreach(MovingObject obj in moveables) {
            IGridMoveableObject moveable = obj.moveable;
            obj.startPos = moveable.GridPosition;
            obj.moveTo = moveable.GetWantsToMoveDirection();
            obj.pushedDirection = Direction.NONE;

            obj.predictedEndPos = obj.startPos + (Vector3Int)obj.moveTo.Delta;

            levels.Add(obj.predictedEndPos.z);
        }

        IntGrid gapFilled = new IntGrid();
        gapFilled.DefaultValue = -1;

        // Update lowest level first
        foreach(int level in levels) {
            foreach(MovingObject obj in moveables) {
                // Things that can happen: 
                //  - you run into a wall/gap => we can know 100% whether we can move now
                bool hitWall = lvl.GetFloorHeightAt(obj.predictedEndPos.x, obj.predictedEndPos.y) > obj.predictedEndPos.z;
                bool hitGap = lvl.GetFloorHeightAt(obj.predictedEndPos.x, obj.predictedEndPos.y) < obj.predictedEndPos.z;
                obj.canMoveNoWall = !hitWall && (!hitGap || gapFilled[obj.predictedEndPos.x, obj.predictedEndPos.y] == obj.predictedEndPos.z);
                
                // Blocks that don't move move with the block underneath (ITERATION 1)
                if(!obj.canMoveNoWall) {
                    MovingObject below = null;
                    foreach(MovingObject floorCheck in moveables) {
                        if(new Vector2Int(floorCheck.startPos.x, floorCheck.startPos.y) == new Vector2Int(obj.startPos.x, obj.startPos.y) && floorCheck.startPos.z == obj.startPos.z-1) {
                            below = floorCheck;
                        }
                    }
                    if(below != null) {
                        // Change direction... Correct it, we can move (as we don't have overhangs and the block below can move)
                        obj.moveTo = below.finalMoveTo;
                        obj.canMoveNoWall = true;
                    }
                }
            }

            //  - you run into another block
            foreach(MovingObject obj in moveables) {
                // Find out which blocks run into each other?
            }

            

            // Finally, move them in memory
            foreach(MovingObject obj in moveables) {
                Direction movingDir = Direction.NONE;
                if(obj.canMoveNoWall) {
                    movingDir = obj.moveTo;
                }
                obj.finalMoveTo = movingDir;
                Vector3Int finalPosition = obj.startPos + (Vector3Int) movingDir.Delta;
                gapFilled[finalPosition.x, finalPosition.y] = finalPosition.z+1;
            }
        }

        // TODO: try to push things



        foreach(MovingObject obj in moveables) {
            // Check if we are moving to the start position of something.
            // If so, we can only move if that object can move & if it doesn't move the opposite direction as us


            // Check if we are moving towards 
        }


        // Finalize, actually move everything.
        foreach(MovingObject obj in moveables) {
            IGridMoveableObject moveable = obj.moveable;
            moveable.GridPosition = obj.startPos + (Vector3Int) obj.finalMoveTo.Delta;

            // Animate
            moveable.Animate(obj.finalMoveTo, 0);

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
            foreach(MovingObject obj in moveables) {
                IGridMoveableObject moveable = obj.moveable;
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

            source.PlayOneShot(win);
        }
    }
}