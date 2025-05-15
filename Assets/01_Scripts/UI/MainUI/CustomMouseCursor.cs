using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CursorState
{
    Idle, // 기본 상태
    Hover, // 버튼 위 
    Click
    //....
}
public class CustomMouseCursor : MonoBehaviour
{
    public RectTransform cursorUI;
    public Image cursorImage; // 커서 이미지

    [SerializeField] Sprite idleTexture; // 커서 텍스쳐
    [SerializeField] Sprite hoverTexture; // 커서 텍스쳐

    CursorState cursorState = CursorState.Idle;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        cursorUI.position = Input.mousePosition;
    }

    public void SetCursorState(CursorState state)
    {
        if (state == cursorState) return;

        cursorState = state;

        switch (state)
        {
            case CursorState.Idle:
                cursorImage.sprite = idleTexture;
                cursorUI.pivot = new Vector2(0.29f, 0.83f);
                break;
            case CursorState.Hover:
                cursorImage.sprite = hoverTexture;
                cursorUI.pivot = new Vector2(0.425f, 0.71f);
                break;
        }
    }

    // 게임 화면을 벗어낫다 돌아와도 커서 제거
    void OnApplicationPause(bool isPaused)
    {
        if (!isPaused)
        {
            Cursor.visible = false;
        }
    }
}
