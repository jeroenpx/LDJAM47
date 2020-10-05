using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelCollection", menuName = "LDJAM47/LevelCollection", order = 0)]
public class LevelCollection : ScriptableObject {
    
    [NaughtyAttributes.ReorderableList]
    public List<string> sceneNames;

    public string endGame;

    public int GetZeroBasedCurrentLevelIndex() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        return sceneNames.IndexOf(currentSceneName);
    }

    public int GetTotalLevels() {
        return sceneNames.Count;
    }

    public void MoveToNextLevel() {
        int nextLevel = GetZeroBasedCurrentLevelIndex()+1;
        if(nextLevel < sceneNames.Count) {
            SceneManager.LoadScene(sceneNames[nextLevel], LoadSceneMode.Single);
        } else {
            SceneManager.LoadScene(endGame, LoadSceneMode.Single);
        }
    }

    public void MoveToPreviousLevel() {
        int nextLevel = GetZeroBasedCurrentLevelIndex()-1;
        SceneManager.LoadScene(sceneNames[nextLevel], LoadSceneMode.Single);
    }

    public void RestartLevel() {
        int nextLevel = GetZeroBasedCurrentLevelIndex();
        SceneManager.LoadScene(sceneNames[nextLevel], LoadSceneMode.Single);
    }
}