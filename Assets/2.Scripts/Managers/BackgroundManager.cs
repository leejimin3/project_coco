using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Wave 기반 배경 관리 시스템
/// Wave들이 끊김 없이 이어져서 스크롤
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundElement
    {
        [Tooltip("배경 GameObject (Hierarchy의 Backgrounds 그룹 내)")]
        public GameObject backgroundObject;

        [Tooltip("스크롤 속도")]
        public float scrollSpeed = 10f;

        [HideInInspector] public float spriteWidth;
    }

    [System.Serializable]
    public class Wave
    {
        [Tooltip("이 Wave의 배경 요소들")]
        public BackgroundElement[] elements;
    }

    [Header("Wave Settings")]
    [Tooltip("Wave 목록 (순서대로 진행)")]
    public Wave[] waves;

    [Header("Scroll Settings")]
    [Tooltip("스크롤 방향 (왼쪽: -1, 오른쪽: 1)")]
    public float scrollDirection = -1f;

    private List<GameObject> activeBackgrounds = new List<GameObject>();
    private int currentWaveIndex = 0;
    private int nextWaveIndex = 1;

    private void Start()
    {
        if (waves.Length == 0) return;

        // 모든 Wave의 요소들 비활성화
        foreach (var wave in waves)
        {
            foreach (var element in wave.elements)
            {
                if (element.backgroundObject != null)
                {
                    element.backgroundObject.SetActive(false);
                }
            }
        }

        // 첫 번째와 두 번째 Wave 배치
        PositionWave(0, 0); // Wave 1을 화면에
        if (waves.Length > 1)
        {
            PositionWave(1, GetWaveWidth(0)); // Wave 2를 Wave 1 오른쪽에
        }
    }

    private void Update()
    {
        if (waves.Length == 0) return;

        // 모든 활성 배경 스크롤
        foreach (var bg in activeBackgrounds)
        {
            if (bg != null)
            {
                Vector3 pos = bg.transform.position;

                // 해당 배경의 스크롤 속도 찾기
                float speed = 10f;
                foreach (var wave in waves)
                {
                    foreach (var element in wave.elements)
                    {
                        if (element.backgroundObject == bg)
                        {
                            speed = element.scrollSpeed;
                            break;
                        }
                    }
                }

                pos.x += scrollDirection * speed * Time.deltaTime;
                bg.transform.position = pos;
            }
        }

        // 첫 번째 Wave가 화면 밖으로 나갔는지 체크
        CheckAndRecycleWave();
    }

    /// <summary>
    /// 특정 Wave를 특정 X 위치에 배치
    /// </summary>
    private void PositionWave(int waveIndex, float startX)
    {
        if (waveIndex >= waves.Length) return;

        Wave wave = waves[waveIndex];
        int sortingOrder = 0;

        foreach (var element in wave.elements)
        {
            if (element.backgroundObject != null)
            {
                element.backgroundObject.SetActive(true);

                SpriteRenderer sr = element.backgroundObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = sortingOrder;
                    sortingOrder++;

                    element.spriteWidth = sr.bounds.size.x;
                }

                // 위치 설정
                Vector3 pos = element.backgroundObject.transform.position;
                pos.x = startX;
                element.backgroundObject.transform.position = pos;

                activeBackgrounds.Add(element.backgroundObject);

                Debug.Log($"Wave {waveIndex + 1} - {element.backgroundObject.name} positioned at X={startX:F2}");
            }
        }
    }

    /// <summary>
    /// Wave의 너비 계산
    /// </summary>
    private float GetWaveWidth(int waveIndex)
    {
        if (waveIndex >= waves.Length || waves[waveIndex].elements.Length == 0)
            return 0;

        Wave wave = waves[waveIndex];
        if (wave.elements[0].backgroundObject != null)
        {
            SpriteRenderer sr = wave.elements[0].backgroundObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                return sr.bounds.size.x;
            }
        }
        return 0;
    }

    /// <summary>
    /// Wave가 화면 밖으로 나가면 재활용
    /// </summary>
    private void CheckAndRecycleWave()
    {
        if (currentWaveIndex >= waves.Length) return;

        Wave currentWave = waves[currentWaveIndex];
        if (currentWave.elements.Length == 0) return;

        var firstElement = currentWave.elements[0];
        if (firstElement.backgroundObject != null && firstElement.backgroundObject.activeSelf)
        {
            float currentX = firstElement.backgroundObject.transform.position.x;
            float spriteWidth = firstElement.spriteWidth;

            // 배경이 완전히 화면 밖으로 나가면
            if (scrollDirection < 0 && currentX <= -spriteWidth)
            {
                // 현재 Wave 비활성화 및 제거
                DeactivateWave(currentWaveIndex);

                // 다음 Wave 인덱스 업데이트
                currentWaveIndex++;
                if (currentWaveIndex >= waves.Length)
                {
                    currentWaveIndex = 0; // 순환
                }

                nextWaveIndex = currentWaveIndex + 1;
                if (nextWaveIndex >= waves.Length)
                {
                    nextWaveIndex = 0; // 순환
                }

                // 새로운 다음 Wave를 끝에 추가
                float newWaveX = GetRightmostX() + GetWaveWidth(currentWaveIndex);
                PositionWave(nextWaveIndex, newWaveX);

                Debug.Log($"Wave {currentWaveIndex} removed, Wave {nextWaveIndex + 1} added");
            }
        }
    }

    /// <summary>
    /// 현재 가장 오른쪽 배경의 X 위치 구하기
    /// </summary>
    private float GetRightmostX()
    {
        float rightmost = 0;
        foreach (var bg in activeBackgrounds)
        {
            if (bg != null && bg.activeSelf)
            {
                if (bg.transform.position.x > rightmost)
                {
                    rightmost = bg.transform.position.x;
                }
            }
        }
        return rightmost;
    }

    /// <summary>
    /// Wave 비활성화
    /// </summary>
    private void DeactivateWave(int waveIndex)
    {
        if (waveIndex >= waves.Length) return;

        Wave wave = waves[waveIndex];
        foreach (var element in wave.elements)
        {
            if (element.backgroundObject != null)
            {
                element.backgroundObject.SetActive(false);
                activeBackgrounds.Remove(element.backgroundObject);
            }
        }
    }

    private void OnDestroy()
    {
        // 모든 Wave 정리
        for (int i = 0; i < waves.Length; i++)
        {
            DeactivateWave(i);
        }
        activeBackgrounds.Clear();
    }
}
