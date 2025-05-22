using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_UnlockController : BasePopupUI
{
    /// <summary>
    /// 캐릭터 스프라이트 및 출력할 dialogue를 설정
    /// </summary>
    [System.Serializable]
    public class CharacterUnlockUIData
    {
        public CharacterClass character;
        public Sprite characterSprite;
        public string[] dialogue;
    }

    [SerializeField] private UnlockItemPrefab unlockItemPrefab;
    [SerializeField] private GameObject popupPanel;

    [Header("오른쪽 캐릭터 패널")]
    [SerializeField] private Image characterImage;

    [Header("말풍선 오브젝트 및 텍스트")]
    [SerializeField] private GameObject speechBubbleObject;
    [SerializeField] private TextMeshProUGUI speechBubbleText;

    [Header("캐릭터 데이터")]
    [SerializeField] private List<CharacterUnlockUIData> characterDataList;

    [Header("책갈피 버튼들")]
    [SerializeField] private List<RectTransform> tabButtons;  // 책갈피 UI의 Transform들

    [Header("미리 생성한 레시피 프리팹")]
    [SerializeField] private RecipeSlotPool recipeSlotPool;

    private CharacterClass? currentVisibleCharacter = null;     //현재 캐릭터

    private Dictionary<CharacterClass, CharacterUnlockUIData> characterDataDict;


    private void OnEnable()
    {
        CardUnlocker.OnRecipeUnlocked += RefreshAllUI;
    }

    private void OnDisable()
    {
        CardUnlocker.OnRecipeUnlocked -= RefreshAllUI;
    }

    private void Awake()
    {
        characterDataDict = characterDataList.ToDictionary(c => c.character, c => c);
    }

    public override void Open()
    {
        base.Open();
        OpenUnlockPopup();
    }


    public void RefreshAllUI(CharacterClass character)
    {
        unlockItemPrefab.UpdateUI();               // 아이템 개수 갱신
        recipeSlotPool.UpdateRecipeButtons(character); // 버튼 활성화 여부 갱신
    }


    public void OnTabSelected(int characterIndex)
    {
        if (currentVisibleCharacter != null)
            recipeSlotPool.OffCharacterRecipe(currentVisibleCharacter.Value);

        CharacterClass selectedClass = (CharacterClass)characterIndex;

        recipeSlotPool.OnCharacterRecipe(selectedClass);

        RefreshAllUI(selectedClass);

        if (characterDataDict.TryGetValue(selectedClass, out var data))
        {
            characterImage.sprite = data.characterSprite;
            FlipImage(characterImage);
            FlipSpeechBubble();

            if (data.dialogue != null && data.dialogue.Length > 0)
            {
                speechBubbleText.text = data.dialogue[Random.Range(0, data.dialogue.Length)];
                speechBubbleObject.SetActive(true);
            }
            else
            {
                speechBubbleText.text = "";
                speechBubbleObject.SetActive(false);
            }
        }

        // 책갈피 중 선택된 것만 가장 위로
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (i == characterIndex)
            {
                tabButtons[i].SetAsLastSibling(); // 위로
            }
            else
            {
                tabButtons[i].SetSiblingIndex(i); // 원래 순서 유지
            }
        }

        currentVisibleCharacter = selectedClass;
    }

    void FlipImage(Image image)
    {
        Vector3 scale = image.rectTransform.localScale;
        scale.x = -Mathf.Abs(scale.x); // 무조건 좌우 반전
        characterImage.rectTransform.localScale = scale;
    }

    /// 말풍선 반전용
    void FlipSpeechBubble()
    {
        // 말풍선 전체 좌우 반전
        Vector3 bubbleScale = speechBubbleObject.transform.localScale;
        bubbleScale.x = -Mathf.Abs(bubbleScale.x);
        speechBubbleObject.transform.localScale = bubbleScale;

        // 텍스트는 다시 원래대로 (오른쪽에서 보이게)
        Vector3 textScale = speechBubbleText.rectTransform.localScale;
        textScale.x = -1f;
        speechBubbleText.rectTransform.localScale = textScale;
    }

    //팝업 켜기
    public void OpenUnlockPopup()
    {
        popupPanel.SetActive(true); // 이 타이밍에 Enable 발생
        OnTabSelected(0); //기존 창으로 이동
    }

    //팝업 끄기(모험 새로하기)
    public void CloseUnlockPopup()
    {
        // 초기화 하고 새로 시작

        // 플레이어 덱 초기화
        foreach (var player in PlayerManager.Instance.activePlayers.Values)
        {
            player.currentDeckIndexes.Clear();
        }

        // 보유 플레이어 초기화
        PlayerManager.Instance.activePlayers.Clear();

        // 데이터 초기화
        ProgressDataManager.Instance.ResetProgress();
        ProgressDataManager.Instance.IsSecondGame = true;
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);

        Close(); // 팝업 닫기
    }
}


