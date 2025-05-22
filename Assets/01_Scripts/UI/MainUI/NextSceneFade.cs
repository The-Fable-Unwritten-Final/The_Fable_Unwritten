using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextSceneFade : MonoBehaviour
{
    public Image fadeImage; // 검정 이미지
    public float fadeDuration = 1f;

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
        // 페이드 아웃
        yield return StartCoroutine(Fade(0f, 1f));

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

        // 열려있는 팝업 닫기
        while (UIManager.Instance.popupStack.Count > 0)
        {
            var popup = UIManager.Instance.popupStack.Peek();
            popup.Close(); // 열려있는 팝업 닫기.
        } 

        // 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));

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
}
