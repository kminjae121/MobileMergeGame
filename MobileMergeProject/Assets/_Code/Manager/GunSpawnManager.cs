using Unity.XR.OpenVR;
using UnityEngine;
using PlayerInputSO = _Code.InputSystem.PlayerInput;

namespace _Code.Manager
{
    public class GunSpawnManager : MonoSingleton<GunSpawnManager>
    {
        [Header("Input")]
        [SerializeField] private PlayerInputSO playerInput;

        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] spawnPrefabs;

        [SerializeField] private Camera mainCamera;

        [Tooltip("2D 오브젝트가 생성될 Z 위치")]
        [SerializeField] private float spawnZ = 0f;

        protected override void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput SO가 연결되지 않았습니다.");
                return;
            }

            playerInput.TorchPressEvent += HandleTorchPress;
        }

        private void OnDisable()
        {
            if (playerInput != null)
                playerInput.TorchPressEvent -= HandleTorchPress;
        }

        private void HandleTorchPress()
        {
            Vector2 screenPosition = playerInput.TorchValue;
            SpawnRandomObject(screenPosition);
        }

        private void SpawnRandomObject(Vector2 screenPosition)
        {
            if (spawnPrefabs == null || spawnPrefabs.Length == 0)
            {
                Debug.LogWarning("생성할 프리팹이 없습니다. Spawn Prefabs 배열에 프리팹을 넣어주세요.");
                return;
            }

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("Main Camera를 찾을 수 없습니다.");
                return;
            }

            GameObject randomPrefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];

            if (randomPrefab == null)
            {
                Debug.LogWarning("Spawn Prefabs 배열 안에 비어있는 프리팹이 있습니다.");
                return;
            }

            float distanceFromCamera = spawnZ - mainCamera.transform.position.z;

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    distanceFromCamera
                )
            );

            worldPosition.z = spawnZ;

            Instantiate(randomPrefab, worldPosition, Quaternion.identity);
        }
    }
}