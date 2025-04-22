using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaseCampPanel : MonoBehaviour
{
    [Header("FadeEffect")]
    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration = 2f;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    protected IEnumerator FadeExit()
    {
        fadeImage.gameObject.SetActive(true);
        SetFadeAlpha(0f);

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(SceneNameData.StageScene);
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            SetFadeAlpha(alpha);
            time += Time.deltaTime;
            yield return null;
        }

        SetFadeAlpha(1f); // Fade Out 완료
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
