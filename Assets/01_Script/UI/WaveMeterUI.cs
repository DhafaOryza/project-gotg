using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class WaveMeterUI : MonoBehaviour
{
    [Header ("Progress Bar Reference")]
    [SerializeField] private Image progressBarFill;

    [Header ("Wave Marker")]
    [SerializeField] private RectTransform flagContainer;
    [SerializeField] private GameObject flagMarkerPrefabs;

    private WaveController waveController;
    private readonly List<GameObject> activeFlags = new List<GameObject>();

    private void Awake()
    {
        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;
    }

    private void OnEnable()
    {
        waveController = GameManager.Instance?.waveController;
        SetupFlagMarker();
    }

    private void OnDisable()
    {
        ClearFlags();
    }

    private void Update()
    {
        if (waveController == null)
        {
            waveController = GameManager.Instance?.waveController;
            if (waveController == null) return;
        }

        if (waveController.IsWaveRunning)
        {
            float progress = waveController.GetTotalNightProgress();

            if (progressBarFill != null)
                progressBarFill.fillAmount = progress;
        }
    }
    
    public void SetupFlagMarker()
    {
        ClearFlags();
        if (waveController == null)
            waveController = GameManager.Instance?.waveController;
        if (waveController == null)
        {
            Debug.LogError("[WaveMeterUI] waveController tidak ditemukan!");
            return;
        }
        


        var waveData = waveController.GetWaveData();
        if (waveData == null || waveData.waveEnemies == null || waveData.waveEnemies.Count == 0)
        {
            Debug.LogWarning("[WaveMeterUI] waveData masih KOSONG saat SetupFlagMarker dipanggil!");
            return;
        }

        int totalWaves = waveData.waveEnemies.Count;

        for (int i = 1; i <= totalWaves ; i++)
        {
            float wavePercentage = (float)i / totalWaves;

            GameObject flag = Instantiate(flagMarkerPrefabs, flagContainer);
            activeFlags.Add(flag);

            flag.transform.localPosition = Vector3.zero;
            flag.transform.localScale = Vector3.one;

            if (progressBarFill != null && progressBarFill.type == Image.Type.Filled && progressBarFill.fillMethod == Image.FillMethod.Radial360)
            {
                float angle = wavePercentage * 360f;
                flag.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }
        }
    }

    public void ResetWaveMeter()
    {
        ClearFlags();
        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;
    }

    public void ClearFlags()
    {
        for (int i = activeFlags.Count - 1; i >= 0; i--)
        {
            if (activeFlags[i] != null)
                Destroy(activeFlags[i]);
        }
        activeFlags.Clear();
    }
}
