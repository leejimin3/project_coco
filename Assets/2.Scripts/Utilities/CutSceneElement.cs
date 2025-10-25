using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 개별 컷씬 요소 컴포넌트 (DOTweenPro 스타일)
/// 각 이미지 GameObject에 부착하여 애니메이션 설정
/// </summary>
[RequireComponent(typeof(Image))]
public class CutSceneElement : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private AnimationType animationType = AnimationType.FadeIn;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private Ease easeType = Ease.OutCubic;

    [Header("이동 애니메이션")]
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private float moveDistance = 300f;

    [Header("스케일 애니메이션")]
    [SerializeField] private float scaleFrom = 0.5f;
    [SerializeField] private float scaleTo = 1f;

    [Header("회전 애니메이션")]
    [SerializeField] private float rotationAngle = 360f;

    [Header("딜레이")]
    [SerializeField] private float startDelay = 0f;

    private Image image;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;

    public enum AnimationType
    {
        FadeIn,           // 단순 페이드인
        SlideIn,          // 슬라이드 + 페이드인
        ScaleUp,          // 작게→크게 + 페이드인
        Rotate,           // 회전 + 페이드인
        ZoomOut,          // 크게→작게
        SlideAndScale,    // 슬라이드 + 스케일 조합
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // 원본 값 저장
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalColor = image.color;
    }

    /// <summary>
    /// 애니메이션 시퀀스 생성 및 반환 (등장 애니메이션만, 사라지지 않고 유지)
    /// </summary>
    public Sequence GetAnimationSequence()
    {
        Sequence seq = DOTween.Sequence();

        // 시작 딜레이
        if (startDelay > 0)
        {
            seq.AppendInterval(startDelay);
        }

        // 초기화
        ResetToInitialState();

        // 애니메이션 타입에 따라 분기
        switch (animationType)
        {
            case AnimationType.FadeIn:
                seq.Append(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;

            case AnimationType.SlideIn:
                Vector2 startPos = originalPosition + moveDirection.normalized * moveDistance;
                rectTransform.anchoredPosition = startPos;
                seq.Append(rectTransform.DOAnchorPos(originalPosition, duration).SetEase(easeType));
                seq.Join(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;

            case AnimationType.ScaleUp:
                rectTransform.localScale = originalScale * scaleFrom;
                seq.Append(rectTransform.DOScale(originalScale * scaleTo, duration).SetEase(easeType));
                seq.Join(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;

            case AnimationType.Rotate:
                rectTransform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
                seq.Append(rectTransform.DOLocalRotateQuaternion(originalRotation, duration).SetEase(easeType));
                seq.Join(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;

            case AnimationType.ZoomOut:
                rectTransform.localScale = originalScale * 1.5f;
                seq.Append(rectTransform.DOScale(originalScale, duration).SetEase(easeType));
                seq.Join(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;

            case AnimationType.SlideAndScale:
                Vector2 startPos2 = originalPosition + moveDirection.normalized * moveDistance;
                rectTransform.anchoredPosition = startPos2;
                rectTransform.localScale = originalScale * scaleFrom;
                seq.Append(rectTransform.DOAnchorPos(originalPosition, duration).SetEase(easeType));
                seq.Join(rectTransform.DOScale(originalScale * scaleTo, duration).SetEase(easeType));
                seq.Join(image.DOFade(originalColor.a, duration).SetEase(easeType));
                break;
        }

        // 화면 표시 시간 (이미지는 유지됨)
        seq.AppendInterval(displayTime);

        // 페이드 아웃 제거 - 이미지가 계속 화면에 남아있음

        return seq;
    }

    /// <summary>
    /// 초기 상태로 리셋
    /// </summary>
    private void ResetToInitialState()
    {
        var color = image.color;
        color.a = 0;
        image.color = color;

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
    }

    /// <summary>
    /// 즉시 애니메이션 재생 (테스트용)
    /// </summary>
    [ContextMenu("Play Animation")]
    public void PlayAnimation()
    {
        GetAnimationSequence();
    }

    /// <summary>
    /// 원본 상태로 복원
    /// </summary>
    [ContextMenu("Reset To Original")]
    public void ResetToOriginal()
    {
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
        image.color = originalColor;
    }
}
