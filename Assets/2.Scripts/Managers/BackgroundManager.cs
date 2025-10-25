using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Depth 기반 배경 무한 스크롤 시스템
/// Depth(Sorting Order)에 따라 스크롤 속도 차등 적용 (패럴랙스 효과)
/// Time.deltaTime 기반으로 프레임과 무관한 일정한 속도 유지
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundLayer
    {
        [Tooltip("배경 GameObject")]
        public GameObject backgroundObject;

        [Tooltip("Depth (Sorting Order) - 높을수록 앞쪽")]
        public int depth = 0;

        [Tooltip("스크롤 속도 (depth가 높을수록 빠르게 설정 권장)")]
        public float scrollSpeed = 5f;

        [HideInInspector] public GameObject clone;
        [HideInInspector] public float spriteWidth;
        [HideInInspector] public float startX;
    }

    [Header("Background Layers")]
    [Tooltip("배경 레이어들 (depth 순서 무관, depth 값으로 자동 정렬)")]
    public BackgroundLayer[] layers;

    [Header("Scroll Settings")]
    [Tooltip("스크롤 방향 (왼쪽: -1, 오른쪽: 1)")]
    public float scrollDirection = -1f;

    [Tooltip("전역 속도 배율 (모든 레이어에 적용)")]
    public float globalSpeedMultiplier = 1f;

    private void Start()
    {
        InitializeBackgrounds();
    }

    private void Update()
    {
        ScrollBackgrounds();
        CheckLoopReset();
    }

    /// <summary>
    /// 배경 초기화 및 복제본 생성
    /// </summary>
    private void InitializeBackgrounds()
    {
        foreach (var layer in layers)
        {
            if (layer.backgroundObject != null)
            {
                SpriteRenderer sr = layer.backgroundObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Depth(Sorting Order) 설정
                    sr.sortingOrder = layer.depth;

                    // 스프라이트 너비 계산
                    layer.spriteWidth = sr.bounds.size.x;
                    layer.startX = layer.backgroundObject.transform.position.x;

                    // 복제본 생성 (무한 스크롤용)
                    layer.clone = Instantiate(layer.backgroundObject, layer.backgroundObject.transform.parent);
                    layer.clone.name = layer.backgroundObject.name + "_Clone";

                    // 복제본 Sorting Order 동일하게 설정
                    SpriteRenderer cloneSr = layer.clone.GetComponent<SpriteRenderer>();
                    if (cloneSr != null)
                    {
                        cloneSr.sortingOrder = layer.depth;
                    }

                    // 복제본을 원본 옆에 배치
                    Vector3 clonePos = layer.backgroundObject.transform.position;
                    if (scrollDirection < 0)
                    {
                        // 왼쪽 스크롤: 복제본을 오른쪽에
                        clonePos.x += layer.spriteWidth;
                    }
                    else
                    {
                        // 오른쪽 스크롤: 복제본을 왼쪽에
                        clonePos.x -= layer.spriteWidth;
                    }
                    layer.clone.transform.position = clonePos;

                    Debug.Log($"Background Layer: {layer.backgroundObject.name}, Depth: {layer.depth}, Speed: {layer.scrollSpeed}, Width: {layer.spriteWidth:F2}");
                }
            }
        }
    }

    /// <summary>
    /// 배경 스크롤 (Time.deltaTime 기반)
    /// </summary>
    private void ScrollBackgrounds()
    {
        foreach (var layer in layers)
        {
            if (layer.backgroundObject != null)
            {
                // 원본 스크롤
                Vector3 pos = layer.backgroundObject.transform.position;
                pos.x += scrollDirection * layer.scrollSpeed * globalSpeedMultiplier * Time.deltaTime;
                layer.backgroundObject.transform.position = pos;

                // 복제본 스크롤
                if (layer.clone != null)
                {
                    Vector3 clonePos = layer.clone.transform.position;
                    clonePos.x += scrollDirection * layer.scrollSpeed * globalSpeedMultiplier * Time.deltaTime;
                    layer.clone.transform.position = clonePos;
                }
            }
        }
    }

    /// <summary>
    /// 무한 반복 체크 및 리셋
    /// </summary>
    private void CheckLoopReset()
    {
        foreach (var layer in layers)
        {
            if (layer.backgroundObject != null && layer.clone != null)
            {
                Vector3 pos = layer.backgroundObject.transform.position;
                Vector3 clonePos = layer.clone.transform.position;

                if (scrollDirection < 0)
                {
                    // 왼쪽 스크롤: 원본이 화면 밖으로 나가면 복제본 오른쪽으로 이동
                    if (pos.x <= layer.startX - layer.spriteWidth)
                    {
                        pos.x = clonePos.x + layer.spriteWidth;
                        layer.backgroundObject.transform.position = pos;
                    }
                    // 복제본이 화면 밖으로 나가면 원본 오른쪽으로 이동
                    if (clonePos.x <= layer.startX - layer.spriteWidth)
                    {
                        clonePos.x = pos.x + layer.spriteWidth;
                        layer.clone.transform.position = clonePos;
                    }
                }
                else if (scrollDirection > 0)
                {
                    // 오른쪽 스크롤: 원본이 화면 밖으로 나가면 복제본 왼쪽으로 이동
                    if (pos.x >= layer.startX + layer.spriteWidth)
                    {
                        pos.x = clonePos.x - layer.spriteWidth;
                        layer.backgroundObject.transform.position = pos;
                    }
                    // 복제본이 화면 밖으로 나가면 원본 왼쪽으로 이동
                    if (clonePos.x >= layer.startX + layer.spriteWidth)
                    {
                        clonePos.x = pos.x - layer.spriteWidth;
                        layer.clone.transform.position = clonePos;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 스크롤 속도 변경 (외부에서 호출 가능)
    /// </summary>
    public void SetGlobalSpeed(float speed)
    {
        globalSpeedMultiplier = speed;
    }

    /// <summary>
    /// 스크롤 일시정지
    /// </summary>
    public void PauseScroll()
    {
        globalSpeedMultiplier = 0f;
    }

    /// <summary>
    /// 스크롤 재개
    /// </summary>
    public void ResumeScroll()
    {
        globalSpeedMultiplier = 1f;
    }

    private void OnDestroy()
    {
        // 복제본 정리
        foreach (var layer in layers)
        {
            if (layer.clone != null)
            {
                Destroy(layer.clone);
            }
        }
    }
}
