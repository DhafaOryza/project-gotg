using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; set; }

    [SerializeField] private EntityDataSO playerData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != null)
        {
            return;
        }
    }

    public void GetOrCreate()
    {
        
    }

    #region Player Data

    public void SetPlayerData(EntityDataSO setPlayerData)
    {
        playerData = setPlayerData;
    }

    public EntityDataSO GetPlayerData()
    {
        return playerData;
    }

    #endregion
}