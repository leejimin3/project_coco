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

        // 목표 회전으로 부드럽게 회전
        float currentRotation = transform.eulerAngles.z;
        // Z축 회전을 -180 ~ 180 범위로 정규화
        if (currentRotation > 180f) currentRotation -= 360f;

        float newRotation = Mathf.LerpAngle(
            currentRotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
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
