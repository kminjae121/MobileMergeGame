using System.Collections.Generic;
using _Code.GunData;
using _Code.Manager;

namespace Code.Inventories
{
    public class Inventory : MonoSingleton<Inventory>
    {
        private Dictionary<string, List<GunDataSO>> gunDatas = new();

        public void GetGun(GunDataSO gunData)
        {
            if (gunData == null)
                return;

            string gunName = gunData.GunName;

            if (!gunDatas.TryGetValue(gunName, out List<GunDataSO> guns))
            {
                guns = new List<GunDataSO>();
                gunDatas.Add(gunName, guns);
            }

            guns.Add(gunData);
        }

        public void Remove(GunDataSO gunData)
        {
            if (gunData == null)
                return;

            string gunName = gunData.GunName;

            if (!gunDatas.TryGetValue(gunName, out List<GunDataSO> guns))
                return;

            guns.Remove(gunData);

            if (guns.Count == 0)
            {
                gunDatas.Remove(gunName);
            }
        }
    }
}