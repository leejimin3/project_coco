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

    [Header("Fade Out Settings")]
    [Tooltip("페이드 아웃 시작 각도 (절대값)")]
    public float fadeOutStartAngle = 90f;

    [Tooltip("페이드 아웃 속도")]
    public float fadeOutSpeed = 1f;

    [Header("Fail Settings")]
    [Tooltip("Fail 판정 각도 (절대값)")]
    public float failAngle = 90f;

    [Tooltip("Fail 판정 후 대기 시간")]
    public float failDelay = 0.2f;

    private Vector3 targetPosition;
    private float targetRotation;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private bool isFadingOut = false;
    private float currentAlpha = 1f;
    private bool hasTriggeredFail = false;

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

        float newRot;

        // 90도 이상이면 복원력 적용 안함
        if (Mathf.Abs(currentRot) >= failAngle)
        {
            newRot = Mathf.LerpAngle(
                currentRot,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            // 90도 미만일 때만 0도로의 복원력을 혼합
            float adjustedTarget = Mathf.LerpAngle(targetRotation, 0f, restorationForce);
            newRot = Mathf.LerpAngle(
                currentRot,
                adjustedTarget,
                Time.deltaTime * rotationSpeed
            );
        }

        transform.rotation = Quaternion.Euler(0, 0, newRot);

        // Coco 애니메이션 상태 업데이트
        UpdateCocoAnimation(newRot);

        // 페이드 아웃 체크 및 처리
        UpdateFadeOut(newRot);

        // Fail 체크
        CheckFail(newRot);
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
    /// 회전각에 따라 페이드 아웃 처리
    /// </summary>
    private void UpdateFadeOut(float rotation)
    {
        if (spriteRenderer == null) return;

        float absRotation = Mathf.Abs(rotation);

        // 90도 이상이면 페이드 아웃 시작
        if (absRotation >= fadeOutStartAngle)
        {
            if (!isFadingOut)
            {
                isFadingOut = true;
                Debug.Log($"Fade out started at rotation: {absRotation:F2}");
            }

            // 알파값 감소
            currentAlpha -= fadeOutSpeed * Time.deltaTime;
            currentAlpha = Mathf.Clamp01(currentAlpha);
        }
        else
        {
            // 90도 미만이면 페이드 인 (복원)
            if (isFadingOut)
            {
                isFadingOut = false;
                Debug.Log($"Fade in started at rotation: {absRotation:F2}");
            }

            // 알파값 증가
            currentAlpha += fadeOutSpeed * Time.deltaTime;
            currentAlpha = Mathf.Clamp01(currentAlpha);
        }

        // SpriteRenderer에 알파 적용
        Color color = spriteRenderer.color;
        color.a = currentAlpha;
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Fail 조건 체크 (90도 이상)
    /// </summary>
    private void CheckFail(float rotation)
    {
        if (hasTriggeredFail) return;

        float absRotation = Mathf.Abs(rotation);

        if (absRotation >= failAngle)
        {
            hasTriggeredFail = true;
            Debug.Log($"[Boat] Fail triggered! Rotation: {absRotation:F2} degrees");
            StartCoroutine(TriggerFailAfterDelay());
        }
    }

    /// <summary>
    /// Fail 처리 (0.2초 대기 후 GameManager에 알림)
    /// </summary>
    private System.Collections.IEnumerator TriggerFailAfterDelay()
    {
        Debug.Log($"[Boat] Waiting {failDelay} seconds before fail...");
        yield return new WaitForSeconds(failDelay);

        Debug.Log("[Boat] Fail delay complete, notifying GameManager");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameFail();
        }
        else
        {
            Debug.LogError("[Boat] GameManager instance not found!");
        }
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
            HandleCollision(collision);
        }
    }

    /// <summary>
    /// 충돌 시 이펙트 생성 및 Obstacle 위치 이동
    /// </summary>

    private void HandleCollision(Collider2D collision)
    {
        GameObject obstacle = collision.gameObject;
        Debug.Log("Boat collided with obstacle!");

        // 충돌 지점 계산
        Vector3 hitPoint = collision.ClosestPoint(transform.position); // Boat 중심 기준으로 가장 가까운 점

        // 충돌 이펙트 생성
        if (collisionEffectPrefab != null)
        {
            Instantiate(collisionEffectPrefab, hitPoint, Quaternion.identity);
        }

        // Obstacle 충격 처리
        Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
        if (obstacleComponent != null)
        {
            float currentRot = transform.eulerAngles.z;
            if (currentRot > 180f) currentRot -= 360f;

            float impactRotation = obstacleComponent.GetImpactRotation();
            targetRotation += impactRotation;

            timer = 0f;

            Debug.Log($"Impact applied: {impactRotation} degrees. Current target rotation: {targetRotation}");
        }

        // Boat 너비 기준으로 Obstacle 이동
        float boatWidth = 0f;
        if (spriteRenderer != null)
        {
            boatWidth = spriteRenderer.bounds.size.x;
        }

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
