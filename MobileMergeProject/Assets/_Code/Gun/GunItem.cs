using _Code.Manager;
using UnityEngine;

namespace _Code.GunData
{
    public class GunItem : MonoBehaviour
    {
        [SerializeField] private GunDataSO gunData;

        private bool isMerging;

        public GunDataSO GunData => gunData;

        private void OnEnable()
        {
            isMerging = false;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (isMerging)
                return;

            if (!other.gameObject.TryGetComponent(out GunItem otherGunItem))
                return;

            if (otherGunItem.isMerging)
                return;

            if (gunData == null || otherGunItem.gunData == null)
                return;

            if (gunData.GunType != otherGunItem.gunData.GunType)
                return;
            
            if (GetInstanceID() > otherGunItem.GetInstanceID())
                return;

            Debug.Log("Test");
            isMerging = true;
            otherGunItem.isMerging = true;

            Vector3 spawnPosition = (transform.position + otherGunItem.transform.position) * 0.5f;

            bool isSpawned = GunMergeManager.Instance.SpawnNextGun(gunData, spawnPosition);

            if (!isSpawned)
            {
                isMerging = false;
                otherGunItem.isMerging = false;
                return;
            }

            otherGunItem.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}