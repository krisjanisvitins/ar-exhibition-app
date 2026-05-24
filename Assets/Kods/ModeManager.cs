// Kontrolē režīmus, UI paneļu pārslēgšanu, kalibrācijas procesa sākšanu, AR plane detection on/off, pāreju starp artist un viewer režīmiem
using UnityEngine;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    // INSPECTOR LAUKI

    [Header("UI Paneļi")]
    [SerializeField] GameObject panelModeSelect;
    [SerializeField] GameObject panelArtistUI;
    [SerializeField] GameObject panelViewerUI;
    [SerializeField] GameObject panelDisclaimer;
    [SerializeField] GameObject panelModelLoad;

    [Header("Komponentes")]
    [SerializeField] SpawnableManager spawnableManager;
    [SerializeField] CalibrationManager calibrationManager;
    [SerializeField] ExhibitionStorage exhibitionStorage;

    [SerializeField] UnityEngine.XR.ARFoundation.ARPlaneManager planeManager;

    // Starts
    void Start()
    {
        panelModeSelect.SetActive(true);    // režīma panelis = off
        panelArtistUI.SetActive(false);     // artist panelis = on
        panelViewerUI.SetActive(false);     // skatītāja panelis = off
        panelDisclaimer.SetActive(false);   // disclaimer panelis = off
        panelModelLoad.SetActive(false);    // modelu ielādes panelis = off
        spawnableManager.enabled = false;   // modeļu izvietošana = off
        planeManager.enabled = false;       // plane detection = off
    }
    // Artist panelis
    public void SelectArtistMode()
    {
        panelModeSelect.SetActive(false);   // režīma panelis = off
        panelDisclaimer.SetActive(true);    // disclaimer panelis = on
    }

    //  Ja lietotājs piekrīt disclaimer -> upload models
    public void AcceptDisclaimer()
    {
        panelDisclaimer.SetActive(false);   // disclaimer panelis = off
        panelModelLoad.SetActive(true);     // modelu ielādes panelis = on
    }
    // ModelLoader paziņo, kad modeļi ir ielādēti un gatavi
    public void OnModelsReady()
    {
        panelModelLoad.SetActive(false);    // modelu ielādes panelis = off
        planeManager.enabled = true;        // plane detection = on  
        calibrationManager.OnCalibrationComplete = OnArtistCalibrationDone; // CalibrationManager pabeidz kalibrāciju -> OnArtistCalibrationDone
        calibrationManager.BeginCalibration();                              // CalibrationManager sāk kalibrāciju
    }
    // Ja lietotājs noraida disclaimer -> atgriežas pie režīma izvēles paneļa
    public void DeclineDisclaimer()
    {
        panelDisclaimer.SetActive(false);   // disclaimer panelis = off
        panelModeSelect.SetActive(true);    // režīma panelis = on
    }
    // CalibrationManager pabeidzis kalibrāciju
    private void OnArtistCalibrationDone()
    {
        panelArtistUI.SetActive(true);      // artist panelis = on
        spawnableManager.enabled = true;    // modeļu izvietošana = on
        calibrationManager.OnCalibrationComplete = null; // noņem callback
    }

    // Skatītāja panelis
    public void SelectViewerMode()
    {
        planeManager.enabled = true;        // plane detection = on
        panelModeSelect.SetActive(false);   // režīma panelis = off
        panelViewerUI.SetActive(true);      // skatītāja panelis = on
        spawnableManager.enabled = false;   // modeļu izvietošana = off
        exhibitionStorage.LoadExhibition(); // ExhibitionStorage ielādē izstādi
    }


    // https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/features/plane-detection/arplanemanager.html
    // https://github.com/Unity-Technologies/arfoundation-samples/issues/880
    // https://medium.com/@sean.duggan/unity-ar-disabling-the-planes-and-placement-fdb65bf05c33
    // Parāda vai paslēpj virsmas
    public void SetPlanesVisible(bool visible)
    {
        if (planeManager == null) return;

        // Paslēpj visas pašreizējās plaknes
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(visible);
        }

        // Ieslēdz vai izslēdz jaunu plakņu atpazīšanu
        planeManager.enabled = visible;
        if (!visible)
        {
            planeManager.trackablesChanged.AddListener(OnPlanesChangedHideNew);
        }
        else {
            planeManager.trackablesChanged.RemoveListener(OnPlanesChangedHideNew);
        }
    }
    // Ja plane detection izslēgts, paslēpj visas jaunas atpazītās plaknes
    private void OnPlanesChangedHideNew(
        UnityEngine.XR.ARFoundation.ARTrackablesChangedEventArgs<UnityEngine.XR.ARFoundation.ARPlane> args)
    {
        foreach (var plane in args.added)
        {
            plane.gameObject.SetActive(false);
        }
    }
}