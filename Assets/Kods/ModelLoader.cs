using GLTFast;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// https://github.com/atteneder/glTFast/issues/55
// https://github.com/yasirkula/UnityNativeFilePicker
// https://forum.unity.com/threads/normalize-the-size-of-an-object.89886/
// https://forum.unity.com/threads/c-renderer-bounds-and-meshfilter-mesh-bounds-returning-different-sizes.222390/

public class ModelLoader : MonoBehaviour
{
    [SerializeField]
    SpawnableManager spawnableManager;

    [SerializeField]
    TMP_Dropdown modelDropdown;

    [SerializeField]
    ARPlaneManager planeManager;

    [SerializeField]
    TextMeshProUGUI loadTimeLabel;

    // Ielādēto modeļu dati
    private List<GameObject> loadedModels = new List<GameObject>(); // ielādētie modeļi
    private List<string> modelNames = new List<string>(); // modeļu nosaukumi
    public List<float> modelBaseScales = new List<float>(); // modeļu sākotnējie izmēri
    public List<string> modelFilePaths = new List<string>(); // modeļu failu ceļi

    void Start()
    {
        if (modelDropdown != null)
        {
            modelDropdown.ClearOptions();
            modelDropdown.onValueChanged.AddListener(SetActiveModel); // no dropdown izvēlas modeli -> SetActiveModel
        }
    }

    // Failu izvēle ar nativefilepicker
    // Atver Android failu izvēlētāju, filtrējot tikai .glb failus
    // https://github.com/yasirkula/UnityNativeFilePicker

    public void PickGLBFile()
    {
        if (planeManager != null) planeManager.enabled = false;

        NativeFilePicker.PickFile((path) =>
        {
            if (planeManager != null) planeManager.enabled = true;
            if (path == null) return;

            string fileName = Path.GetFileName(path);
            string destPath = Path.Combine(Application.persistentDataPath, fileName);
            File.Copy(path, destPath, true);
            LoadGLBFromPath(destPath, fileName);

        }, new string[] { "model/gltf-binary" });
    }


    public void PickGLBFileAny() // fallback
    {

        NativeFilePicker.PickFile((path) =>
        {

            if (path == null) return;

            string fileName = Path.GetFileName(path);
            if (!fileName.ToLower().EndsWith(".glb")) // pārbauda, vai fails beidzas ar .glb
            {
                Debug.LogError("Nav .glb fails: " + fileName);
                return;
            }

            string destPath = Path.Combine(Application.persistentDataPath, fileName); // izveido ceļu uz persistentDataPath, kur glabāt failu
            File.Copy(path, destPath, true); // kopē failu uz persistentDataPath
            LoadGLBFromPath(destPath, fileName); // ielādē modeli no ceļa, izmantojot faila nosaukumu

        }, new string[] { "*/*" });
    }

    // GLB IELĀDE

    // https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.10/manual/ImportRuntime.html
    public async System.Threading.Tasks.Task LoadGLBFromPathAsync(string filePath, string fileName)
    {

        float startTime = Time.realtimeSinceStartup; // ielades laikam

        byte[] data = File.ReadAllBytes(filePath);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri("file://" + filePath));

