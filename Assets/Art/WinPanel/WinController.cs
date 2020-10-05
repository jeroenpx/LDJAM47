using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinController : MonoBehaviour
{
    public Animator animator;

    public void OnMessageWeWon() {
        animator.SetTrigger("DoWin");

        LeanTween.delayedCall(2f, () => {
            // Move on to the next level
            GetComponent<LevelInfoMgr>().GotoNextLevel();
        });
    }
}
