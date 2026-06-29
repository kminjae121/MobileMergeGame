using System;
using UnityEngine;


namespace _Code.GunData
{
    public enum GunType
    {
        None = -1,
        Pistol = 0,
        Rifle = 1,
        Shotgun = 2,
        Launcher = 3,
    }
    [CreateAssetMenu(menuName = "GunData", fileName = "GunDataSO")]
    public class GunDataSO : ScriptableObject
    {
        public GunItem GunItemPrefab;
        public GameObject GunPrefab;
        public GunType GunType;
        public string GunName;
        public int GunIdx;

        private void OnValidate()
        {
            GunIdx = (int)GunType;
        }
    }   
}
