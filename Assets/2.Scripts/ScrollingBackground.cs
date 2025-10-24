using UnityEngine;

/// <summary>
/// World Space 배경을 무한 스크롤시키는 스크립트
/// 배경을 자동으로 복제하여 무한 반복
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundLayer
    {
        [Tooltip("배경 오브젝트 (SpriteRenderer 필요)")]
        public GameObject background;

        [Tooltip("스크롤 속도 (가까운 배경일수록 빠르게)")]
        public float scrollSpeed = 2f;

        [HideInInspector] public GameObject clone;
        [HideInInspector] public float spriteWidth;
    }

    [Header("Background Layers")]
    [Tooltip("배경 레이어들 (각 레이어마다 속도 다르게 설정)")]
    public BackgroundLayer[] backgroundLayers;

    [Header("Scroll Settings")]
    [Tooltip("스크롤 방향 (왼쪽: -1, 오른쪽: 1)")]
    public float scrollDirection = -1f;

    private void Start()
    {
        // 각 레이어마다 복제본 생성
        foreach (var layer in backgroundLayers)
        {
            if (layer.background != null)
            {
                SpriteRenderer sr = layer.background.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    layer.spriteWidth = sr.bounds.size.x;

                    // 복제본 생성
                    layer.clone = Instantiate(layer.background);
                    layer.clone.name = layer.background.name + "_Clone";

                    // 복제본을 원본 옆에 배치
                    Vector3 clonePos = layer.background.transform.position;
                    if (scrollDirection < 0)
                        clonePos.x += layer.spriteWidth; // 왼쪽 스크롤: 오른쪽에 배치
                    else
                        clonePos.x -= layer.spriteWidth; // 오른쪽 스크롤: 왼쪽에 배치

                    layer.clone.transform.position = clonePos;
                }
            }
        }
    }

    private void Update()
    {
        foreach (var layer in backgroundLayers)
        {
            if (layer.background != null && layer.clone != null)
            {
                // 원본 스크롤
                Vector3 pos = layer.background.transform.position;
                pos.x += scrollDirection * layer.scrollSpeed * Time.deltaTime;
                layer.background.transform.position = pos;

                // 복제본도 같이 스크롤
                Vector3 clonePos = layer.clone.transform.position;
                clonePos.x += scrollDirection * layer.scrollSpeed * Time.deltaTime;
                layer.clone.transform.position = clonePos;

                // 무한 루프: 원본이 화면 밖으로 나가면 복제본 뒤로 이동
                if (scrollDirection < 0)
                {
                    if (pos.x <= -layer.spriteWidth)
                    {
                        pos.x = clonePos.x + layer.spriteWidth;
                        layer.background.transform.position = pos;
                    }
                    if (clonePos.x <= -layer.spriteWidth)
                    {
                        clonePos.x = pos.x + layer.spriteWidth;
                        layer.clone.transform.position = clonePos;
                    }
                }
                else if (scrollDirection > 0)
                {
                    if (pos.x >= layer.spriteWidth)
                    {
                        pos.x = clonePos.x - layer.spriteWidth;
                        layer.background.transform.position = pos;
                    }
                    if (clonePos.x >= layer.spriteWidth)
                    {
                        clonePos.x = pos.x - layer.spriteWidth;
                        layer.clone.transform.position = clonePos;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 복제본 정리
        foreach (var layer in backgroundLayers)
        {
            if (layer.clone != null)
            {
                Destroy(layer.clone);
            }
        }
    }
}
