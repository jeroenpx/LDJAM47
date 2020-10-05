using UnityEngine;

public class Button : MonoBehaviour {
    [field: SerializeField]
    public Vector3Int GridPosition {get; set;}

    private bool isPressed;
    
    public void UpdatePressed (bool current) {
        if(isPressed != current) {
            isPressed = current;
        }
        
    }
}