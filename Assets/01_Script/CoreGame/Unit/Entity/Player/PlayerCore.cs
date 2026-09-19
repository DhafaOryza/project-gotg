using UnityEngine;

namespace _01_Script.CoreGame.Unit.Entity.Player
{
    public class PlayerCore : UnitCore
    {
        protected override void Start() => base.Start();
        public override void Die()
        {
            base.Die();
            Debug.Log("Kalah");
        }
        
    }
}