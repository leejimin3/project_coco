using UnityEngine;

/// <summary>
/// 보트 트리거 시스템
/// 장애물이나 아이템과 충돌 감지
/// </summary>
public class BoatTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("충돌 레이어 (어떤 레이어와 충돌할지)")]
    public LayerMask collisionLayers;

    [Header("Debug")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLogs = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 자기 자신(Boat)과의 충돌 무시
        if (other.CompareTag("Boat"))
        {
            return;
        }

        // 부모가 같은 오브젝트 무시 (Boat의 자식들끼리 충돌 무시)
        if (transform.parent != null && other.transform.IsChildOf(transform.parent))
        {
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"[BoatTrigger] 충돌 시작: {other.gameObject.name} (Tag: {other.tag})");
        }

        // 태그별 처리
        HandleCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 트리거 영역 내에 머물러 있을 때 (필요 시 사용)
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 자기 자신(Boat)과의 충돌 무시
        if (other.CompareTag("Boat"))
        {
            return;
        }

        // 부모가 같은 오브젝트 무시 (Boat의 자식들끼리 충돌 무시)
        if (transform.parent != null && other.transform.IsChildOf(transform.parent))
        {
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"[BoatTrigger] 충돌 종료: {other.gameObject.name} (Tag: {other.tag})");
        }
    }

    /// <summary>
    /// 충돌 처리
    /// </summary>
    private void HandleCollision(Collider2D other)
    {
        // 태그에 따라 다른 처리
        switch (other.tag)
        {
            case "Obstacle":
                OnObstacleHit(other.gameObject);
                break;

            case "Item":
                OnItemCollected(other.gameObject);
                break;

            case "Coin":
                OnCoinCollected(other.gameObject);
                break;

            default:
                if (showDebugLogs)
                {
                    Debug.Log($"[BoatTrigger] 알 수 없는 태그: {other.tag}");
                }
                break;
        }
    }

    /// <summary>
    /// 장애물 충돌 처리
    /// </summary>
    private void OnObstacleHit(GameObject obstacle)
    {
        Debug.Log($"[BoatTrigger] 장애물 충돌! {obstacle.name}");

        // TODO: 게임 오버 또는 데미지 처리
        // GameManager.Instance.GameOver();
    }

    /// <summary>
    /// 아이템 수집 처리
    /// </summary>
    private void OnItemCollected(GameObject item)
    {
        Debug.Log($"[BoatTrigger] 아이템 획득! {item.name}");

        // TODO: 아이템 효과 적용
        // 아이템 제거
        Destroy(item);
    }

    /// <summary>
    /// 코인 수집 처리
    /// </summary>
    private void OnCoinCollected(GameObject coin)
    {
        Debug.Log($"[BoatTrigger] 코인 획득! {coin.name}");

        // TODO: 점수 추가
        // GameManager.Instance.AddScore(10);

        // 코인 제거
        Destroy(coin);
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        // Collider2D 시각화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;

            if (col is BoxCollider2D)
            {
                BoxCollider2D boxCol = col as BoxCollider2D;
                Gizmos.DrawWireCube(
                    transform.position + (Vector3)boxCol.offset,
                    boxCol.size
                );
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circleCol = col as CircleCollider2D;
                Gizmos.DrawWireSphere(
                    transform.position + (Vector3)circleCol.offset,
                    circleCol.radius
                );
            }
        }
    }
}
