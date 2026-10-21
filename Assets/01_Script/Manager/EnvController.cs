using UnityEngine;

public class EnvController : MonoBehaviour
{
    [SerializeField] private GameObject day;
    [SerializeField] private GameObject night;

    public bool isNightAlready = false;

    void Awake()
    {
        if (day == null) return;
        if (night == null) return;
    }

    public void SwitchToNight()
    {
        if (day == null || night == null) return;

        isNightAlready = true;

        day.SetActive(false);
        night.SetActive(true);
    }

    public void SwitchToDay()
    {
        if (day == null || night == null) return;

        isNightAlready = false;

        day.SetActive(true);
        night.SetActive(false);
    }
}