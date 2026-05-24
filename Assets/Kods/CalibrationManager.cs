/*
    Lokālās koordinātu sistēma:
    - Origin = P1 (pirmais pieskāriena punkts)
    - Forward axis (+Z) = horizontāls virziens no P1 uz P2
    - Up axis (+Y) = pasaules augšupvērstais virziens
    - Right axis (+X) = automātiski no rotation
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;     
using TMPro;                     

public class CalibrationManager : MonoBehaviour
{
    // INSPECTOR LAUKI

    [Header("AR komponentes")]
    [SerializeField] ARRaycastManager raycastManager;
    [Header("Kalibrācijas UI")]
    [SerializeField] GameObject panelCalibration;
    [SerializeField] TextMeshProUGUI instructionLabel;
    [Header("Vizuālais atskaites punkts")]
    [SerializeField] GameObject markerPrefab;

    List<ARRaycastHit> hits;    // raycast saraksts
    GameObject marker1Instance; // P1
    GameObject marker2Instance; // P2

    // Vai kalibrācija ir pabeigta
    public bool IsCalibrated { get; private set; } = false;

    // Atskaites punkti ARCore pasaules koordinātās
    public Vector3 ReferencePoint1 { get; private set; } // P1 pasaules koordinātas
    public Vector3 ReferencePoint2 { get; private set; } // P2 pasaules koordinātas
    public System.Action OnCalibrationComplete;          // Kalibrācijas pabeigšanas callback

    int pointsCollected = 0;          // 0 = gaida P1, 1 = gaida P2 2 = pabeigts
    bool isActive = false;            // Vai kalibrācija aktīva

    // https://docs.unity3d.com/ScriptReference/Transform.InverseTransformPoint.html
    // https://docs.unity3d.com/ScriptReference/Transform.TransformPoint.html
    Transform referenceTransform; // Atskaites koordinātu sistēmas transform (position, rotation)

    // https://sadra1f.github.io/unity-ar-tutorial/en/placing-objects.html
    // Inicializē raycast sarakstu un atskaites koordinātu sistēmu
    void Awake()
    {
        hits = new List<ARRaycastHit>();
        marker1Instance = null;
        referenceTransform = new GameObject("ReferenceFrame").transform;
    }

    //  Kalibrācijas process, gaida kad pieskarseis
    //  Izsauc ModeManager pēc režīma izvēles

    public void BeginCalibration()
    {
        pointsCollected = 0;    // gaida pirmo punktu
        IsCalibrated = false;   // nav kalibrēts
        isActive = true;        // aktivizē kalibrāciju

        panelCalibration.SetActive(true); // kalibrācijas panelis = on
        instructionLabel.text = "Pieskarieties pirmajam atskaites punktam uz grīdas";
    }


    //  Beidz kalibrāciju un paslēpj UI paneli.
    public void EndCalibration()
    {
        isActive = false; // deaktivizē kalibrāciju
        panelCalibration.SetActive(false); // kalibrācijas panelis = off
    }

    // https://learn.unity.com/tutorial/placing-an-object-on-a-plane-in-ar

    // input apstrāde
    void Update()
    {
        if (!isActive || IsCalibrated)
            return;

        if (Input.touchCount == 0) // nav pieskārienu -> XR Simulation: peles ievade
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) // peles klikšķis un nav uz UI
                PlacePoint(Input.mousePosition); // novieto atskaites punktu
            return;
        }

        Touch touch = Input.GetTouch(0); // pirmais pieskāriens

        if (touch.phase != TouchPhase.Began)
            return;

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) // pieskāriens uz UI -> ignorē
            return;

        PlacePoint(touch.position); // novieto atskaites punktu
    }

    // Raycast uz AR plaknēm un saglabā ref punktus
    // https://sadra1f.github.io/unity-ar-tutorial/en/placing-objects.html
    void PlacePoint(Vector2 touchPosition)
    {
        if (!raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) // ja nav atrasta plakne
        {
            instructionLabel.text = "Nav atrasta plakne";
            return;
        }

        if (pointsCollected == 0) // pirmā punkta izvēle
        {
            ReferencePoint1 = hits[0].pose.position; // pirmā punkta koordinātes
            marker1Instance = CreateAnchor(hits[0]); // izveido vizuālo marķieri
            pointsCollected = 1;


            instructionLabel.text = "Pieskarieties otrajam atskaites punktam uz grīdas";
        }
        else if (pointsCollected == 1) // otrā punkta izvēle
        {
            ReferencePoint2 = hits[0].pose.position; // otrā punkta koordinātes
            marker2Instance = CreateAnchor(hits[0]); // izveido vizuālo marķieri
            pointsCollected = 2;

            ComputeReferenceTransform(); // aprēķina atskaites koordinātu sistēmu
            IsCalibrated = true; // kalibrācija pabeigta

            OnCalibrationComplete?.Invoke();
            EndCalibration(); // beidz kalibrāciju
        }
    }

    // https://www.andreasjakl.com/raycast-anchor-placing-ar-foundation-holograms-part-3/
    // izveido ARAnchor un vizuālo marķieri
    GameObject CreateAnchor(ARRaycastHit hit)
    {
        if (markerPrefab == null)
            return null;

        GameObject instance = Instantiate(markerPrefab, hit.pose.position, Quaternion.identity); // izveido marķieri

        if (!instance.GetComponent<ARAnchor>()) // ja nav ARAnchor komponentes pievieno
            instance.AddComponent<ARAnchor>();

        return instance;
    }

    // https://docs.unity3d.com/ScriptReference/Quaternion.LookRotation.html
    // Izveido koordinātu sistēmu
    void ComputeReferenceTransform()
    {
        Vector3 relativePos = ReferencePoint2 - ReferencePoint1; // virziena vektors no P1 uz P2
        relativePos = Vector3.ProjectOnPlane(relativePos, Vector3.up); // noņem vertikālo komponenti

        referenceTransform.position = ReferencePoint1; // Origin = P1
        referenceTransform.rotation = Quaternion.LookRotation(relativePos, Vector3.up); // Z ass uz P2 ; Y ass uz augšu
    }

    // https://docs.unity3d.com/ScriptReference/Transform.InverseTransformPoint.html
    // ARCore pasaules koordinātas -> lokālās koordinātas
    public Vector3 WorldToLocalPosition(Vector3 worldPosition)
    {
        return referenceTransform.InverseTransformPoint(worldPosition);
    }

    // https://docs.unity3d.com/ScriptReference/Transform.TransformPoint.html
    // Lokālās koordinātas -> pasaules koordinātas
    public Vector3 LocalToWorldPosition(Vector3 localPosition)
    {
        return referenceTransform.TransformPoint(localPosition);
    }

    // https://discussions.unity.com/t/what-is-the-rotation-equivalent-of-inversetransformpoint/45386
    // https://wirewhiz.com/quaternion-tips/
    // https://docs.unity3d.com/ScriptReference/Quaternion.Inverse.html
    // Pasaules rotācija -> lokālā rotācija
    public Quaternion WorldToLocalRotation(Quaternion worldRotation)
    {
        return Quaternion.Inverse(referenceTransform.rotation) * worldRotation;
    }

    // https://wirewhiz.com/quaternion-tips/
    // Lokālā rotācija -> pasaules rotācija
    public Quaternion LocalToWorldRotation(Quaternion localRotation)
    {
        return referenceTransform.rotation * localRotation;
    }
    




    // Paslēpj vai parāda marķierus (ExhibitionStorage)
    public void SetMarkersVisible(bool visible)
    {
        if (marker1Instance != null) marker1Instance.SetActive(visible);
        if (marker2Instance != null) marker2Instance.SetActive(visible);
    }
}