using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatCameraController : MonoBehaviour
{
    Vector3 mainCamPos; // 카메라 원래 위치 저장

    [SerializeField] CinemachineVirtualCamera mainCam;
    [SerializeField] CinemachineVirtualCamera combatCam;

    public List<PlayerController> players;
    public List<Enemy> enemies;
    [SerializeField] Material combatBackgroundMaterial; // 전투 배경 머티리얼

    Coroutine combatCameraCoroutine;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C)) // 테스트용 카메라 흔들림
        {
            CameraPunch();
        }
        if(Input.GetKeyDown(KeyCode.V)) // 테스트용 카메라 액션
        {
            List<IStatusReceiver> enemies = new List<IStatusReceiver>();
            enemies.Add(this.enemies[0]);
            enemies.Add(this.enemies[1]);

            PlayCombatCamera(players[0], enemies, 1.5f);
        }
    }

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
        Debug.Log($"{target.Count} 개");
        combatCameraCoroutine = StartCoroutine(PlayCombatCameraCoroutine(caster, target, time));
    }
    IEnumerator PlayCombatCameraCoroutine(IStatusReceiver caster, List<IStatusReceiver> target, float time)
    {
        // 추후 확장성(행동시 대상 이동등)을 고려해, caster를 구분.

        if (caster is PlayerController player) // 시전자가 플레이어 진영
        {
            Vector3 playerPos = player.transform.position; // 플레이어 위치 저장
            Vector3 targetPos = new Vector3(-1.75f,-0.25f,0); // 플레이어를 이동시킬 위치

            CameraZoomInAction(time);// 카메라 줌인
            //0.3초동안 전투배경 alpha값 페이드인
            combatBackgroundMaterial
                .DOFade(1f, 0.3f) // 알파 1로
                .OnStart(() =>
                {
                    player.transform.DOMove(targetPos,0.3f); // 플레이어 이동
                });
            player.GetComponent<SpriteRenderer>().sortingOrder = 1;
            foreach (var t in target)
            {
                Debug.Log($"타겟: {t}");
                if (t is Enemy enemy)
                {
                    Debug.Log("레이어 설정");
                    enemy.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
            }
            yield return new WaitForSeconds(time);

            //0.2초동안 전투배경 alpha값 페이드아웃
            combatBackgroundMaterial
                .DOFade(0f, 0.3f) // 알파 0으로
                .OnStart(() =>
                {
                    player.transform.DOMove(playerPos, 0.3f); // 플레이어 위치 복구
                })
                .onComplete = () =>
                {
                    player.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    foreach (var t in target)
                    {
                        if (t is Enemy enemy)
                        {
                            enemy.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        }
                    }
                };
        }
        else if (caster is Enemy enemy) // 시전자가 몬스터 진영
        {
            //0.3초동안 전투배경 alpha값 페이드인
            combatBackgroundMaterial.DOFade(1f, 0.3f); // 알파 1로
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
                .DOFade(0f, 0.3f) // 알파 0으로
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

    public void CameraPunch()
    {
        Vector3 mainCamPos = mainCam.transform.localPosition; // 카메라 원래 위치 저장

        // 카메라 흔들림 효과
        mainCam.transform.DOShakePosition(0.5f, 0.15f, 8, 40, false, true)
            .OnComplete(() =>
            {
                mainCam.transform.DOKill();
                mainCam.transform.localPosition = mainCamPos;
            });
    }

    private void CameraZoomInAction(float time)
    {
        // 이후 플레이어의 동작에 맞춰 줌인/아웃 분리 호출.
        // 지금은 코루틴으로 임시 구현.
        StartCoroutine(ActionStart(time));
    }
    IEnumerator ActionStart(float time)
    {
        mainCam.enabled = false;
        yield return new WaitForSeconds(time);
        mainCam.enabled = true;
    }
}
