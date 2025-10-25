using UnityEngine;
using DG.Tweening;

/// <summary>
/// DOTween 간단 사용 헬퍼
/// 자주 사용하는 트윈 애니메이션을 쉽게 사용
/// </summary>
public static class DOTweenHelper
{
    // ========== 이동 ==========

    /// <summary>
    /// 특정 위치로 이동
    /// </summary>
    public static Tween MoveTo(this Transform transform, Vector3 target, float duration)
    {
        return transform.DOMove(target, duration);
    }

    /// <summary>
    /// 로컬 위치로 이동
    /// </summary>
    public static Tween MoveToLocal(this Transform transform, Vector3 target, float duration)
    {
        return transform.DOLocalMove(target, duration);
    }

    /// <summary>
    /// 위아래로 흔들리기 (Bounce)
    /// </summary>
    public static Tween Bounce(this Transform transform, float height, float duration)
    {
        Vector3 start = transform.position;
        return transform.DOMoveY(start.y + height, duration)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ========== 회전 ==========

    /// <summary>
    /// Z축 회전
    /// </summary>
    public static Tween RotateZ(this Transform transform, float angle, float duration)
    {
        return transform.DORotate(new Vector3(0, 0, angle), duration);
    }

    /// <summary>
    /// 무한 회전
    /// </summary>
    public static Tween RotateInfinite(this Transform transform, float speed = 1f)
    {
        return transform.DORotate(new Vector3(0, 0, 360), speed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    // ========== 크기 ==========

    /// <summary>
    /// 크기 변경
    /// </summary>
    public static Tween ScaleTo(this Transform transform, Vector3 scale, float duration)
    {
        return transform.DOScale(scale, duration);
    }

    /// <summary>
    /// 펄스 효과 (크기 변화 반복)
    /// </summary>
    public static Tween Pulse(this Transform transform, float scaleMultiplier, float duration)
    {
        Vector3 originalScale = transform.localScale;
        return transform.DOScale(originalScale * scaleMultiplier, duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// 팝업 등장 (작게→크게)
    /// </summary>
    public static Tween PopIn(this Transform transform, float duration = 0.3f)
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        return transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 팝업 사라지기 (크게→작게)
    /// </summary>
    public static Tween PopOut(this Transform transform, float duration = 0.3f)
    {
        return transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
    }

    // ========== 페이드 ==========

    /// <summary>
    /// SpriteRenderer 페이드 인
    /// </summary>
    public static Tween FadeIn(this SpriteRenderer renderer, float duration)
    {
        return renderer.DOFade(1f, duration);
    }

    /// <summary>
    /// SpriteRenderer 페이드 아웃
    /// </summary>
    public static Tween FadeOut(this SpriteRenderer renderer, float duration)
    {
        return renderer.DOFade(0f, duration);
    }

    /// <summary>
    /// CanvasGroup 페이드 인
    /// </summary>
    public static Tween FadeIn(this CanvasGroup canvasGroup, float duration)
    {
        return canvasGroup.DOFade(1f, duration);
    }

    /// <summary>
    /// CanvasGroup 페이드 아웃
    /// </summary>
    public static Tween FadeOut(this CanvasGroup canvasGroup, float duration)
    {
        return canvasGroup.DOFade(0f, duration);
    }

    // ========== 흔들기 ==========

    /// <summary>
    /// 위치 흔들기 (Shake)
    /// </summary>
    public static Tween Shake(this Transform transform, float duration, float strength = 1f)
    {
        return transform.DOShakePosition(duration, strength);
    }

    /// <summary>
    /// 회전 흔들기
    /// </summary>
    public static Tween ShakeRotation(this Transform transform, float duration, float strength = 90f)
    {
        return transform.DOShakeRotation(duration, strength);
    }

    // ========== 시퀀스 ==========

    /// <summary>
    /// 여러 트윈을 순서대로 실행
    /// </summary>
    public static Sequence CreateSequence(params Tween[] tweens)
    {
        Sequence sequence = DOTween.Sequence();
        foreach (var tween in tweens)
        {
            sequence.Append(tween);
        }
        return sequence;
    }

    // ========== 유틸리티 ==========

    /// <summary>
    /// 모든 트윈 정지
    /// </summary>
    public static void KillAll(this Transform transform)
    {
        transform.DOKill();
    }

    /// <summary>
    /// 대기 (Delay)
    /// </summary>
    public static Tween Wait(float duration, System.Action onComplete)
    {
        return DOVirtual.DelayedCall(duration, () => onComplete?.Invoke());
    }
}
