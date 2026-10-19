using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//// 카메라 줌인 액션은 기본적으로
//// 1. 플레이어 > 몬스터
//// 2. 플레이어 > 플레이어 그룹 or 플레이어 > 대상지정 없음
//// 두 가지 경우로 나뉜다는 가정하에 설계했음. 만약 플레이어 + 몬스터 전체 대상의 효과가 추가될 경우 추가적인 구조 변경이 필요함.

public class CombatCameraController : MonoBehaviour
{
    Vector3 mainCamPos; // 카메라 원래 위치 저장

    [SerializeField] CinemachineVirtualCamera mainCam;
    [SerializeField] CinemachineVirtualCamera combatCam;
    [SerializeField] Vector3 combatCamPos = new Vector3 (1.5f,0,-10f); // 전투 카메라 초기 위치(적 위치 줌)
    [SerializeField] Vector3 playerGoPos = new Vector3(-1.75f,-0.25f,0);
    [SerializeField] float combatTransitionTime = 0.3f; // 전투 관련 효과 전환 텀.

    public List<PlayerController> players;
    public List<Enemy> enemies;
    [SerializeField] Material combatBackgroundMaterial; // 전투 배경 머티리얼

    public Coroutine combatCameraCoroutine; // 특수 모션 카메라 줌인 효과시


    [SerializeField, Range(0f, 1f)] private float dimBrightness = 0.35f;

    private readonly Dictionary<SpriteRenderer, Color> originalColors = new();
    private readonly Dictionary<SpriteRenderer, int> originalSortingOrders = new();

    private void Awake()
    {
        GameManager.Instance.RegisterCombatCamera(this);
        mainCamPos = mainCam.transform.localPosition; // 카메라 원래 위치 저장
        combatBackgroundMaterial.DOFade(0f, 0f); // 알파 0으로 초기화
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnregisterCombatCamera();
    }
    /// <summary>
    /// 전투 행동시, 카메라 효과 실행 함수
    /// </summary>
    /// <param name="caster">행동의 시전자</param>
    /// <param name="target">행동의 피격자</param>
    /// <param name="time">행동을 진행하는 시간</param>
    public void PlayCombatCamera(IStatusReceiver caster, List<IStatusReceiver> target, float time)
    {
        if (combatCameraCoroutine != null)
        {
            StopCoroutine(combatCameraCoroutine);
            combatCameraCoroutine = null;
        }

        colorRestoreTween?.Kill();
        colorRestoreTween = null;

        combatBackgroundMaterial.DOKill();
        RestoreSortingOrders();
        RestoreCharacterColors();

        combatCameraCoroutine = StartCoroutine(PlayCombatCameraCoroutine(caster, target, time));
    }

