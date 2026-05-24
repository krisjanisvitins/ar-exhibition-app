//    https://docs.unity3d.com/ScriptReference/JsonUtility.html
// --name
// --date
// --objects[]

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] // drīkst saglabāt json

// Izstades dati
public class ExhibitionData
{
    public string exhibitionName;      // Nosaukums
    public string creationDate;        // Datums
    public List<PlacedObjectData> objects = new List<PlacedObjectData>(); // visi AR modeļi izstādē
}

// Objekta dati
[Serializable]
public class PlacedObjectData 
{
    public string modelFileName;    // modeļa faila nosaukums
    public Vector3 localPosition;   // modeļa pozīcija izstādes lokālajā koordinātu sistēmā
    public Vector3 localRotation;   // modeļa rotācija izstādes lokālajā koordinātu sistēmā
    public Vector3 localScale;      // izmērs
}