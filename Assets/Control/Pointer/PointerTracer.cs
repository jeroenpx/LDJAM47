using UnityEngine;
using System.Collections.Generic;

public class PointerTracer : MonoBehaviour {

    public Camera cam;

    public string layerMask;

    private Cube highlightedCube;

    private bool dragging;

    private LevelControl control;
    private LevelRepresentation lvl;
    private List<Vector3Int> positions;
    private List<GameObject> dropIndicators;
    private List<Material> dropMaterials;
    public GameObject dropIndicatorPrefab;
    private int lastUpdate;

    // While dragging
    private Vector3 velocity;
    private Vector3Int startPosition;
    private Vector3Int newTarget;
    public float smoothTime = 1f;
    public float mouseMoveFactor = 0.1f;
    public float friction = 2;
    public float accelRate = 0.5f;

    public float basicAlpha = 0.1f;
    public float highlighAlpha = 1f;

    public float upAmount = .3f;

    public Color indicatorColorBasic;
    public Color indicatorColorPicked;

    private bool fastForward = false;

    public float fastSpeed = 4f;

    public bool disabledTransition = false;

    public AudioSource source;

    public AudioClip hover;

    public float hoverVol;

    public AudioClip timeStop;
    public AudioClip timeStart;
    public AudioClip tileHover;
    public AudioClip tileSelect;
 

    private bool hoverPlayed = false;
    private bool tileHoverPlayed = false;
    private bool tileSelectPlayed = false;

    private void Start() {
        control = GetComponent<LevelControl>();
        lvl = GetComponent<LevelReader>().GetLevel();

        source = GetComponent<AudioSource>();

       

        dropIndicators = new List<GameObject>();
        dropMaterials = new List<Material>();
        for(int i=0;i<5;i++) {
            dropIndicators.Add(Instantiate(dropIndicatorPrefab, Vector3.zero, Quaternion.identity));
            dropIndicators[i].SetActive(false);
            dropMaterials.Add(dropIndicators[i].GetComponentInChildren<Renderer>().material);
        }
        Shader.SetGlobalFloat("GLOBAL_IndicatorAlpha", basicAlpha);

        Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeRestart", Time.unscaledTime-20000);
        Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeFreeze", Time.unscaledTime-40000);

        Time.timeScale = 1f;
        ToggleFastForward(true, 2f);
        LeanTween.delayedCall(8f, () => {
            ToggleFastForward(false, 1f);
        });
    }

    public void ToggleFastForward(bool enable, float factor) {
        if(!dragging) {
            fastForward = enable;
            if(enable) {
                Time.timeScale = factor * fastSpeed;
            } else {
                Time.timeScale = 1f;
            }
        }
    }

    public void DisableControls(bool disable) {
        disabledTransition = disable;
    }

    private void UpdateDropPositions() {
        positions = highlightedCube.GetPositionsReachable(lvl, 1);
        UpdateDropDisplay();
    }

    private void UpdateDropDisplay() {
        for(int i=0;i<positions.Count;i++) {
            dropIndicators[i].SetActive(true);
            dropIndicators[i].transform.position = new Vector3(positions[i].x + .5f, positions[i].z, -(positions[i].y +.5f));
            if(dragging && positions[i] == newTarget) {
                dropMaterials[i].SetColor("_Color", indicatorColorPicked);
            } else {
                dropMaterials[i].SetColor("_Color", indicatorColorBasic);
            }
        }
        for(int i=positions.Count;i<dropIndicators.Count;i++) {
            dropIndicators[i].SetActive(false);
            dropIndicators[i].transform.position = Vector3.zero;
        }
    }

    private void DisableDropIndicators() {
        for(int i=0;i<dropIndicators.Count;i++) {
            dropIndicators[i].SetActive(false);
            dropIndicators[i].transform.position = Vector3.zero;
        }
    }
    
