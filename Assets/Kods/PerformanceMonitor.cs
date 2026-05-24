// https://gist.github.com/mstevenson/5103365


using System.Collections;
using UnityEngine;
using TMPro;

public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statsLabel;

    private float count;

    private IEnumerator Start()
    {
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            if (statsLabel != null)
                statsLabel.text = "FPS: " + Mathf.Round(count);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
