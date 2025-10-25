using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 장애물 Lane 기반 스폰 관리 시스템
/// 2개 Lane에 각각 다른 장애물 배정
/// weight = 충격강도 = 회전각 = 출현간격
/// </summary>
public class ObstacleSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleData
    {
        [Tooltip("장애물 프리팹")]
        public GameObject obstaclePrefab;

        [Tooltip("장애물의 무게 (충격강도=회전각, 높을수록 출현간격 증가)")]
        public float weight = 10f;
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

        [Header("Position")]
        [Tooltip("Y축 스폰 위치 오프셋 (화면 중앙 기준)")]
        public float spawnYOffset = 0f;

        [HideInInspector] public Dictionary<int, float> nextSpawnTimes = new Dictionary<int, float>();
    }

    [Header("Lane Settings")]
    [Tooltip("2개의 Lane 설정")]
    public Lane[] lanes = new Lane[2];

    [Header("Spawn Settings")]
    [Tooltip("기본 스폰 간격 배율 (weight * 이 값 = 실제 간격(초))")]
    public float spawnIntervalMultiplier = 0.1f;

    [Tooltip("스폰 간격 랜덤 범위 비율 (0~1, 0.2 = ±20%)")]
    public float spawnIntervalRandomRatio = 0.2f;

    [Header("Position Settings")]
    [Tooltip("X축 스폰 위치 오프셋 (화면 오른쪽 끝 기준)")]
    public float spawnXOffset = 1f;

    [Header("Movement Settings")]
    [Tooltip("기본 이동 속도")]
    public float baseSpeed = 5f;

    [Tooltip("무게 기반 속도 배율 (weight * 이 값 = 추가 속도)")]
    public float weightSpeedMultiplier = 0.2f;

    [Tooltip("이동 속도 랜덤 범위 (±)")]
    public float moveSpeedRandomRange = 2f;

    [Tooltip("이동 방향 (-1: 왼쪽, 1: 오른쪽)")]
    public float moveDirection = -1f;

    [Header("Flow Effect")]
    [Tooltip("물결 효과 활성화")]
    public bool enableFlowEffect = true;

    [Tooltip("물결 X축 흔들림 강도")]
    public float flowWaveAmplitudeX = 0.3f;

    [Tooltip("물결 Y축 흔들림 강도")]
    public float flowWaveAmplitudeY = 0.2f;

    [Tooltip("물결 속도")]
    public float flowWaveSpeed = 2f;

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

        // 각 Lane의 각 장애물별 첫 스폰 시간 설정
        foreach (var lane in lanes)
        {
            for (int i = 0; i < lane.obstacles.Length; i++)
            {
                float interval = CalculateSpawnInterval(lane.obstacles[i].weight);
                lane.nextSpawnTimes[i] = Time.time + interval;
            }
        }
    }

    private void Update()
    {
        // 카메라가 움직일 수 있으므로 매 프레임 업데이트
        UpdateSpawnPositions();

        // 각 Lane별로 스폰 체크
        for (int laneIdx = 0; laneIdx < lanes.Length; laneIdx++)
        {
            Lane lane = lanes[laneIdx];

            for (int obstacleIdx = 0; obstacleIdx < lane.obstacles.Length; obstacleIdx++)
            {
                if (lane.obstacles[obstacleIdx].obstaclePrefab == null) continue;

                if (lane.nextSpawnTimes.ContainsKey(obstacleIdx) && Time.time >= lane.nextSpawnTimes[obstacleIdx])
                {
                    SpawnObstacle(lane, obstacleIdx, laneIdx);

                    // 다음 스폰 시간 계산 (weight 기반)
                    float interval = CalculateSpawnInterval(lane.obstacles[obstacleIdx].weight);
                    lane.nextSpawnTimes[obstacleIdx] = Time.time + interval;
                }
            }
        }

        // 활성 장애물 이동
        MoveObstacles();

        // 화면 밖으로 나간 장애물 제거
        CleanupObstacles();
    }

    /// <summary>
    /// weight를 기반으로 스폰 간격 계산
    /// weight가 높을수록 간격이 길어짐
    /// </summary>
    private float CalculateSpawnInterval(float weight)
    {
        float baseInterval = weight * spawnIntervalMultiplier;
        float randomRange = baseInterval * spawnIntervalRandomRatio;
        float randomOffset = Random.Range(-randomRange, randomRange);
        return baseInterval + randomOffset;
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
    /// 특정 Lane의 특정 장애물 스폰
    /// </summary>
    private void SpawnObstacle(Lane lane, int obstacleIndex, int laneIndex)
    {
        ObstacleData selectedData = lane.obstacles[obstacleIndex];
        if (selectedData == null || selectedData.obstaclePrefab == null) return;

        // Y 위치 계산 (Lane의 Y offset 사용)
        float spawnY = (mainCamera != null ? mainCamera.transform.position.y : 0f) + lane.spawnYOffset;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        // 장애물 생성
        GameObject obstacle = Instantiate(selectedData.obstaclePrefab, spawnPosition, Quaternion.identity);
        obstacle.transform.SetParent(transform);
        activeObstacles.Add(obstacle);

        // Obstacle 컴포넌트에 무게(충격강도=회전각) 설정
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            obstacleComponent.impactRotation = selectedData.weight; // weight = 회전각(도)
        }

        // 무게 기반 이동 속도 계산 (무거울수록 빠름)
        float weightBasedSpeed = baseSpeed + (selectedData.weight * weightSpeedMultiplier);
        float randomSpeed = weightBasedSpeed + Random.Range(-moveSpeedRandomRange, moveSpeedRandomRange);

        // 물결 효과 데이터 생성
        ObstacleFlowData flowData = new ObstacleFlowData
        {
            obstacle = obstacle,
            individualSpeed = randomSpeed,
            wavePhaseX = Random.Range(0f, Mathf.PI * 2f),
            wavePhaseY = Random.Range(0f, Mathf.PI * 2f),
            basePosition = spawnPosition,
            useFlowEffect = enableFlowEffect,
            flowAmplitudeX = flowWaveAmplitudeX,
            flowAmplitudeY = flowWaveAmplitudeY,
            flowSpeed = flowWaveSpeed
        };
        obstacleFlowDataList.Add(flowData);

        if (showDebugLogs)
        {
            float nextInterval = CalculateSpawnInterval(selectedData.weight);
            Debug.Log($"[{lane.laneName}] {selectedData.obstaclePrefab.name} spawned at Y={spawnY:F2}, Weight={selectedData.weight}, Next in {nextInterval:F2}s");
        }
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
            float centerY = mainCamera.transform.position.y;

            // 각 Lane의 스폰 위치 표시
            for (int i = 0; i < lanes.Length; i++)
            {
                Lane lane = lanes[i];
                float spawnY = centerY + lane.spawnYOffset;

                // Lane별 색상
                Gizmos.color = i == 0 ? Color.green : Color.cyan;

                // 스폰 포인트 표시
                Vector3 spawnPos = new Vector3(tempSpawnX, spawnY, 0f);
                Gizmos.DrawWireSphere(spawnPos, 0.3f);

                // Lane 라인 표시
                Gizmos.color = i == 0 ? Color.yellow : Color.magenta;
                Gizmos.DrawLine(
                    new Vector3(tempSpawnX - 0.5f, spawnY, 0f),
                    new Vector3(tempSpawnX + 0.5f, spawnY, 0f)
                );
            }
        }
    }
}
