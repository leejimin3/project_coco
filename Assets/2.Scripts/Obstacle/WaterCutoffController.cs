using UnityEngine;

public class WaterCutoffController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private float worldCutoffY = 0f;
    private Material _materialInstance;
    public float waterYOffset = 0f;

    public bool bRotate = false;
    public float rotateValue = 0f;
    public Transform body;
    void Start()
    {
        body = transform.Find("Body")?.transform;
        SpriteRenderer spriteRenderer = body.GetComponent<SpriteRenderer>();
        if (render == null)
        {
            // Renderer에서 현재 Material 복사본 생성
            var renderer = GetComponent<SpriteRenderer>();   
        }
        _materialInstance = Instantiate(spriteRenderer.sharedMaterial);
        spriteRenderer.material = _materialInstance; // 독립된 머티리얼로 교체
        worldCutoffY = transform.position.y + waterYOffset;
    }

    void Update()
    {
        // 런타임에서 실시간 조정 가능
        _materialInstance.SetFloat("_CutoffHeight", worldCutoffY);

        if (bRotate)
        {
            body.Rotate(0f, 0f, rotateValue * Time.deltaTime);
        }
    }
}