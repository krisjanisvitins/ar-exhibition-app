// Galvenais skripts objektu pārvaldībai - 3D objektu izvietošana uz plaknēm, pārvietošana, dzēšana, mērogošana, rotācija
// https://learn.unity.com/tutorial/placing-and-manipulating-objects-in-ar
// https://sadra1f.github.io/unity-ar-tutorial/en/placing-objects.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; 
using UnityEngine.EventSystems;   
using UnityEngine.UI;              
using TMPro;                    

public class SpawnableManager : MonoBehaviour
{
    // INSPECTOR

    [Header("AR komponentes")]
    [SerializeField]
    ARRaycastManager m_RaycastManager;

    [Header("Mērogošanas UI")]
    [SerializeField]
    Slider scaleSlider;

    [SerializeField]
    TextMeshProUGUI scaleLabel;

    [Header("Rotācijas UI")]
    [SerializeField]
    Slider rotationSlider; 

    [SerializeField]
    TextMeshProUGUI rotationLabel;

    [HideInInspector]
    public float baseScale = 1f;

    public GameObject spawnablePrefab;    // 3D modelis, ko izvieto uz plaknēm

    public string currentModelFileName = "";

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    Camera arCam;                                 

    List<GameObject> spawnedObjects = new List<GameObject>(); // Visu izvietoto objektu saraksts

    GameObject spawnedObject;             // Pašlaik satvertais objekts
    GameObject selectedObject;            // Izvēlētais objekts

    // Mērogošana/rotācija/dzēšana
    float currentScale = 0.05f;
    float currentRotationY = 0f;

    float touchStartTime;
    bool isTouchHeld;
    bool hasMoved;
    float longPressDuration = 0.5f;
    GameObject heldObject;

    Vector3 mouseStartPosition;
    float moveThreshold = 5f;

    void Start()
    {
        spawnedObject = null;
        arCam = Camera.main;
                                      // https://docs.unity3d.com/ScriptReference/Camera-main.html

        // Mēroga sliders
        if (scaleSlider != null)
        {
            scaleSlider.value = currentScale;
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
            // https://docs.unity3d.com/530/Documentation/ScriptReference/UI.Slider-onValueChanged.html
        }
        UpdateScaleLabel();


        // Rotācijas sliders
        if (rotationSlider != null)
        {
            rotationSlider.value = currentRotationY;
            rotationSlider.onValueChanged.AddListener(OnRotationChanged);
        }
        UpdateRotationLabel();
    }

    // Ievades pārbaude

    void Update()
    {
        if (Input.touchCount == 0)
        {
            HandleMouseInput();
            return;
        }
        HandleTouchInput();
    }

    // UI POGU FUNKCIJAS

    // Izdzēš pēdējo izvietoto objektu no scene un saraksta
    public void DeleteLastObject()
    {
        if (spawnedObjects.Count > 0)
        {
                GameObject lastObject = spawnedObjects[spawnedObjects.Count - 1];
                spawnedObjects.RemoveAt(spawnedObjects.Count - 1);

                if (lastObject == selectedObject) selectedObject = null;

                Destroy(lastObject);
                Debug.Log("Izdzēsts pēdējais objekts");
        }
        else
        {
            Debug.Log("Nav ko dzēst");
        }
    }


    // Izdzēš visus izvietotos objektus no scene un iztīra sarakstu

