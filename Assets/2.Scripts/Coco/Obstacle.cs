using UnityEngine;

/// <summary>
/// 장애물 정보 (충격 강도 등)
/// ObstacleSpawnManager에서 자동으로 설정됨
/// </summary>
public class Obstacle : MonoBehaviour
{
    [HideInInspector]
    public float impactRotation = 10f;

    [Tooltip("충격 방향 (1: 시계방향, -1: 반시계방향)")]
    public float impactDirection = 1f;

    /// <summary>
    /// 충격 회전값 반환
    /// </summary>
    public float GetImpactRotation()
    {
        return impactRotation * impactDirection;
    }
}
