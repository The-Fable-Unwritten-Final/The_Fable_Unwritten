using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextSceneFade : MonoBehaviour
{
    public Image fadeImage; // 검정 이미지
    public float fadeDuration = 1f;

    void Awake()
    {
        fadeImage.material.SetFloat("_Progress", 0f);
        // 이전 색상 (182, 150, 114)
        fadeImage.material.color = new Color(178f / 255f, 114f / 255f, 61f / 255f, 0f);
    }

    /// <summary>
    /// 씬 전환시 사용하는 메서드
    /// </summary>
    /// <param name="nextSceneName">해당 씬 이름 string</param>
    public void StartSceneTransition(string nextSceneName)
    {
        fadeImage.raycastTarget = true;
        StartCoroutine(Transition(nextSceneName));
    }

    IEnumerator Transition(string nextScene)
    {
        IfCombatScene();
        // 페이드 아웃
        yield return StartCoroutine(InkCover(0f, 1f));

        // 씬 비동기 로드 (0.9까지 진행)
        AsyncOperation async = SceneManager.LoadSceneAsync(nextScene);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
            yield return null;

        // 전환
        async.allowSceneActivation = true;

        // 씬 전환 완료까지 대기
        while (!async.isDone)
            yield return null;

        // UIParticle 및 메모리 정리
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // 열려있는 팝업 닫기
        while (UIManager.Instance.popupStack.Count > 0)
        {
            var popup = UIManager.Instance.popupStack.Peek();
            popup.Close(); // 열려있는 팝업 닫기.
        } 

        // 페이드 인
        yield return StartCoroutine(InkCover(1f, 0f));

        fadeImage.raycastTarget = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }

    IEnumerator InkCover(float from, float to, float duration = 1.5f)
    {
        float elapsed = 0f;
        Material mat = fadeImage.material;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            float progress = Mathf.Lerp(from, to, t);

            // Progress와 Alpha를 조건부로 조정
            float alpha;
            if (from < to) // 페이드 아웃: 0 -> 1
            {
                // progress 0~0.5: alpha 0~1, progress 0.5~1: alpha 1
                alpha = progress <= 0.1f ? progress / 0.1f : 1f;
            }
            else // 페이드 인: 1 -> 0
            {
                // progress 1~0.5: alpha 1, progress 0.5~0: alpha 1~0
                alpha = progress >= 0.1f ? 1f : progress / 0.1f;
            }

            mat.SetFloat("_Progress", progress);
            // 이전 색상 (182, 150, 114)
            mat.color = new Color(178f / 255f, 114f / 255f, 61f / 255f, alpha);

            yield return null;
        }
    }

    void IfCombatScene()
    {
        if (GameManager.Instance.combatUIController != null)
        {
            // 전투 씬일 경우 카드를 아래로 내리는 메서드 호출
            GameManager.Instance.combatUIController.MoveCardsWhenExitSceneTransition();

            // 모든 UIParticle 강제 정리
            Coffee.UIExtensions.UIParticle[] allParticles = FindObjectsOfType<Coffee.UIExtensions.UIParticle>();
            foreach (Coffee.UIExtensions.UIParticle particle in allParticles)
            {
                if (particle != null)
                {
                    particle.Clear();
                    particle.Stop();
                }
            }
        }
    }
}