    private void Update() {
        if(disabledTransition) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.F)) {
            ToggleFastForward(!fastForward, 1f);
        }

        if(fastForward) {
            if(highlightedCube!=null) {
                highlightedCube.Highlight(false);
                highlightedCube = null;
                DisableDropIndicators();
            }
            return;
        }

        // Stuff...
        Shader.SetGlobalFloat("_GLOBAL_UnscaledTime", Time.unscaledTime);

        Ray r = cam.ScreenPointToRay(Input.mousePosition);

        if(!dragging) {
            RaycastHit hitInfo;
            Debug.DrawRay(r.origin, r.direction*20f, Color.red, 0.1f);
            if(Physics.Raycast(r, out hitInfo, 1000f, 1 << LayerMask.NameToLayer(layerMask))) {
                Cube c = hitInfo.collider.gameObject.GetComponent<Cube>();
                if(c!=null) {

                    if (hoverPlayed == false)
                    {
                        source.PlayOneShot(hover, hoverVol);
                        hoverPlayed = true;
                    }
               
                    // We are pointing at this cube. Highlight it!
                    if(highlightedCube == c) {
                        // Already the case
                        // Still... Might need to update dropPositions
                        if(lastUpdate != control.stepCounter) {
                            UpdateDropPositions();
                            lastUpdate = control.stepCounter;
                        }
                    } else {
                        if(highlightedCube!=null) {
                            highlightedCube.Highlight(false);
                            highlightedCube = null;
                        }
                        highlightedCube = c;
                        highlightedCube.Highlight(true);
                        UpdateDropPositions();
                        lastUpdate = control.stepCounter;
                    }
                } else {
                    if(highlightedCube!=null) {
                        highlightedCube.Highlight(false);
                        highlightedCube = null;
                        DisableDropIndicators();
                    }
                }
            } else {
                if(highlightedCube!=null) {
                    highlightedCube.Highlight(false);
                    highlightedCube = null;
                    DisableDropIndicators();

                    hoverPlayed = false;
                }
            }

            if(Input.GetKeyDown(KeyCode.Mouse0) && highlightedCube!=null) {
                // STOP THE WORLD!
                dragging = true;
                Time.timeScale = 0;
                Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeFreeze", Time.unscaledTime);
                Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeRestart", Time.unscaledTime+360000);
                Shader.SetGlobalFloat("GLOBAL_IndicatorAlpha", highlighAlpha);

                source.PlayOneShot(timeStop);

                // Do something with highlightedCube
                startPosition = highlightedCube.GridPosition;
                newTarget = startPosition;
                highlightedCube.OnClickStopMove();

                
                Shader.SetGlobalVector("_GLOBAL_TimeStopCenter", new Vector3(startPosition.x + .5f, startPosition.z+.6f, -(startPosition.y +.5f)));

               

                UpdateDropDisplay();
                
                // Do something with camera? (difficult?)
            }
        } else {
            // Snap Cube!
            float planeHeight = startPosition.z+.6f;
            Plane rayPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
            float enter;
            if(rayPlane.Raycast(r, out enter)) {
                Vector3 mouseWorldLayerPosition = r.origin + r.direction*enter;
                float totalDist = float.MaxValue;
                Vector3Int bestPosition = Vector3Int.zero;
                foreach(Vector3Int pos in positions) {
                    float dist = Vector3.Distance(mouseWorldLayerPosition, new Vector3(pos.x + .5f, planeHeight, -(pos.y +.5f)));
                    if(dist<totalDist) {
                        bestPosition = pos;
                        totalDist = dist;

                      
                    }
                }
                if(bestPosition != newTarget) {
                    Vector3Int oldTarget = newTarget;
                    newTarget = bestPosition;
                            
                  
                    source.PlayOneShot(tileHover, hoverVol);
               

                    if (highlightedCube.GridPosition != newTarget && highlightedCube.GridPosition == oldTarget) {
                        GetComponent<LevelInfoMgr>().IncrementMoves();
                    } else if(highlightedCube.GridPosition == newTarget) {
                        GetComponent<LevelInfoMgr>().DecrementMoves();
                    }
                }
            }
            UpdateDropDisplay();

            // Move cube smoothly + allow cursor feedback
            Vector3 targetCubePosition = new Vector3(newTarget.x + .5f, newTarget.z, -(newTarget.y +.5f));
            Vector3 targetCubePositionUp = targetCubePosition + new Vector3(0, upAmount, 0);
            highlightedCube.transform.position = LeanSmooth.spring(highlightedCube.transform.position, targetCubePositionUp, ref velocity, smoothTime, float.MaxValue, Time.unscaledDeltaTime, friction, accelRate);
            velocity += new Vector3(mouseMoveFactor * Input.GetAxis("Mouse X"), 0, mouseMoveFactor * Input.GetAxis("Mouse Y"));
            //LeanTween.(highlightedCube, )



          

            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                // Released mouse button
                // RESTART THE WORLD
                dragging = false;
                Time.timeScale = 1;
                Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeRestart", Time.unscaledTime);
                //Shader.SetGlobalFloat("_GLOBAL_UnscaledTimeFreeze", Time.unscaledTime+360000);

                source.PlayOneShot(timeStart);

                if (tileSelectPlayed == false)
                {
                    source.PlayOneShot(tileSelect);
                    tileSelectPlayed = true;
                }

                tileSelectPlayed = false;

                Shader.SetGlobalFloat("GLOBAL_IndicatorAlpha", basicAlpha);
                highlightedCube.transform.position = targetCubePosition;
                if(highlightedCube.GridPosition != newTarget) {
                    highlightedCube.GridPosition = newTarget;
                }
                UpdateDropDisplay();
                control.CheckWinCondition();
            }
        }
    }


}