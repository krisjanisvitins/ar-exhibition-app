using UnityEngine;
// https://docs.unity3d.com/ScriptReference/AssetDatabase.CreateAsset.html
// https://docs.unity3d.com/560/Documentation/Manual/AssetDatabase.html
// https://docs.unity3d.com/ScriptReference/Shader.Find.html

using UnityEngine;

public class FindShader : MonoBehaviour
{
    void Start()
    {
        Shader shader = Shader.Find("glTF/PbrMetallicRoughness");
        if (shader == null)
        {
            Debug.LogError("Shader not found");
            return;
        }

        Material material = new Material(shader);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(material, "Assets/Resources/GltfPbrReference.mat");
        Debug.Log(UnityEditor.AssetDatabase.GetAssetPath(material));
#endif
    }
}