    IEnumerator PlayCombatCameraCoroutine(IStatusReceiver caster, List<IStatusReceiver> target, float time)
    {
        if (!(caster is PlayerController) && !(caster is Enemy))
        {
            Debug.LogError($"[CombatCameraController] 잘못된 캐스터 타입: {caster?.GetType()}");
            combatCameraCoroutine = null;
            yield break;
        }

        // 시전자와 타겟을 제외한 캐릭터 어둡게 처리
        SetCharacterDim(caster, target);

        // 기존 전투 배경 페이드
        combatBackgroundMaterial.DOFade(1f, combatTransitionTime);

        // 시전자와 타겟을 전면으로 이동
        SetForeground(caster);

        if (target != null)
        {
            foreach (var t in target)
                SetForeground(t);
        }

        yield return new WaitForSeconds(time);

        // 전투 배경 원상복구
        yield return combatBackgroundMaterial
            .DOFade(0f, combatTransitionTime)
            .WaitForCompletion();

        RestoreSortingOrders();
        RestoreCharacterColors();

        Debug.Log("[CombatCamera] Fade Restore Complete");
        combatCameraCoroutine = null;
    }
    // 몬스터의 공격 애니메이션의 싱크에 맞춰서 공격시점에서 >> 데미지 적용 + 카메라 액션을 하기에, 매개변수로 받는 형식이 아니라 체력에 적용을 해주는 시점에서 각각의 메서드(CameraPunch)를 상황에 맞게 호출 형식으로 변경.
    public void CameraPunch()
    {
        Vector3 mainCamPos = mainCam.transform.localPosition; // 카메라 원래 위치 저장

        // 카메라 흔들림 효과
        mainCam.transform.DOShakePosition(0.5f, 0.10f, 8, 40, false, true)
            .OnComplete(() =>
            {
                mainCam.transform.DOKill();
                mainCam.transform.localPosition = mainCamPos;
            });
    }
    public void CameraPunchHard()
    {
        Vector3 mainCamPos = mainCam.transform.localPosition; // 카메라 원래 위치 저장

        // 카메라 흔들림 효과
        mainCam.transform.DOShakePosition(0.5f, 0.20f, 8, 40, false, true)
            .OnComplete(() =>
            {
                mainCam.transform.DOKill();
                mainCam.transform.localPosition = mainCamPos;
            });
    }
    /// <summary>
    /// 카메라 줌
    /// </summary>
    /// <param name="time">줌 연출 시간</param>
    /// <param name="b">True == 몬스터 방향 줌</param>
    private void CameraZoomInAction(float time, bool b)
    {
        // 이후 플레이어의 동작에 맞춰 줌인/아웃 분리 호출.
        // 지금은 코루틴으로 임시 구현.
        StartCoroutine(ActionStart(time,b));
    }
    IEnumerator ActionStart(float time,bool b)
    {
        mainCam.enabled = false;
        if(b)
            combatCam.transform.position = combatCamPos;
        else
            combatCam.transform.position = new Vector3(-combatCamPos.x, 0, combatCamPos.z);

        yield return new WaitForSeconds(time);
        mainCam.enabled = true;
    }

    private void SetCharacterDim(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        foreach (var player in players)
        {
            if (player == null || player.spriteRenderer == null) continue;
            if (ReferenceEquals(player, caster) || (targets != null && targets.Contains(player))) continue;

            DimSprite(player.spriteRenderer);
        }

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.spriteRenderer == null) continue;
            if (ReferenceEquals(enemy, caster) || (targets != null && targets.Contains(enemy))) continue;

            DimSprite(enemy.spriteRenderer);
        }
    }

    private void DimSprite(SpriteRenderer sr)
    {
        if (!originalColors.ContainsKey(sr))
        {
            Color or = sr.color;
            or.a = 1f;
            originalColors.Add(sr, or);
        }
        sr.DOKill();

        Color original = originalColors[sr];
        Color dimColor = new Color(
            original.r * dimBrightness,
            original.g * dimBrightness,
            original.b * dimBrightness,
            original.a
        );

        sr.DOColor(dimColor, combatTransitionTime);
    }
    private Tween colorRestoreTween;

    private void RestoreCharacterColors()
    {
        colorRestoreTween?.Kill();
        colorRestoreTween = null;

        foreach (var pair in originalColors)
        {
            if (pair.Key == null) continue;

            pair.Key.DOKill();
            pair.Key.color = pair.Value;

            Debug.Log($"[CombatCamera] Restore {pair.Key.name}: {pair.Key.color}");
        }

        originalColors.Clear();
    }

    private void SetForeground(IStatusReceiver receiver)
    {
        SpriteRenderer sr = null;

        if (receiver is PlayerController player)
            sr = player.spriteRenderer;
        else if (receiver is Enemy enemy)
            sr = enemy.spriteRenderer;

        if (sr == null) return;

        if (!originalSortingOrders.ContainsKey(sr))
            originalSortingOrders.Add(sr, sr.sortingOrder);

        sr.sortingOrder = 1;
    }

    private void RestoreSortingOrders()
    {
        foreach (var pair in originalSortingOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value;
        }

        originalSortingOrders.Clear();
    }
}
