using System;
using System.Collections.Generic;
using _Code.GunData;
using UnityEngine;

namespace _Code.Manager
{
    public class GunMergeManager : MonoSingleton<GunMergeManager>
    {
        [SerializeField] private List<GunDataSO> gunDatas;
        
        private readonly Dictionary<GunType, GunDataSO> gunDataDict = new();

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
                return;

            gunDataDict.Clear();

            foreach (var gunData in gunDatas)
            {
                if (gunData == null)
                    continue;

                gunDataDict[gunData.GunType] = gunData;
            }
        }

        public GunDataSO GetNextGunType(GunDataSO gunData)
        {
            if (gunData == null)
                return null;

            int nextIdx = (int)gunData.GunType + 1;

            if (!Enum.IsDefined(typeof(GunType), nextIdx))
                return null;

            GunType nextGunType = (GunType)nextIdx;

            if (!gunDataDict.TryGetValue(nextGunType, out GunDataSO nextGunData))
                return null;

            return nextGunData;
        }

        public bool SpawnNextGun(GunDataSO gunData, Vector3 position)
        {
            GunDataSO nextGunData = GetNextGunType(gunData);

            if (nextGunData == null)
                return false;

            if (nextGunData.GunItemPrefab == null)
            {
                Debug.LogWarning($"{nextGunData.name}에 GunItemPrefab이 없습니다.");
                return false;
            }

            Instantiate(nextGunData.GunItemPrefab, position, Quaternion.identity);
            return true;
        }
    }
}