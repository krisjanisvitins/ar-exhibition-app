using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
// https://tutorialsforar.com/how-to-calculate-plane-area-in-ar-using-unity-and-ar-foundation/
// https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/manual/plane-manager.html
// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.orderbydescending
public class PlaneFilter : MonoBehaviour
{
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] int maxVisiblePlanes = 10; // max plakņu skaits
    private float CalculatePlaneArea(ARPlane plane) // plaknes laukuma aprēķins
    {
        return plane.size.x * plane.size.y;
    }
    void Update()
    {
        if (planeManager == null || !planeManager.enabled)
            return;
        // savāc visas atpazītās plaknes sarakstā
        var planes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables) // iterē cauri visām atpazītajām plaknēm
        {
            planes.Add(plane);
        }
        // sakārto plaknes pēc laukuma dilstošā secībā un saglabā
        var sortedPlanes = planes
            .OrderByDescending(plane => CalculatePlaneArea(plane))
            .ToList();

        // aktivizē tikai top maxVisiblePlanes plaknes, pārējās paslēpj
        for (int i = 0; i < sortedPlanes.Count; i++)
        {
            sortedPlanes[i].gameObject.SetActive(i < maxVisiblePlanes);
        }
    }
}
