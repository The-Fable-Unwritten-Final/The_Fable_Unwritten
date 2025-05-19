using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//// 카메라 줌인 액션은 기본적으로
//// 1. 플레이어 > 몬스터
//// 2. 플레이어 > 플레이어 그룹 or 플레이어 > 대상지정 없음
//// 두 가지 경우로 나뉜다는 가정하에 설계했음. 만약 플레이어 + 몬스터 전체 대상의 효과가 추가될 경우 추가적인 구조 변경이 필요함. (if (target[0] is Enemy) 방식으로 target을 직접 체크 중)

public class CombatCameraController : MonoBehaviour
{
    Vector3 mainCamPos; // 카메라 원래 위치 저장

    [SerializeField] CinemachineVirtualCamera mainCam;
    [SerializeField] CinemachineVirtualCamera combatCam;
    [SerializeField] Vector3 combatCamPos = new Vector3 (1.5f,0,-10f); // 전투 카메라 초기 위치(적 위치 줌)
    [SerializeField] float combatTransitionTime = 0.3f; // 전투 관련 효과 전환 텀.

    public List<PlayerController> players;
    public List<Enemy> enemies;
    [SerializeField] Material combatBackgroundMaterial; // 전투 배경 머티리얼

    Coroutine combatCameraCoroutine;


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
        if(combatCameraCoroutine != null) // 이미 카메라 효과가 진행중인 경우
        {
            StopCoroutine(combatCameraCoroutine);
            combatCameraCoroutine = null;
            foreach (var t in target)
            {
                if (t is PlayerController player)
                {
                    player.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    if(caster is Enemy e)
                        e.GetComponent<SpriteRenderer>().sortingOrder = -1;
                }
                else if (t is Enemy enemy)
                {
                    enemy.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    if (caster is PlayerController p)
                        p.GetComponent<SpriteRenderer>().sortingOrder = -1;
                }
            }
        }
        combatCameraCoroutine = StartCoroutine(PlayCombatCameraCoroutine(caster, target, time));
    }
    IEnumerator PlayCombatCameraCoroutine(IStatusReceiver caster, List<IStatusReceiver> target, float time)
    {
        // 추후 확장성(행동시 대상 이동등)을 고려해, caster로 구분.

        if (caster is PlayerController player) // 시전자가 플레이어 진영
        {
            Vector3 playerPos = player.transform.position; // 플레이어 위치 저장
            Vector3 targetPos = new Vector3(-1.75f,-0.25f,0); // 플레이어를 이동시킬 위치

            // 객체 이동 + 카메라 줌인 액션 조정
            if (target[0] is Enemy)// 몬스터 대상 행동
            {
                CameraZoomInAction(time,true);// 카메라 줌인(몬스터 방향)
                player.transform.DOMove(targetPos, combatTransitionTime); // 플레이어 이동
            }
            else
            {
                CameraZoomInAction(time,false);// 카메라 줌인(플레이어 방향)
            }

            //0.3초동안 전투배경 alpha값 페이드인
            combatBackgroundMaterial.DOFade(1f, combatTransitionTime); // 알파 1로

            // sortingOrder 설정
            player.GetComponent<SpriteRenderer>().sortingOrder = 1;
            foreach (var t in target)
            {
                var mono = t as MonoBehaviour;

                if(mono!= null)
                    mono.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }

            yield return new WaitForSeconds(time);

            if (target[0] is Enemy)
                player.transform.DOMove(playerPos, combatTransitionTime); // 플레이어 원래 위치로 이동

            //0.2초동안 전투배경 alpha값 페이드 아웃 + 페이드 아웃에 맞춰 sortingOrder 조정
            combatBackgroundMaterial
                .DOFade(0f, combatTransitionTime) // 알파 0으로
                .onComplete = () =>
                {
                    player.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    foreach (var t in target)
                    {
                        var mono = t as MonoBehaviour;

                        if (mono != null)
                            mono.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    }
                };
        }
        else if (caster is Enemy enemy) // 시전자가 몬스터 진영
        {
            //0.3초동안 전투배경 alpha값 페이드인
            combatBackgroundMaterial.DOFade(1f, combatTransitionTime); // 알파 1로
            enemy.GetComponent<SpriteRenderer>().sortingOrder = 1;
            foreach (var t in target)
            {
                if (t is PlayerController p)
                {
                    p.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
            }
            //caster
            yield return new WaitForSeconds(time);

            //0.2초동안 전투배경 alpha값 페이드아웃
            combatBackgroundMaterial
                .DOFade(0f, combatTransitionTime) // 알파 0으로
                .onComplete = () =>
                {
                    foreach (var t in target)
                    {
                        enemy.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        if (t is PlayerController p)
                        {
                            p.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        }
                    }
                };
        }
        else
        {
            Debug.LogError($"[CombatCameraController] 잘못된 캐스터 타입: {caster.GetType()}");
            yield return null;
        }
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
}
