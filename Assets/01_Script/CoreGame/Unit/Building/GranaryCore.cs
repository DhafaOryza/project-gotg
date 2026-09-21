using UnityEngine;
namespace _01_Script.CoreGame.Unit.Building
{
    public class GranaryCore : BuildingCore
    {
        protected override void Start()
        {
            base.Start();
            Debug.Log($"Lumbung Utama [{BuildingName}] siap dilindungi! HP: {_currentHealth}");
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log("<color=red>GAME OVER! Lumbung Utama telah hancur!</color>");
        }
    }
}