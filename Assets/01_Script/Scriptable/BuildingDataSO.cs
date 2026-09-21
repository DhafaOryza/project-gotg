using _01_Script.Enum;
using UnityEngine;

namespace _01_Script.Scriptable
{
    [CreateAssetMenu(fileName = "New Building Data", menuName = "Granary Defense/Building Data")]
    public class BuildingDataSO : ScriptableObject
    {
        [Header ("Identity & Classification")]

        [SerializeField] private string _buildingID;
        [SerializeField] private string _buildingName;
        [SerializeField] private FactionType _faction = FactionType.BUILDING;
        [SerializeField] private Sprite _buildingIcon;


        [Header ("Base Stats")]
        [SerializeField] private float _maxHp = 100f;

        [Header ("Cost")]
        public int BuildingCost;

        public string BuildingID => _buildingID;
        public string BuildingName => _buildingName;
        public FactionType Faction => _faction;
        public Sprite BuildingICon => _buildingIcon;
        public float MaxHP => _maxHp;
    }
}