        if (success)
        {
            var modelObject = new GameObject("Model_" + fileName); // izveido tukšu GameObject, kurā ielādēt modeli
            await gltf.InstantiateMainSceneAsync(modelObject.transform);

            modelObject.SetActive(false); // sākumā modelis = off

            AddCollider(modelObject); // pievieno collideri
            NormalizeModelSize(modelObject, 1f); // normalizē modeļa izmēru
            loadedModels.Add(modelObject); // pievieno ielādēto modeli
            modelNames.Add(fileName); // pievieno modeļa nosaukumu
            modelFilePaths.Add(filePath); // pievieno modeļa faila ceļu

            UpdateDropdown(); // atjauno dropdown ar ielādētiem modeļiem

            int newIndex = loadedModels.Count - 1; // jaunā modeļa indekss dropdownā
            if (modelDropdown != null) modelDropdown.value = newIndex;
            SetActiveModel(newIndex);

            float loadTime = Time.realtimeSinceStartup - startTime;  // ielades laikam
            if (loadTimeLabel != null)                                 // ielades laikam
                loadTimeLabel.text = "Ielāde: " + loadTime.ToString("F2") + "s";

            Debug.Log("Modelis ielādēts");
        }
        else {
            Debug.LogError("Loading glTF failed!");
        }
    }


    private async void LoadGLBFromPath(string filePath, string fileName)
    {
        await LoadGLBFromPathAsync(filePath, fileName);
    }

    // Dropdown atjaunošana ar ielādētiem modeļiem
    private void UpdateDropdown()
    {
        if (modelDropdown == null) return;
        modelDropdown.ClearOptions();
        modelDropdown.AddOptions(modelNames);
    }

    // Collider pievienošana
    // Bounds aprēķināšana
    // https://gist.github.com/jhocking/e64a5abdcae9b294f02cec56e26fb14b
    private void AddCollider(GameObject assetModel)
    {
        assetModel.SetActive(true);

        var bounds = new Bounds(Vector3.zero, Vector3.zero); // sākotnējie bounds

        var descendants = assetModel.GetComponentsInChildren<Transform>();
        foreach (Transform desc in descendants)
        {
            if (desc.TryGetComponent<Renderer>(out var childRenderer))
            {
                if (bounds.extents == Vector3.zero)
                    bounds = childRenderer.bounds;
                bounds.Encapsulate(childRenderer.bounds);
            }
        }

        var boxCol = assetModel.AddComponent<BoxCollider>(); // pievieno BoxCollider
        boxCol.center = bounds.center - assetModel.transform.position; // BoxCollider centrs = modeļa bounds centrs - modeļa pozīcija
        boxCol.size = bounds.size; // collider izmērs = modeļa bounds izmērs

        assetModel.tag = "Spawnable";
        assetModel.SetActive(false); // modelis = off
    }


    // Aktīvais modelis

    public void SetActiveModel(int index)
    {
        if (index >= 0 && index < loadedModels.Count)
        {
            spawnableManager.spawnablePrefab = loadedModels[index]; // iestata SpawnableManager prefabam ielādēto modeli
            spawnableManager.baseScale = modelBaseScales[index]; // iestata SpawnableManager baseScale ielādētā modeļa sākotnējo izmēru
            spawnableManager.currentModelFileName = modelNames[index]; // iestata SpawnableManager currentModelFileName ielādētā modeļa nosaukumu
        }
    }

    // https://discussions.unity.com/threads/normalize-the-size-of-an-object.89886/
    // https://docs.unity3d.com/ScriptReference/Renderer-bounds.html
    // https://docs.unity3d.com/ScriptReference/Bounds.Encapsulate.html
    // Izmēra normalizācija
    private void NormalizeModelSize(GameObject model, float targetSize = 1f)
    {
        model.SetActive(true);

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(); // https://docs.unity3d.com/ScriptReference/Component.GetComponentsInChildren.html - iegūst visus renderer komponentus
        if (renderers.Length == 0)
        {
            modelBaseScales.Add(1f);
            model.SetActive(false);
            return;
        }
        // aprekina kopejo bounding box
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds); // https://docs.unity3d.com/ScriptReference/Bounds.Encapsulate.html
        }
        // nosaka lielāko dimensiju
        Vector3 size = bounds.size;
        float maxDimension = Mathf.Max(size.x, Mathf.Max(size.y, size.z));

        // mērogo modeli tā, lai lielākā dimensija būtu vienāda ar targetSize
        if (maxDimension > 0)
        {
            float scaleFactor = targetSize / maxDimension;
            model.transform.localScale = Vector3.one * scaleFactor;
            modelBaseScales.Add(scaleFactor);
        }
        else {
            modelBaseScales.Add(1f);
        }
        model.SetActive(false);
    }


    // Priekš exhibitionstorage
    public GameObject GetLoadedModelByName(string fileName)
    {
        for (int i = 0; i < modelNames.Count; i++)
        {
            if (modelNames[i] == fileName) return loadedModels[i];
        }
        return null;
    }
}