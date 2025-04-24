using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneEffectPlayer : MonoBehaviour
{
    public static CutsceneEffectPlayer Instance { get; private set; }

    [Header("UI References")]
    public Canvas canvas;
    public Image blackoutImage;
    public Text centerText;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ClearAll();
    }

    public void Play(string type, string content = "")
    {
        switch (type)
        {
            case "animation":
                StartCoroutine(PlayWakeUpAnimation());
                break;
            case "blackout":
                StartCoroutine(BlackoutFadeIn());
                break;
            case "center":
                ShowCenterText(content);
                break;
        }
    }

    public RectTransform topMask;
    public RectTransform bottomMask;
    public float duration = 1.5f;

    public IEnumerator PlayWakeUpAnimation()
    {
        topMask.anchoredPosition = Vector2.zero;
        bottomMask.anchoredPosition = Vector2.zero;

        topMask.gameObject.SetActive(true);
        bottomMask.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            float offset = Mathf.Lerp(0f, 540f, t / duration); // 열리는 높이
            topMask.anchoredPosition = new Vector2(0, offset);
            bottomMask.anchoredPosition = new Vector2(0, -offset);
            t += Time.deltaTime;
            yield return null;
        }

        topMask.gameObject.SetActive(false);
        bottomMask.gameObject.SetActive(false);
    }

    private IEnumerator BlackoutFadeIn()
    {
        blackoutImage.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            blackoutImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }

    private void ShowCenterText(string text)
    {
        centerText.text = text;
        centerText.gameObject.SetActive(true);
        StartCoroutine(HideCenterText());
    }

    private IEnumerator HideCenterText()
    {
        yield return new WaitForSeconds(2f);
        centerText.gameObject.SetActive(false);
    }

    public void ClearAll()
    {
        // 🔹 Blackout
        if (blackoutImage != null)
        {
            blackoutImage.color = new Color(0, 0, 0, 0); // 완전히 투명하게 초기화
            blackoutImage.gameObject.SetActive(false);
        }

        // 🔹 Center Text
        if (centerText != null)
        {
            centerText.text = "";
            centerText.gameObject.SetActive(false);
        }

        // 🔹 Top/Bottom Mask 위치 초기화 및 비활성화
        if (topMask != null)
        {
            topMask.anchoredPosition = Vector2.zero;
            topMask.gameObject.SetActive(false);
        }

        if (bottomMask != null)
        {
            bottomMask.anchoredPosition = Vector2.zero;
            bottomMask.gameObject.SetActive(false);
        }

        Debug.Log("[CutsceneEffectPlayer] All effects cleared.");
    }
}