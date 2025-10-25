using UnityEngine;

/// <summary>
/// 종이배 오브젝트
/// 화면 가운데를 중심으로 일정 반경 내에서 물 위에서 넘실거리듯이 움직임
/// </summary>
public class Boat : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("넘실거림의 중심 위치")]
    public Vector3 centerPosition = Vector3.zero;

    [Tooltip("넘실거리는 반경")]
    public float floatRadius = 0.5f;

    [Tooltip("위치 변경 주기 (초)")]
    public float changeInterval = 2f;

    [Tooltip("이동 속도 (부드럽게 보간)")]
    public float moveSpeed = 1f;

    [Header("Rotation Settings")]
    [Tooltip("최대 회전 각도 (degrees)")]
    public float maxRotationAngle = 15f;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 1f;

    [Tooltip("0도로 복원하는 힘의 강도 (0이면 복원력 없음)")]
    public float restorationForce = 0.5f;

    [Header("Collision Settings")]
    [Tooltip("충돌 시 왼쪽으로 이동할 추가 거리")]
    public float collisionOffsetDistance = 10f;

    [Tooltip("충돌 이펙트 프리팹")]
    public GameObject collisionEffectPrefab;

    [Header("Player Animation")]
    [Tooltip("Coco 플레이어 참조")]
    public Coco cocoPlayer;

    private Vector3 targetPosition;
    private float targetRotation;
    private float timer;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // SpriteRenderer 가져오기 및 Pivot을 Bottom Center로 설정
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Sprite의 Pivot을 Bottom Center로 설정하려면
            // Sprite를 새로 생성하거나 Import Settings에서 설정해야 함
            // 여기서는 로컬 위치 조정으로 처리
            Vector3 offset = new Vector3(0, spriteRenderer.bounds.extents.y, 0);
            transform.position += offset;
        }

        // Coco 플레이어 자동 찾기
        if (cocoPlayer == null)
        {
            cocoPlayer = GetComponentInChildren<Coco>();
            if (cocoPlayer == null)
            {
                cocoPlayer = FindObjectOfType<Coco>();
            }

            if (cocoPlayer == null)
            {
                Debug.LogWarning("Coco 플레이어를 찾을 수 없습니다. Boat Inspector에서 수동으로 할당해주세요.");
            }
        }

        // 시작 위치를 중심으로 설정
        if (centerPosition == Vector3.zero)
        {
            centerPosition = transform.position;
        }

        // 첫 목표 위치 및 회전 설정
        targetPosition = GetRandomPositionInRadius();
        targetRotation = GetRandomRotation();
        timer = 0f;
    }

    private void Update()
    {
        // 타이머 업데이트
        timer += Time.deltaTime;

        // 일정 시간마다 새로운 목표 위치와 회전 설정
        if (timer >= changeInterval)
        {
            targetPosition = GetRandomPositionInRadius();
            targetRotation = GetRandomRotation();
            timer = 0f;
        }

        // 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );

        // 목표 회전으로 부드럽게 회전 + 0도로 복원하는 힘 적용
        float currentRot = transform.eulerAngles.z;
        // Z축 회전을 -180 ~ 180 범위로 정규화
        if (currentRot > 180f) currentRot -= 360f;

        // 목표 회전각에 0도로의 복원력을 혼합
        float adjustedTarget = Mathf.LerpAngle(targetRotation, 0f, restorationForce);

        float newRot = Mathf.LerpAngle(
            currentRot,
            adjustedTarget,
            Time.deltaTime * rotationSpeed
        );
        transform.rotation = Quaternion.Euler(0, 0, newRot);

        // Coco 애니메이션 상태 업데이트
        UpdateCocoAnimation(newRot);
    }

    /// <summary>
    /// 회전각에 따라 Coco 애니메이션 상태 변경
    /// </summary>
    private void UpdateCocoAnimation(float rotation)
    {
        if (cocoPlayer == null) return;

        // 절대값 기준으로 상태 결정
        float absRotation = Mathf.Abs(rotation);

        int animState;
        if (absRotation < 12.5f) // 0~12.5도
        {
            animState = 0; // run0
        }
        else if (absRotation < 37.5f) // 12.5~37.5도 (25도 중심)
        {
            animState = 1; // run1
        }
        else if (absRotation < 62.5f) // 37.5~62.5도 (50도 중심)
        {
            animState = 2; // run2
        }
        else // 62.5도 이상 (75도 중심)
        {
            animState = 3; // run3
        }

        cocoPlayer.SetState(animState);
    }

    /// <summary>
    /// 중심점 기준 반경 내의 랜덤 위치를 반환
    /// </summary>
    private Vector3 GetRandomPositionInRadius()
    {
        Vector2 randomCircle = Random.insideUnitCircle * floatRadius;
        return centerPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);
    }

    /// <summary>
    /// 랜덤 회전 각도를 반환
    /// </summary>
    private float GetRandomRotation()
    {
        return Random.Range(-maxRotationAngle, maxRotationAngle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Obstacle과 충돌 시 (Trigger 방식)
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            HandleCollision(collision.gameObject);
        }
    }

    /// <summary>
    /// 충돌 시 이펙트 생성 및 Obstacle 위치 이동
    /// </summary>
    private void HandleCollision(GameObject obstacle)
    {
        Debug.Log("Boat collided with obstacle!");

        // 충돌 이펙트 생성
        if (collisionEffectPrefab != null)
        {
            Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Obstacle의 충격 정보 가져오기
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            // 현재 회전각에 충격 회전 추가 (제한 없이)
            float currentRot = transform.eulerAngles.z;
            if (currentRot > 180f) currentRot -= 360f;

            float impactRotation = obstacleComponent.GetImpactRotation();
            targetRotation += impactRotation;

            // 타이머 리셋하여 바로 다음 랜덤 회전으로 덮어씌워지지 않도록
            timer = 0f;

            Debug.Log($"Impact applied: {impactRotation} degrees. Current target rotation: {targetRotation}");
        }

        // Boat의 너비 계산
        float boatWidth = 0f;
        if (spriteRenderer != null)
        {
            boatWidth = spriteRenderer.bounds.size.x;
        }

        // Obstacle을 왼쪽으로 이동 (boat width + offset distance)
        Vector3 obstaclePosition = obstacle.transform.position;
        obstaclePosition.x -= (boatWidth + collisionOffsetDistance);
        obstacle.transform.position = obstaclePosition;

        Debug.Log($"Obstacle moved left by {boatWidth + collisionOffsetDistance} units. New position: {obstaclePosition}");
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 넘실거림 반경 표시
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? centerPosition : transform.position;
        Gizmos.DrawWireSphere(center, floatRadius);
        Gizmos.DrawLine(center + Vector3.up * floatRadius, center + Vector3.down * floatRadius);
        Gizmos.DrawLine(center + Vector3.left * floatRadius, center + Vector3.right * floatRadius);
    }
}
