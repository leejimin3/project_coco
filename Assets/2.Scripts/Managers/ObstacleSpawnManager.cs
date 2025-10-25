using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 장애물 스폰 관리 시스템 (Lane 기반)
/// 각 Lane마다 다른 장애물, 속도, 위치 설정
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
    }

    [System.Serializable]
    public class Lane
    {
        [Header("Lane Info")]
        [Tooltip("Lane 이름")]
        public string laneName = "Lane 1";

        [Header("Obstacles")]
        [Tooltip("이 Lane에서 스폰할 장애물 목록")]
        public ObstacleData[] obstacles;

        [Header("Spawn Settings")]
        [Tooltip("장애물 스폰 간격 (초)")]
        public float spawnInterval = 2f;

        [Tooltip("스폰 간격 랜덤 범위 (±초)")]
        public float spawnIntervalRandomRange = 0.5f;

        [Header("Position")]
        [Tooltip("Y축 스폰 위치 (화면 중앙 기준 오프셋)")]
        public float spawnYOffset = 0f;

        [Tooltip("스폰 위치 Y축 랜덤 범위 (±)")]
        public float spawnYRandomRange = 0f;

        [Header("Movement")]
        [Tooltip("장애물 기본 이동 속도")]
        public float moveSpeed = 10f;

        [Tooltip("이동 속도 랜덤 범위 (±)")]
        public float moveSpeedRandomRange = 2f;

        [Header("Flow Effect")]
        [Tooltip("물결 효과 활성화")]
        public bool enableFlowEffect = true;

        [Tooltip("물결 X축 흔들림 강도")]
        public float flowWaveAmplitudeX = 0.3f;

        [Tooltip("물결 Y축 흔들림 강도")]
        public float flowWaveAmplitudeY = 0.2f;

        [Tooltip("물결 속도")]
        public float flowWaveSpeed = 2f;

        [HideInInspector] public float nextSpawnTime;
    }

    [Header("Lane Settings")]
    [Tooltip("Lane 목록")]
    public Lane[] lanes;

    [Header("Global Settings")]
    [Tooltip("X축 스폰 위치 오프셋 (화면 오른쪽 끝 기준)")]
    public float spawnXOffset = 1f;

    [Tooltip("이동 방향 (-1: 왼쪽, 1: 오른쪽)")]
    public float moveDirection = -1f;

    [Header("Debug")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLogs = true;

    private Camera mainCamera;
    private float spawnX;
    private float despawnX;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<ObstacleFlowData> obstacleFlowDataList = new List<ObstacleFlowData>();

    // 물결 효과 데이터
    private class ObstacleFlowData
    {
        public GameObject obstacle;
        public float individualSpeed;
        public float wavePhaseX;
        public float wavePhaseY;
        public Vector3 basePosition;
        public bool useFlowEffect;
        public float flowAmplitudeX;
        public float flowAmplitudeY;
        public float flowSpeed;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        UpdateSpawnPositions();

        // 각 Lane의 첫 스폰 시간 설정
        foreach (var lane in lanes)
        {
            lane.nextSpawnTime = Time.time + lane.spawnInterval;
        }
    }

    private void Update()
    {
        // 카메라가 움직일 수 있으므로 매 프레임 업데이트
        UpdateSpawnPositions();

        // 각 Lane별로 스폰 체크
        for (int i = 0; i < lanes.Length; i++)
        {
            Lane lane = lanes[i];
            if (Time.time >= lane.nextSpawnTime && lane.obstacles.Length > 0)
            {
                SpawnObstacleInLane(lane, i);

                // 다음 스폰 시간 계산
                float randomOffset = Random.Range(-lane.spawnIntervalRandomRange, lane.spawnIntervalRandomRange);
                lane.nextSpawnTime = Time.time + lane.spawnInterval + randomOffset;
            }
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
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            spawnX = mainCamera.transform.position.x + screenWidth / 2f + spawnXOffset;
            despawnX = mainCamera.transform.position.x - screenWidth / 2f - 1f;
        }
    }

    /// <summary>
    /// 특정 Lane에 장애물 스폰
    /// </summary>
    private void SpawnObstacleInLane(Lane lane, int laneIndex)
    {
        // 가중치 기반 랜덤 선택
        GameObject selectedPrefab = SelectRandomObstacle(lane.obstacles);
        if (selectedPrefab == null) return;

        // 스폰 위치 계산
        float spawnY = (mainCamera != null ? mainCamera.transform.position.y : 0f) + lane.spawnYOffset;

        // Y축 랜덤 오프셋 적용
        if (lane.spawnYRandomRange > 0)
        {
            spawnY += Random.Range(-lane.spawnYRandomRange, lane.spawnYRandomRange);
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
            individualSpeed = lane.moveSpeed + Random.Range(-lane.moveSpeedRandomRange, lane.moveSpeedRandomRange),
            wavePhaseX = Random.Range(0f, Mathf.PI * 2f),
            wavePhaseY = Random.Range(0f, Mathf.PI * 2f),
            basePosition = spawnPosition,
            useFlowEffect = lane.enableFlowEffect,
            flowAmplitudeX = lane.flowWaveAmplitudeX,
            flowAmplitudeY = lane.flowWaveAmplitudeY,
            flowSpeed = lane.flowWaveSpeed
        };
        obstacleFlowDataList.Add(flowData);

        if (showDebugLogs)
        {
            Debug.Log($"[{lane.laneName}] Obstacle spawned: {selectedPrefab.name} at Y={spawnY:F2}, Speed={flowData.individualSpeed:F2}");
        }
    }

    /// <summary>
    /// 가중치 기반 랜덤 장애물 선택
    /// </summary>
    private GameObject SelectRandomObstacle(ObstacleData[] obstacles)
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
                if (flowData.useFlowEffect)
                {
                    // X축 흔들림 (sin 파동)
                    float waveOffsetX = Mathf.Sin(Time.time * flowData.flowSpeed + flowData.wavePhaseX) * flowData.flowAmplitudeX;
                    finalPosition.x += waveOffsetX;

                    // Y축 흔들림 (cos 파동 - 다른 패턴)
                    float waveOffsetY = Mathf.Cos(Time.time * flowData.flowSpeed + flowData.wavePhaseY) * flowData.flowAmplitudeY;
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
            }
            // 오른쪽으로 이동 중일 때 오른쪽 끝을 벗어나면 제거
            else if (moveDirection > 0 && obstacleX > spawnX)
            {
                GameObject obstacle = obstacleFlowDataList[i].obstacle;
                obstacleFlowDataList.RemoveAt(i);
                activeObstacles.Remove(obstacle);
                Destroy(obstacle);
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

        if (mainCamera != null && lanes != null)
        {
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;
            float tempSpawnX = mainCamera.transform.position.x + screenWidth / 2f + spawnXOffset;

            // 각 Lane의 스폰 위치 표시
            for (int i = 0; i < lanes.Length; i++)
            {
                Lane lane = lanes[i];
                float spawnY = mainCamera.transform.position.y + lane.spawnYOffset;

                // 스폰 위치 표시 (Lane별로 다른 색상)
                Gizmos.color = i == 0 ? Color.green : Color.cyan;
                Gizmos.DrawWireSphere(new Vector3(tempSpawnX, spawnY, 0f), 0.3f);

                // Lane 이름 표시 위치
                Vector3 labelPos = new Vector3(tempSpawnX + 0.5f, spawnY, 0f);

                // Y 범위 표시
                if (lane.spawnYRandomRange > 0)
                {
                    Gizmos.color = i == 0 ? Color.yellow : Color.magenta;
                    Gizmos.DrawLine(
                        new Vector3(tempSpawnX, spawnY - lane.spawnYRandomRange, 0f),
                        new Vector3(tempSpawnX, spawnY + lane.spawnYRandomRange, 0f)
                    );
                }
            }
        }
    }
}
