using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target = null;
    private Vector3 currentVelocity;

    public float smoothTime = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        Player p = GameObject.FindObjectOfType<Player>();
        if(p != null) {
            target = p.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(target!=null) {
            transform.position = LeanSmooth.damp(transform.position, target.position, ref currentVelocity, smoothTime);
        }
    }
}
