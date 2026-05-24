// https://github.com/Unity-Technologies/arfoundation-samples/issues/1086


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class AppReset : MonoBehaviour
{
    public void ResetApp()
    {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
