 // Izstādes faila struktūra:
 //       Exhibition_<date>.arexhibit (ZIP)
 //       _ exhibition.json (objektu pozīcijas, rotācijas, mērogi)
 //       _ models/
 //           _ duck.glb
 //           _ fox.glb
 //           _ ...

 //   https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive
 //   https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchiveentry

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ExhibitionStorage : MonoBehaviour
{
    // INSPECTOR LAUKI

    [Header("Komponentes")]
    [SerializeField] CalibrationManager calibrationManager; // lai konvertētu koordinātes
    [SerializeField] SpawnableManager spawnableManager; // pārvalda izvietotos modeļus
    [SerializeField] ModelLoader modelLoader; // ielāde modeļus no failiem
    [SerializeField] ModeManager modeManager;
    [SerializeField] TextMeshProUGUI exhibitionLoadTimeLabel;

    // Mape, kur tiek atpakots ielādētais izstādes arhīvs
    const string LoadedExhibitionFolder = "loaded_exhibition";

    // SAGLABĀŠANA
    // https://github.com/yasirkula/UnityNativeFilePicker#c-api
    public void SaveExhibition()
    {

        string fileName = "Exhibition_" // faila nosaukums ar timestamp
            + DateTime.Now.ToString("yyyyMMdd_HHmmss")
            + ".arexhibit";

        string outputPath = Path.Combine(Application.persistentDataPath, fileName); // pilnais ceļš iekš storage

        try
        {
            BuildExhibitionArchive(outputPath); // uzbūve zip

            NativeFilePicker.ExportFile(outputPath, (success) => // eksportē uz storage
            {
                Debug.Log(success
                    ? "Izstāde eksportēta: " + fileName
                    : "Fails saglabāts: " + outputPath);
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Kļūda saglabājot izstādi: " + e.Message);
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zipfileextensions.createentryfromfile
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive.createentry
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.streamwriter
    // https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
    private void BuildExhibitionArchive(string outputPath)
    {
        // Savāc izstādes datus un convert uz JSON
        ExhibitionData data = CollectExhibitionData();
        string json = JsonUtility.ToJson(data, true);

        // Ja fails jau eksistē, pārraksta to
        if (File.Exists(outputPath)) File.Delete(outputPath);

        // Izveido ZIP arhīvu
        using (FileStream zipToOpen = new FileStream(outputPath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                ZipArchiveEntry jsonEntry = archive.CreateEntry("exhibition.json"); // pievieno json
                using (StreamWriter writer = new StreamWriter(jsonEntry.Open()))
                {
                    writer.Write(json); // ieraksta json saturu
                }

                // Pievieno katru unikālu .glb failu
                HashSet<string> addedFiles = new HashSet<string>();

                foreach (PlacedObjectData obj in data.objects)
                {
                    if (addedFiles.Contains(obj.modelFileName)) continue;

                    string sourcePath = FindModelSourcePath(obj.modelFileName); // atrod modeļa avota ceļu
                    archive.CreateEntryFromFile(sourcePath, "models/" + obj.modelFileName); // pievieno modeli arhīvā zem models/
                    addedFiles.Add(obj.modelFileName); // atzīmē kā pievienotu
                }
            }

            Debug.Log("ZIP izveidots: " + outputPath);
        }
    }

    // https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
    // Savāc izstādes datus
    private ExhibitionData CollectExhibitionData()
    {
        ExhibitionData data = new ExhibitionData();
        data.exhibitionName = "Exhibition_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        data.creationDate = DateTime.Now.ToString("o");

        List<GameObject> placedObjects = spawnableManager.GetPlacedObjects(); // iegūst visus izvietotos objektus
        foreach (GameObject obj in placedObjects)
        {
            if (obj == null) continue;

            PlacedObjectTag tag = obj.GetComponent<PlacedObjectTag>(); // iegūst PlacedObjectTag, lai uzzinātu modeļa faila nosaukumu

            PlacedObjectData objData = new PlacedObjectData(); // izveido jaunu PlacedObjectData objektu
            objData.modelFileName = tag.modelFileName; // faila nosaukums
            objData.localPosition = calibrationManager.WorldToLocalPosition(obj.transform.position); // konvertē pasaules pozīciju uz lokālo
            objData.localRotation = calibrationManager.WorldToLocalRotation(obj.transform.rotation).eulerAngles; // konvertē pasaules rotāciju uz lokālo un iegūst euler leņķus
            objData.localScale = obj.transform.localScale; // iegūst lokālo mērogu

            data.objects.Add(objData); // pievieno objektu izstādes datiem
        }

        return data; // atgriež pilnu izstādes datu objektu
    }

    // atrod modeļa avota ceļu
    private string FindModelSourcePath(string fileName)
    {
        for (int i = 0; i < modelLoader.modelFilePaths.Count; i++) // iterē cauri ielādēto modeļu ceļiem
        {
            if (Path.GetFileName(modelLoader.modelFilePaths[i]) == fileName)
                return modelLoader.modelFilePaths[i]; // ja faila nosaukums sakrīt, atgriež ceļu
        }
        return null;
    }

    // IELĀDE
    // https://docs.unity3d.com/ScriptReference/JsonUtility.FromJson.html
    public void LoadExhibition()
    {
        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Izstādes ielāde neizdevās");
                return;
            }

            try
            {
                string extractPath = ExtractArchive(path); // atpako arhīvu un iegūst ceļu uz atpakoto mapi
                string jsonPath = Path.Combine(extractPath, "exhibition.json"); // ceļš uz exhibition.json failu

                string json = File.ReadAllText(jsonPath); // nolasa json
                ExhibitionData data = JsonUtility.FromJson<ExhibitionData>(json); // konvertē json uz ExhibitionData objektu

                Debug.Log("Ielādēta izstāde: " + data.exhibitionName
                    + " (" + data.objects.Count + " objekti)");

                // Pēc kalibrācijas izvieto objektus
                calibrationManager.OnCalibrationComplete = () =>
                {
                    _ = PlaceLoadedExhibitionAsync(data, extractPath);
                    calibrationManager.OnCalibrationComplete = null; 
                };
                calibrationManager.BeginCalibration();
            }
            catch (Exception e)
            {
                Debug.LogError("Kluda ieladejot izstadi: " + e.Message);
            }
        }, new string[] { "*/*" });
    }

    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchiveentry
    //  Atpako ZIP arhīvu uz apakšmapi.
    private string ExtractArchive(string archivePath)
    {
        string extractPath = Path.Combine(Application.persistentDataPath, LoadedExhibitionFolder); // ceļš uz atpakoto mapi

        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true); // ja mape jau eksistē, izdzēš to un visu saturu
        Directory.CreateDirectory(extractPath); // izveido jaunu tukšu mapi

        string normalizedPath = Path.GetFullPath(extractPath); //normalizē ceļu
        if (!normalizedPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            normalizedPath += Path.DirectorySeparatorChar;

        using (FileStream zipStream = new FileStream(archivePath, FileMode.Open)) // atver ZIP failu lasīšanai
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in archive.Entries) // iterē cauri visiem arhīva failiem
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;

                string destinationPath = Path.GetFullPath( // izveido ceļu uz atpakoto failu
                    Path.Combine(normalizedPath, entry.FullName));

                string destinationDir = Path.GetDirectoryName(destinationPath); // izveido apakšmapes, ja vajg
                if (!string.IsNullOrEmpty(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                entry.ExtractToFile(destinationPath, true);
            }
        }
        return extractPath;
    }
    // https://docs.unity3d.com/ScriptReference/Quaternion.Euler.html
    // .glb objektu ieladesana un izvietosana
    private async Task PlaceLoadedExhibitionAsync(ExhibitionData data, string extractPath)
    {
        float startTime = Time.realtimeSinceStartup; // izstades ielad laikam

        string modelsDir = Path.Combine(extractPath, "models"); // mape ar modeļiem

        // Katru modeli ielade
        HashSet<string> loadedFileNames = new HashSet<string>();
        foreach (PlacedObjectData obj in data.objects)
        {
            if (loadedFileNames.Contains(obj.modelFileName))
                continue;

            string modelPath = Path.Combine(modelsDir, obj.modelFileName); // ceļš uz modeļa failu

            await modelLoader.LoadGLBFromPathAsync(modelPath, obj.modelFileName); // ielādē modeli
            loadedFileNames.Add(obj.modelFileName); // atzīmē kā ielādētu
        }

        // Katru objektu izvieto
        foreach (PlacedObjectData obj in data.objects)
        {
            GameObject prefab = modelLoader.GetLoadedModelByName(obj.modelFileName); // iegūst ielādētā modeļa prefab no ModelLoader

            Vector3 worldPos = calibrationManager.LocalToWorldPosition(obj.localPosition); // konvertē lokālo pozīciju uz pasaules
            Quaternion worldRot = calibrationManager.LocalToWorldRotation( // konvertē lokālo rotāciju uz pasaules
                Quaternion.Euler(obj.localRotation)); // konvertē euler leņķus uz quaternion

            spawnableManager.SpawnFromData(prefab, worldPos, worldRot, // izvieto objektu pasaulē ar atributiem
                obj.localScale, obj.modelFileName);
        }

        Debug.Log("Izstāde izvietota: " + data.objects.Count + " objekti");

        float loadTime = Time.realtimeSinceStartup - startTime;  // izstades ielad laikam
        if (exhibitionLoadTimeLabel != null)                     // izstades ielad laikam
            exhibitionLoadTimeLabel.text = "Izstāde: " + loadTime.ToString("F2") + "s";

        // Paslēpj AR plaknes un kalibrācijas marķierus
        if (modeManager != null) modeManager.SetPlanesVisible(false);
        calibrationManager.SetMarkersVisible(false);
    }
}