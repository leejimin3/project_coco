using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 컷씬 매니저
/// 하위의 모든 CutSceneElement를 자동으로 찾아서 순차 재생
/// </summary>
public class CutSceneController : MonoBehaviour
{
    [Header("자동 설정")]
    [SerializeField] private bool autoFindElements = true;
    [SerializeField] private List<CutSceneElement> elements = new List<CutSceneElement>();

    [Header("재생 설정")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool loop = false;

    [Header("카메라 설정")]
    [SerializeField] private Camera cutSceneCamera;
    [SerializeField] private bool deactivateOtherCameras = true;

    [Header("완료 후 동작")]
    [SerializeField] private bool autoTransition = true;
    [SerializeField] private GameObject nextCanvas;
    [SerializeField] private Camera nextCamera;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("캔버스 전환 애니메이션")]
    [SerializeField] private bool useTransitionAnimation = true;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Vector2 exitDirection = Vector2.left;  // 현재 캔버스 나가는 방향
    [SerializeField] private Vector2 enterDirection = Vector2.right; // 다음 캔버스 들어오는 방향
    [SerializeField] private float moveDistance = 1920f; // 이동 거리 (화면 너비)

    [Header("스킵 설정")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    [Header("이벤트")]
    public UnityEvent OnCutSceneStart;
    public UnityEvent<int> OnElementStart; // 현재 요소 인덱스
    public UnityEvent OnCutSceneComplete;

    private Sequence mainSequence;
    private bool isPlaying = false;

    private void Start()
    {
        if (autoFindElements)
        {
            FindAllElements();
        }

        // playOnStart가 true일 때만 카메라 설정
        if (playOnStart)
        {
            SetupCutSceneCamera();
            PlayCutScene();
        }
    }

    /// <summary>
    /// 컷씬 시작 시 카메라 설정
    /// </summary>
    private void SetupCutSceneCamera()
    {
        if (cutSceneCamera == null) return;

        // 다른 모든 카메라 비활성화
        if (deactivateOtherCameras)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (var cam in allCameras)
            {
                if (cam != cutSceneCamera)
                {
                    cam.gameObject.SetActive(false);
                }
            }
        }

        // 컷씬 카메라 활성화
        cutSceneCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (allowSkip && isPlaying && Input.GetKeyDown(skipKey))
        {
            SkipCutScene();
        }
    }

    /// <summary>
    /// 하위의 모든 CutSceneElement 자동 탐색
    /// </summary>
    [ContextMenu("Find All Elements")]
    public void FindAllElements()
    {
        elements.Clear();
        elements.AddRange(GetComponentsInChildren<CutSceneElement>(true));
        Debug.Log($"Found {elements.Count} CutSceneElements");
    }

    /// <summary>
    /// 컷씬 재생
    /// </summary>
    public void PlayCutScene()
    {
        if (isPlaying) return;
        if (elements.Count == 0)
        {
            Debug.LogWarning("No CutSceneElements found!");
            return;
        }

        // 컷씬 시작 시 카메라 설정
        SetupCutSceneCamera();

        isPlaying = true;
        OnCutSceneStart?.Invoke();

        mainSequence = DOTween.Sequence();

        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            if (element == null) continue;

            int index = i;
            mainSequence.AppendCallback(() => OnElementStart?.Invoke(index));

            Sequence elementSeq = element.GetAnimationSequence();
            if (elementSeq != null)
            {
                mainSequence.Append(elementSeq);
            }
        }

        mainSequence.OnComplete(OnSequenceComplete);

        if (loop)
        {
            mainSequence.SetLoops(-1);
        }
    }

    /// <summary>
    /// 컷씬 스킵
    /// </summary>
    public void SkipCutScene()
    {
        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Complete(true);
        }
    }

    /// <summary>
    /// 일시정지
    /// </summary>
    public void PauseCutScene()
    {
        mainSequence?.Pause();
    }

    /// <summary>
    /// 재개
    /// </summary>
    public void ResumeCutScene()
    {
        mainSequence?.Play();
    }

    /// <summary>
    /// 시퀀스 완료 콜백
    /// </summary>
    private void OnSequenceComplete()
    {
        isPlaying = false;
        OnCutSceneComplete?.Invoke();

        if (autoTransition && !loop)
        {
            DOVirtual.DelayedCall(transitionDelay, TransitionToNext);
        }
    }

    /// <summary>
    /// 다음 화면으로 전환
    /// </summary>
    private void TransitionToNext()
    {
        // 카메라 전환
        if (nextCamera != null)
        {
            Camera currentCamera = Camera.main;
            if (currentCamera != null)
            {
                currentCamera.gameObject.SetActive(false);
            }
            nextCamera.gameObject.SetActive(true);
        }

        // 캔버스 전환
        if (nextCanvas != null)
        {
            if (useTransitionAnimation)
            {
                TransitionWithAnimation();
            }
            else
            {
                TransitionWithFade();
            }
        }
        // 씬 전환
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// 이동 애니메이션으로 캔버스 전환
    /// </summary>
    private void TransitionWithAnimation()
    {
        RectTransform currentRect = GetComponent<RectTransform>();
        RectTransform nextRect = nextCanvas.GetComponent<RectTransform>();

        if (currentRect == null || nextRect == null)
        {
            // RectTransform이 없으면 기본 전환
            TransitionWithFade();
            return;
        }

        // 다음 캔버스 활성화 및 시작 위치 설정
        nextCanvas.SetActive(true);
        Vector2 nextStartPos = nextRect.anchoredPosition + enterDirection.normalized * moveDistance;
        nextRect.anchoredPosition = nextStartPos;

        // 현재 캔버스 나가는 애니메이션
        Vector2 currentTargetPos = currentRect.anchoredPosition + exitDirection.normalized * moveDistance;
        currentRect.DOAnchorPos(currentTargetPos, transitionDuration).SetEase(Ease.InOutQuad);

        // 다음 캔버스 들어오는 애니메이션
        Vector2 nextTargetPos = nextRect.anchoredPosition - enterDirection.normalized * moveDistance;
        nextRect.DOAnchorPos(nextTargetPos, transitionDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// 페이드 효과로 캔버스 전환 (기존 방식)
    /// </summary>
    private void TransitionWithFade()
    {
        CanvasGroup currentCG = GetComponent<CanvasGroup>();
        if (currentCG != null)
        {
            currentCG.DOFade(0f, transitionDuration).OnComplete(() =>
            {
                gameObject.SetActive(false);
                nextCanvas.SetActive(true);

                // 다음 캔버스 페이드인
                CanvasGroup nextCG = nextCanvas.GetComponent<CanvasGroup>();
                if (nextCG != null)
                {
                    nextCG.alpha = 0;
                    nextCG.DOFade(1f, transitionDuration);
                }
            });
        }
        else
        {
            gameObject.SetActive(false);
            nextCanvas.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        mainSequence?.Kill();
    }
}
