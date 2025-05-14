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
}
