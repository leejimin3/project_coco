using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 장애물 스폰 관리 시스템
/// 화면 오른쪽에서 장애물을 생성하여 왼쪽으로 흘러보냄
/// </summary>
public class ObstacleSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleData
    {
        [Tooltip("장애물 프리팹")]
        public GameObject obstaclePrefab;

        [Tooltip("이 장애물의 스폰 가중치 (높을수록 자주 등장)")]
        public float spawnWeight = 1f;

        [HideInInspector] public float individualSpeed; // 개별 속도
        [HideInInspector] public float wavePhaseX; // 물결 위상 X
        [HideInInspector] public float wavePhaseY; // 물결 위상 Y
        [HideInInspector] public Vector3 basePosition; // 기준 위치
    }

    [Header("Spawn Settings")]
    [Tooltip("스폰할 장애물 목록")]
    public ObstacleData[] obstacles;

    [Tooltip("장애물 스폰 간격 (초)")]
    public float spawnInterval = 2f;

    [Tooltip("스폰 간격 랜덤 범위 (±초)")]
    public float spawnIntervalRandomRange = 0.5f;

    [Header("Spawn Position")]
    [Tooltip("X축 스폰 위치 오프셋 (화면 오른쪽 끝 기준)")]
    public float spawnXOffset = 1f;

    [Tooltip("Y축 스폰 위치 (화면 중앙 기준 오프셋)")]
    public float spawnYOffset = 0f;

    [Tooltip("스폰 위치 Y축 랜덤 범위 (±)")]
    public float spawnYRandomRange = 0f;

    [Header("Movement Settings")]
    [Tooltip("장애물 기본 이동 속도")]
    public float moveSpeed = 10f;

    [Tooltip("이동 속도 랜덤 범위 (±)")]
    public float moveSpeedRandomRange = 2f;

    [Tooltip("이동 방향 (-1: 왼쪽, 1: 오른쪽)")]
    public float moveDirection = -1f;

    [Header("Flow Effect (물결 효과)")]
    [Tooltip("물결 효과 활성화")]
    public bool enableFlowEffect = true;

    [Tooltip("물결 X축 흔들림 강도")]
    public float flowWaveAmplitudeX = 0.3f;

    [Tooltip("물결 Y축 흔들림 강도")]
    public float flowWaveAmplitudeY = 0.2f;

    [Tooltip("물결 속도 (높을수록 빠르게 흔들림)")]
    public float flowWaveSpeed = 2f;

    private Camera mainCamera;
    private float spawnX;
    private float despawnX;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<ObstacleFlowData> obstacleFlowDataList = new List<ObstacleFlowData>();
    private float nextSpawnTime;

    // 물결 효과 데이터
    private class ObstacleFlowData
    {
        public GameObject obstacle;
        public float individualSpeed;
        public float wavePhaseX;
        public float wavePhaseY;
        public Vector3 basePosition;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        UpdateSpawnPositions();

        // 첫 스폰 시간 설정
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        // 카메라가 움직일 수 있으므로 매 프레임 업데이트
        UpdateSpawnPositions();

        // 스폰 체크
        if (Time.time >= nextSpawnTime && obstacles.Length > 0)
        {
            SpawnObstacle();

            // 다음 스폰 시간 계산 (랜덤 범위 적용)
            float randomOffset = Random.Range(-spawnIntervalRandomRange, spawnIntervalRandomRange);
            nextSpawnTime = Time.time + spawnInterval + randomOffset;
        }

        // 활성 장애물 이동
        MoveObstacles();

        // 화면 밖으로 나간 장애물 제거
        CleanupObstacles();
    }

    /// <summary>
    /// 스폰 위치 업데이트
    /// </summary>
    private void UpdateSpawnPositions()
    {
        if (mainCamera != null)
        {
            // 화면 오른쪽 끝 위치 계산
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            spawnX = mainCamera.transform.position.x + screenWidth / 2f + spawnXOffset;
            despawnX = mainCamera.transform.position.x - screenWidth / 2f - 1f;
        }
    }

    /// <summary>
    /// 장애물 스폰
    /// </summary>
    private void SpawnObstacle()
    {
        // 가중치 기반 랜덤 선택
        GameObject selectedPrefab = SelectRandomObstacle();
        if (selectedPrefab == null) return;

        // 스폰 위치 계산
        float spawnY = (mainCamera != null ? mainCamera.transform.position.y : 0f) + spawnYOffset;

        // Y축 랜덤 오프셋 적용
        if (spawnYRandomRange > 0)
        {
            spawnY += Random.Range(-spawnYRandomRange, spawnYRandomRange);
        }

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        // 장애물 생성
        GameObject obstacle = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        obstacle.transform.SetParent(transform);
        activeObstacles.Add(obstacle);

        // 물결 효과 데이터 생성
        ObstacleFlowData flowData = new ObstacleFlowData
        {
            obstacle = obstacle,
            individualSpeed = moveSpeed + Random.Range(-moveSpeedRandomRange, moveSpeedRandomRange),
            wavePhaseX = Random.Range(0f, Mathf.PI * 2f), // 랜덤 시작 위상
            wavePhaseY = Random.Range(0f, Mathf.PI * 2f),
            basePosition = spawnPosition
        };
        obstacleFlowDataList.Add(flowData);

        Debug.Log($"Obstacle spawned at X={spawnX:F2}, Y={spawnY:F2}, Speed={flowData.individualSpeed:F2}");
    }

    /// <summary>
    /// 가중치 기반 랜덤 장애물 선택
    /// </summary>
    private GameObject SelectRandomObstacle()
    {
        if (obstacles.Length == 0) return null;

        // 총 가중치 계산
        float totalWeight = 0f;
        foreach (var obstacle in obstacles)
        {
            totalWeight += obstacle.spawnWeight;
        }

        // 랜덤 값 생성
        float randomValue = Random.Range(0f, totalWeight);

        // 가중치에 따라 선택
        float currentWeight = 0f;
        foreach (var obstacle in obstacles)
        {
            currentWeight += obstacle.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return obstacle.obstaclePrefab;
            }
        }

        // 기본값 (첫 번째)
        return obstacles[0].obstaclePrefab;
    }

    /// <summary>
    /// 활성 장애물 이동 (물결 효과 포함)
    /// </summary>
    private void MoveObstacles()
    {
        for (int i = 0; i < obstacleFlowDataList.Count; i++)
        {
            ObstacleFlowData flowData = obstacleFlowDataList[i];
            if (flowData.obstacle != null)
            {
                // 기본 이동
                flowData.basePosition.x += moveDirection * flowData.individualSpeed * Time.deltaTime;

                Vector3 finalPosition = flowData.basePosition;

                // 물결 효과 적용
                if (enableFlowEffect)
                {
                    // X축 흔들림 (sin 파동)
                    float waveOffsetX = Mathf.Sin(Time.time * flowWaveSpeed + flowData.wavePhaseX) * flowWaveAmplitudeX;
                    finalPosition.x += waveOffsetX;

                    // Y축 흔들림 (cos 파동 - 다른 패턴)
                    float waveOffsetY = Mathf.Cos(Time.time * flowWaveSpeed + flowData.wavePhaseY) * flowWaveAmplitudeY;
                    finalPosition.y += waveOffsetY;
                }

                flowData.obstacle.transform.position = finalPosition;
            }
        }
    }

    /// <summary>
    /// 화면 밖으로 나간 장애물 제거
    /// </summary>
    private void CleanupObstacles()
    {
        for (int i = obstacleFlowDataList.Count - 1; i >= 0; i--)
        {
            if (obstacleFlowDataList[i].obstacle == null)
            {
                obstacleFlowDataList.RemoveAt(i);
                activeObstacles.RemoveAt(i);
                continue;
            }

            float obstacleX = obstacleFlowDataList[i].basePosition.x;

            // 왼쪽으로 이동 중일 때 왼쪽 끝을 벗어나면 제거
            if (moveDirection < 0 && obstacleX < despawnX)
            {
                GameObject obstacle = obstacleFlowDataList[i].obstacle;
                obstacleFlowDataList.RemoveAt(i);
                activeObstacles.Remove(obstacle);
                Destroy(obstacle);
                Debug.Log("Obstacle despawned (left screen)");
            }
            // 오른쪽으로 이동 중일 때 오른쪽 끝을 벗어나면 제거
            else if (moveDirection > 0 && obstacleX > spawnX)
            {
                GameObject obstacle = obstacleFlowDataList[i].obstacle;
                obstacleFlowDataList.RemoveAt(i);
                activeObstacles.Remove(obstacle);
                Destroy(obstacle);
                Debug.Log("Obstacle despawned (right screen)");
            }
        }
    }

    private void OnDestroy()
    {
        // 모든 활성 장애물 제거
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        activeObstacles.Clear();
        obstacleFlowDataList.Clear();
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            float tempSpawnX = mainCamera.transform.position.x + screenWidth / 2f + spawnXOffset;
            float spawnY = mainCamera.transform.position.y + spawnYOffset;

            // 스폰 위치 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(tempSpawnX, spawnY, 0f), 0.5f);

            // 스폰 Y 범위 표시
            if (spawnYRandomRange > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    new Vector3(tempSpawnX, spawnY - spawnYRandomRange, 0f),
                    new Vector3(tempSpawnX, spawnY + spawnYRandomRange, 0f)
                );
            }

            // 물결 효과 범위 표시
            if (enableFlowEffect)
            {
                Gizmos.color = Color.cyan;
                // X축 흔들림 범위
                Gizmos.DrawWireCube(
                    new Vector3(tempSpawnX - flowWaveAmplitudeX, spawnY, 0f),
                    new Vector3(flowWaveAmplitudeX * 2f, 0.2f, 0f)
                );
            }
        }
    }
}
