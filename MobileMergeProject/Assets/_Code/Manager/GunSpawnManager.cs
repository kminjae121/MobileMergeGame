using System;
using Unity.XR.OpenVR;
using UnityEngine;
using PlayerInputSO = _Code.InputSystem.PlayerInput;
using Random = UnityEngine.Random;

namespace _Code.Manager
{
    public class GunSpawnManager : MonoSingleton<GunSpawnManager>
    {
        [Header("Input")]
        [SerializeField] private PlayerInputSO playerInput;

        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] spawnPrefabs;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject spawningObj;

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

            playerInput.TorchReleaseEvent += HandleTorchPress;
        }

        private void OnDisable()
        {
            if (playerInput != null)
                playerInput.TorchReleaseEvent -= HandleTorchPress;
        }

        private void Update()
        {
            if (GameManager.Instance.PlayMode == PlayMode.Moving)
                return;
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(playerInput.TorchValue.x,0, 0));

            worldPosition.y = 5;
            worldPosition.z = 0;

            spawningObj.transform.position = worldPosition;
        }

        private void HandleTorchPress()
        {
            SpawnRandomObject();
        }

        private void SpawnRandomObject()
        {
            if (GameManager.Instance.PlayMode == PlayMode.Moving)
                return;
            
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
            
            Instantiate(randomPrefab, spawningObj.transform.position, Quaternion.identity);
        }
    }
}