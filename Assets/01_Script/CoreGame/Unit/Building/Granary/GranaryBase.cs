using UnityEngine;
namespace _01_Script.CoreGame.Unit.Building
{
    public class GranaryCore : BuildingBase
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"Lumbung Utama [{BuildingName}] siap dilindungi! HP: {currentHealth}");
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log("<color=red>GAME OVER! Lumbung Utama telah hancur!</color>");
        }
    }
}