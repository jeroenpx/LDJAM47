using UnityEngine;

public class Randomizer : MonoBehaviour {
    
    public float minScale = 1;
    public float maxScale = 1;

    public AnimationCurve scaleDistribution;

    public float maxTilt = 45;

    [NaughtyAttributes.Button("Randomize All")]
    public void RandomizeAll() {
        RandomizeScale();
        RandomizeTilt();
    }

    [NaughtyAttributes.Button("Randomize Scale")]
    public void RandomizeScale() {
        transform.localScale = Vector3.one * (Mathf.Clamp01(scaleDistribution.Evaluate(Random.value))*(maxScale-minScale)+minScale);
    }

    [ContextMenu("Randomize Tilt & Rotation")]
    [NaughtyAttributes.Button("Randomize Tilt & Rotation")]
    public void RandomizeTilt() {
        Vector2 rot = Random.insideUnitCircle * Mathf.Sin(maxTilt*Mathf.Deg2Rad);
        float dist = rot.magnitude;
        float angle = Vector2.SignedAngle(Vector2.right, rot);

        Quaternion rotateAround = Quaternion.AngleAxis(Random.value*360, Vector3.up);
        Quaternion tilt = Quaternion.AngleAxis(Mathf.Asin(dist)*Mathf.Rad2Deg, Vector3.forward);
        Quaternion rotateTilt = Quaternion.AngleAxis(angle, Vector3.up);

        Quaternion total = rotateTilt*tilt*rotateAround;
        transform.localRotation = total;
    }
}