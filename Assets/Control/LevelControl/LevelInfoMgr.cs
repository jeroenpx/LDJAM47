using UnityEngine;

public class LevelInfoMgr : MonoBehaviour {
    
    public LevelCollection collection;

    public TMPro.TextMeshProUGUI textLevel;
    public TMPro.TextMeshProUGUI textInstructions;

    public ParticleSystem particles;

    private bool transitioning;

    private bool fastForwardMode;

    public GameObject canvas;

    public bool intro = true;

    private int movesDone = 0;

    public int minMoves = 0;

    private void UpdateInstructions() {
        int myIndex = collection.GetZeroBasedCurrentLevelIndex();
        textLevel.text = "Level "+(myIndex+1)+" / "+collection.GetTotalLevels();
        string instructions = "R   restart level\n";
        if (myIndex>0) {
            instructions+="\nS   skip level\nP   previous level\n";
        } else {
            instructions+="S   skip level\n";
        }
        instructions+="\nF   toggle fast forward\n\n";

        instructions+="\n# moves\n";
        instructions+="   perfect: "+minMoves+"\n";
        instructions+="   you: "+movesDone+"\n";

        textInstructions.text = instructions;
    }

    private void Start() {
        UpdateInstructions();

        intro = true;
        LeanTween.delayedCall(3f, () => {intro = false;}).setIgnoreTimeScale(true);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.R)) {
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            GotoNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            if(collection.GetZeroBasedCurrentLevelIndex()>0) {
                GotoPreviousLevel();
            }
        }

        canvas.SetActive(!fastForwardMode && !transitioning && !intro);
    }

    public void RestartLevel() {
        if(!transitioning) {
            transitioning = true;
            particles.Play();
            GetComponent<PointerTracer>().DisableControls(true);
            LeanTween.delayedCall(1f, () => {
                collection.RestartLevel();
            }).setIgnoreTimeScale(true);
        }
        
    }

    public void GotoNextLevel() {
        if(!transitioning) {
            transitioning = true;
            particles.Play();
            GetComponent<PointerTracer>().DisableControls(true);
            LeanTween.delayedCall(1f, () => {
                collection.MoveToNextLevel();
            });
        }
    }

    public void GotoPreviousLevel() {
        if(!transitioning) {
            transitioning = true;
            particles.Play();
            GetComponent<PointerTracer>().DisableControls(true);
            LeanTween.delayedCall(1f, () => {
                collection.MoveToPreviousLevel();
            });
        }
    }

    public void SetFastForwardMode(bool fastForwardMode) {
        this.fastForwardMode = fastForwardMode;
    }

    public void IncrementMoves() {
        this.movesDone ++;
        UpdateInstructions();
    }

    public void DecrementMoves() {
        this.movesDone --;
        UpdateInstructions();
    }
}