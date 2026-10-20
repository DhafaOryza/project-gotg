using System;

/// <summary>
/// Apa pun yang punya "nyawa" yang bisa ditampilkan di HealthBar:
/// HP untuk BaseEntity (player, musuh), durability untuk BuildingBase.
/// </summary>
public interface IHealthSource
{
    int CurrentHealth { get; }
    int MaxHealth { get; }

    /// <summary>Dipicu setiap nilainya berubah: (current, max).</summary>
    event Action<int, int> OnHealthChanged;
}