    public void DeleteAllObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear();
        spawnedObject = null;
        selectedObject = null;
    }

    // Izdzēš šobrīd izvēlēto objektu

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            spawnedObjects.Remove(selectedObject);
            Destroy(selectedObject);
            selectedObject = null;
            spawnedObject = null;
        }
        else
        {
            Debug.Log("Nav izvēlēts objekts");
        }
    }

    // MĒROGOŠANA/ROTĀCIJA

    private void OnScaleChanged(float value)
    {
        currentScale = value;
        UpdateScaleLabel();
        if (selectedObject != null)
        {
            float finalScale = baseScale * currentScale;
            selectedObject.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
            // https://docs.unity3d.com/ScriptReference/Transform-localScale.html
            Debug.Log("Mērogs: " + currentScale.ToString("F3"));
        }
    }

    private void UpdateScaleLabel()
    {
        if (scaleLabel != null)
        {
            scaleLabel.text = "Mērogs: " + currentScale.ToString("F3");
        }
    }


    // https://docs.unity3d.com/ScriptReference/Quaternion.Euler.html

    private void OnRotationChanged(float value)
    {
        currentRotationY = value;
        UpdateRotationLabel();
        if (selectedObject != null)
        {
            selectedObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
            Debug.Log("Rotācija: " + currentRotationY.ToString("F0") + "°");
        }
    }

    private void UpdateRotationLabel()
    {
        if (rotationLabel != null)
        {
            rotationLabel.text = "Rotācija: " + currentRotationY.ToString("F0") + "°";
        }
    }

    // PELES APSTRĀDE XR SIMULATION
    private void HandleMouseInput()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(0))
        // https://docs.unity3d.com/ScriptReference/Input.GetMouseButtonDown.html
        {
            touchStartTime = Time.time;
            hasMoved = false;
            isTouchHeld = false;
            heldObject = null;
            mouseStartPosition = Input.mousePosition;

            RaycastHit hit;
            Ray ray = arCam.ScreenPointToRay(Input.mousePosition);
            if (m_RaycastManager.Raycast(Input.mousePosition, m_Hits, TrackableType.PlaneWithinPolygon) && m_Hits.Count > 0)
            {
                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Spawnable")
                {
                    spawnedObject = hit.collider.gameObject;
                    heldObject = hit.collider.gameObject;
                    isTouchHeld = true;

                    selectedObject = hit.collider.gameObject;
                    currentScale = selectedObject.transform.localScale.x / baseScale;
                    if (scaleSlider != null) scaleSlider.value = currentScale;
                    UpdateScaleLabel();

                    currentRotationY = selectedObject.transform.rotation.eulerAngles.y;
                    if (rotationSlider != null) rotationSlider.value = currentRotationY;
                    UpdateRotationLabel();
                }
                else
                {
                    SpawnPrefab(m_Hits[0].pose.position);
                    selectedObject = null;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            float distanceMoved = Vector3.Distance(Input.mousePosition, mouseStartPosition);

            if (distanceMoved > moveThreshold)
            {
                hasMoved = true;
                if (spawnedObject != null && m_RaycastManager.Raycast(Input.mousePosition, m_Hits, TrackableType.PlaneWithinPolygon) && m_Hits.Count > 0)
                {
                    spawnedObject.transform.position = m_Hits[0].pose.position;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            spawnedObject = null;
            isTouchHeld = false;
            heldObject = null;
        }

    }

    // PIESKĀRIENU APSTRĀDE

    //   https://docs.unity3d.com/ScriptReference/TouchPhase.html
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
        if (IsPointerOverUIObject(touch.position)) return;

        RaycastHit hit;
        Ray ray = arCam.ScreenPointToRay(touch.position);

        if (m_RaycastManager.Raycast(touch.position, m_Hits, TrackableType.PlaneWithinPolygon))
        {
            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
                hasMoved = false;
                isTouchHeld = false;
                heldObject = null;

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Spawnable")
                {
                    // Trāpīja objektam
                    spawnedObject = hit.collider.gameObject;
                    heldObject = hit.collider.gameObject;
                    isTouchHeld = true;

                    // Objekts izvēlēts mērogošanai un rotācijai
                    selectedObject = hit.collider.gameObject;
                    currentScale = selectedObject.transform.localScale.x / baseScale; ;
                    if (scaleSlider != null) scaleSlider.value = currentScale;
                    UpdateScaleLabel();

                    currentRotationY = selectedObject.transform.rotation.eulerAngles.y;
                    if (rotationSlider != null) rotationSlider.value = currentRotationY;
                    UpdateRotationLabel();
                }
                else
                {
                    SpawnPrefab(m_Hits[0].pose.position);
                    selectedObject = null;
                }
            }
            // Pārvietošana
            else if (touch.phase == TouchPhase.Moved && spawnedObject != null)
            {
                spawnedObject.transform.position = m_Hits[0].pose.position;
                hasMoved = true;
            }


            if (touch.phase == TouchPhase.Ended)
            {
                spawnedObject = null;
                isTouchHeld = false;
                heldObject = null;
            }
        }
    }

    // UI PIESKĀRIENU FILTRĒŠANA

    // https://discussions.unity.com/t/ar-foundation-never-blocking-raycaster-on-ui/812377
    private bool IsPointerOverUIObject(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    // OBJEKTU IZVIETOŠANA

    // 3D objekta instance
    private void SpawnPrefab(Vector3 spawnPosition)
    {
        Quaternion spawnRotation = Quaternion.Euler(0f, currentRotationY, 0f);

        spawnedObject = Instantiate(spawnablePrefab, spawnPosition, spawnRotation);
        spawnedObject.SetActive(true);

        float finalScale = baseScale * currentScale;
        spawnedObject.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
        spawnedObject.tag = "Spawnable";

        // Pievieno tag
        PlacedObjectTag placedTag = spawnedObject.AddComponent<PlacedObjectTag>();
        placedTag.modelFileName = currentModelFileName;

        spawnedObjects.Add(spawnedObject);
    }


    // METODES PRIEKŠ EXHIBITIONSTORAGE


    // Viss objektu saraksts
    public List<GameObject> GetPlacedObjects()
    {
        return spawnedObjects;
    }

    //  Objektu izvietošana no datiem


    public void SpawnFromData(GameObject prefab, Vector3 worldPosition,
            Quaternion worldRotation, Vector3 localScale, string modelFileName)
    {
        GameObject obj = Instantiate(prefab, worldPosition, worldRotation);
        obj.SetActive(true);
        obj.transform.localScale = localScale;
        obj.tag = "Spawnable";

        PlacedObjectTag placedTag = obj.AddComponent<PlacedObjectTag>();
        placedTag.modelFileName = modelFileName;

        spawnedObjects.Add(obj);
    }
}