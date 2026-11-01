using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int _money = 0;

    public int Money => _money;

    public event Action OnChange;

    void OnValidate()
    {
        OnChange?.Invoke();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        _money += amount;
        OnChange?.Invoke();
    }

    /// <summary>Kurangi uang. Return false (tanpa mengubah apa pun) kalau uang tidak cukup.</summary>
    public bool SpendMoney(int amount)
    {
        if (amount < 0 || amount > _money) return false;

        _money -= amount;

        // Dulu tidak memicu event, jadi UI (teks uang, tombol upgrade) tidak ikut berubah setelah membeli.
        OnChange?.Invoke();
        return true;
    }
}