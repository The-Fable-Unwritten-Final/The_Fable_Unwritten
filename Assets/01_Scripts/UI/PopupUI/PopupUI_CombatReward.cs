using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class PopupUI_CombatReward : BasePopupUI
{
    // 게임 최종 출시 시점에서는 전투 결과 창을 어떻게 할지 모르겠지만
    // 일단은 전투가 끝나면 관련 데이터 정리의 대부분이 이곳에서 이루어 지니 만약 교체 시
    // 해당 코드를 상세히 읽어 볼것.
    // (최종 클리어 시 관련 연산 및 문체 해금, 애널리틱스 기록 등의 코드가 존재)
    [SerializeField] TextMeshProUGUI resultText; // 결과 텍스트
    [SerializeField] GameObject winText; // 승리 시 텍스트
    [SerializeField] GameObject loseText; // 패배 시 텍스트
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] Button confirmButton;
    [SerializeField] RectTransform[] rewardCardSlots; // 카드 보상 3개
    [SerializeField] TextMeshProUGUI[] hpRewardText; // 체력 보상 3개
    [SerializeField] Image[] rewardCardImages; // 카드 보상 캐릭터 아이콘
    [SerializeField] Sprite[] characterIcons; // 캐릭터 아이콘 (0: 소피아, 1: 카일라, 2: 레온)


    private CardModel selectedRewardCard; // 보상 선택 카드
    private List<CardModel> rewardCardCandidates = new(); // 보상 후보 카드 리스트
    private List<Button> rewardCardButtons = new(); // 보상 카드 버튼 리스트
    private List<RectTransform> rewardCardRectTransforms = new(); // 보상 카드 RectTransform 리스트 (애니메이션용)
    private List<Vector3> initialCardPositions = new(); // 카드의 초기 위치 저장

    private void OnEnable()
    {
        short iswin = GameManager.Instance.turnController.battleFlow.isWin;
        if(iswin == 1) // 전투 승리 시 출력
        {
            var setting = ProgressDataManager.Instance;
            var lasVisitde = setting.VisitedNodes.Last();

            // 튜토리얼 스테이지(1)인 경우 보상 UI 없이 바로 진행
            if (setting.StageIndex == 1)
            {
                // 랜덤 보상 카드 선택 및 추가
                var unlockedCardList = new List<int>(setting.unlockedCards);
                if (unlockedCardList.Count > 0)
                {
                    int randomIndex = Random.Range(0, unlockedCardList.Count);
                    CardModel rewardCard = DataManager.Instance.GetCardByIndex(unlockedCardList[randomIndex]);
                    
                    if (rewardCard != null)
                    {
                        PlayerData targetCharacter = null;
                        int cardIndex = rewardCard.index;
                        
                        // 카드 인덱스에 따라 대상 캐릭터 결정
                        if (cardIndex >= 1000 && cardIndex < 2000)
                        {
                            // 소피아 (1000번대)
                            targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Sophia);
                        }
                        else if (cardIndex >= 2000 && cardIndex < 3000)
                        {
                            // 카일라 (2000번대)
                            targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Kayla);
                        }
                        else if (cardIndex >= 3000 && cardIndex < 4000)
                        {
                            // 레온 (3000번대)
                            targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Leon);
                        }
                        
                        // 대상 캐릭터의 덱에 카드 추가
                        if (targetCharacter != null && targetCharacter.currentDeck != null)
                        {
                            targetCharacter.currentDeck.Add(rewardCard);
                            Debug.Log($"[PopupUI_CombatReward] {targetCharacter.CharacterName}의 덱에 카드 {cardIndex} 추가");
                        }
                    }
                }

                // 다음 노드로 진행
                setting.RetryFromStart = false;
                setting.StageCleared = true;
                setting.IsNewStage = true;
                setting.SavedEnemySetIndex = -1;
                gameObject.SetActive(false);
                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);
                return;
            }

            // 애널리틱스
            GameManager.Instance.analyticsLogger.LogStageClearInfo(setting.SavedStageData.stageIndex, lasVisitde.columnIndex); // 전투 승리시 시 애널리틱스 기록

            SoundManager.Instance.PlaySFX(SoundCategory.UI, 5); // 승리 시 효과음 적용
            winText.SetActive(true);
            loseText.SetActive(false);
            GenerateRewardCards();

            // 제거할 카드 선택 전까지 확인 버튼 비활성화
            confirmButton.gameObject.SetActive(false);
            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SoundCategory.Button, 0); // 기본 버튼 사운드
                
                // 선택된 보상 카드를 해당 캐릭터의 덱에 추가
                if (selectedRewardCard != null)
                {
                    PlayerData targetCharacter = null;
                    int cardIndex = selectedRewardCard.index;
                    
                    // 카드 인덱스에 따라 대상 캐릭터 결정
                    if (cardIndex >= 1000 && cardIndex < 2000)
                    {
                        // 소피아 (1000번대) - PlayerDatas[1]
                        targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Sophia);
                    }
                    else if (cardIndex >= 2000 && cardIndex < 3000)
                    {
                        // 카일라 (2000번대) - PlayerDatas[0]
                        targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Kayla);
                    }
                    else if (cardIndex >= 3000 && cardIndex < 4000)
                    {
                        // 레온 (3000번대) - PlayerDatas[2]
                        targetCharacter = setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Leon);
                    }
                    
                    // 대상 캐릭터의 덱에 카드 추가
                    if (targetCharacter != null && targetCharacter.currentDeck != null)
                    {
                        targetCharacter.currentDeck.Add(selectedRewardCard);
                    }
                    else
                    {
                        Debug.LogWarning($"[PopupUI_CombatReward] 카드 인덱스 {cardIndex}에 해당하는 캐릭터를 찾을 수 없습니다.");
                    }
                    
                    // 해금 카드 목록에도 추가 (이미 해금되었지만 명시적으로)
                    setting.unlockedCards.Add(selectedRewardCard.index);
                    
                    // 체력 증가 적용
                    int hpIncrease = CalculateHPIncrease(selectedRewardCard);
                    if (targetCharacter != null)
                    {
                        CharHPAdd(targetCharacter, hpIncrease);
                    }

                    // 카드 보상 선택 애널리틱스 기록
                    int cardCount = targetCharacter.currentDeck.Count(c => c.index == selectedRewardCard.index);
                    GameManager.Instance.analyticsLogger.LogSelectCardInfo(selectedRewardCard.index,cardCount);

                }
                
                setting.RetryFromStart = false;
                setting.StageCleared = true;

                // 1 스테이지 클리어 후, 실패 시 2스테이지부터 시작하게 설정
                if (setting.StageIndex == 1)
                {
                    var lasColum = setting.SavedStageData.columns[^1];

                    if (lasColum.Contains(lasVisitde))
                    {
                        setting.MinStageIndex = 2;
                    }
                }

                if (setting.CurrentNode.type == NodeType.Boss || 
                    (setting.StageIndex == 1 && setting.CurrentNode.columnIndex == 3))
                {
                    setting.IsNewStage = true;
                }
                else
                {
                    setting.IsNewStage = false;
                }

                if (setting.CurrentNode.type == NodeType.EliteBattle)
                {
                    setting.EliteClear(setting.CurrentTheme);
                }
                ProgressDataManager.Instance.SavedEnemySetIndex = -1; // 랜덤 에너미 셋 초기화

                gameObject.SetActive(false);
                
                // 엔딩일시 표시(테스트 버전용 - 스테이지 4 클리어시 얼리 액세스 기준 엔드)
                if (setting.CurrentNode.type == NodeType.Boss
                    && setting.StageIndex == 4)
                {
                    ProgressDataManager.Instance.IsEndingClear = true;
                    ProgressDataManager.Instance.UnlockRandomStyle(); // 앤딩 클리어시 랜덤 문체 해금

                    // 앤딩 처음인경우
                    if (!ProgressDataManager.Instance.ProgressTutorial.Contains(7))
                    {
                        GameManager.Instance.tutorialController.ShowTutorial(7);
                        ProgressDataManager.Instance.ResetProgress();
                        return;
                    }
                    // 앤딩 두번째 이후
                    else
                    {
                        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
                        return;
                    }
                }

                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.StageScene);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
        else // 전투 패배 시 출력
        {
            var setting = ProgressDataManager.Instance;
            var lasVisitde = setting.VisitedNodes.Last();
            // 애널리틱스
            GameManager.Instance.analyticsLogger.LogStageFailInfo(setting.SavedStageData.stageIndex, lasVisitde.columnIndex); // 전투 패배 시 애널리틱스 기록

            winText.SetActive(false);
            loseText.SetActive(true);
            // 리워드 로드 + 텍스트 표시
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                ProgressDataManager.Instance.RetryFromStart = true;
                ProgressDataManager.Instance.ClearStageState();

                // 최소 시작 스테이지부터 재시작 (1 또는 2)
                ProgressDataManager.Instance.StageIndex = ProgressDataManager.Instance.MinStageIndex;
                ProgressDataManager.Instance.GameStartType = GameStartType.New;
                ProgressDataManager.Instance.ResetProgress();

                UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
                gameObject.SetActive(false);

                // StageMoveTest.cs 를 임시로 가져만 왔음. 추후 전투 승리/패배시 기능 재 구현
            });
        }
    }
    

    private void GenerateRewardCards()
    {
        // 1. 해금된 카드 중에서 중복 없이 3개 선택
        var unlockedCardList = new List<int>(DataManager.Instance.AllCards
            .Where(card => card.isUnlocked)
            .Select(card => card.index)
            .ToList());
        
        if (unlockedCardList.Count < 3)
        {
            Debug.LogWarning("[PopupUI_CombatReward] 해금된 카드가 3개 미만입니다.");
            return;
        }

        rewardCardCandidates.Clear();
        rewardCardButtons.Clear();
        rewardCardRectTransforms.Clear();
        initialCardPositions.Clear();
        selectedRewardCard = null;

        // 무작위로 섞은 후 3개 선택
        for (int i = 0; i < unlockedCardList.Count; i++)
        {
            int randomIndex = Random.Range(i, unlockedCardList.Count);
            int temp = unlockedCardList[i];
            unlockedCardList[i] = unlockedCardList[randomIndex];
            unlockedCardList[randomIndex] = temp;
        }

        // 선택된 3개 카드의 데이터 수집
        for (int i = 0; i < 3; i++)
        {
            CardModel cardData = DataManager.Instance.GetCardByIndex(unlockedCardList[i]);
            if (cardData != null)
            {
                rewardCardCandidates.Add(cardData);
            }
        }

        // 2. 각 슬롯에 카드 배치
        for (int i = 0; i < rewardCardCandidates.Count && i < rewardCardSlots.Length; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            cardObj.transform.SetParent(rewardCardSlots[i], false);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            
            // 스케일과 위치 설정
            cardRect.localScale = new Vector3(3.0f, 3.0f, 3.0f);
            cardRect.localPosition = new Vector3(0, 0, cardRect.localPosition.z);

            // CardInHand로 이미지/텍스트 업데이트
            CardInHand cardInHand = cardObj.GetComponent<CardInHand>();
            if (cardInHand != null)
            {
                cardInHand.cardData = rewardCardCandidates[i];
                cardInHand.UpdateCardImage();
                cardInHand.UpdateCardInfoOnlyUI();
            }

            // Button 컴포넌트 확인
            Button cardBtn = cardObj.GetComponent<Button>();
            if (cardBtn == null)
            {
                cardBtn = cardObj.AddComponent<Button>();
            }

            // targetGraphic 설정 (PopupUI_Maintenance와 동일 방식)
            if (cardInHand != null)
            {
                GameObject illustCoverObj = cardInHand.GetCoverIllust();
                if (illustCoverObj != null)
                {
                    Image illustCover = illustCoverObj.GetComponent<Image>();
                    if (illustCover != null)
                    {
                        cardBtn.targetGraphic = illustCover;
                    }
                }
            }

            // CardInHand 컴포넌트 제거
            if (cardInHand != null)
            {
                DestroyImmediate(cardInHand);
            }

            // 체력 증가량 계산 및 텍스트 표시
            CardModel selectedCard = rewardCardCandidates[i];
            int hpIncrease = CalculateHPIncrease(selectedCard);
            HPRewardTextUpdate(i, selectedCard, hpIncrease);

            // 캐릭터 아이콘 설정 및 크기 조정
            int cardIndex = selectedCard.index;
            int characterIconIndex = -1;
            
            if (cardIndex >= 1000 && cardIndex < 2000)
            {
                // 소피아 (1000번대)
                characterIconIndex = 0;
            }
            else if (cardIndex >= 2000 && cardIndex < 3000)
            {
                // 카일라 (2000번대)
                characterIconIndex = 1;
            }
            else if (cardIndex >= 3000 && cardIndex < 4000)
            {
                // 레온 (3000번대)
                characterIconIndex = 2;
            }
            
            if (characterIconIndex >= 0 && characterIconIndex < characterIcons.Length && 
                i < rewardCardImages.Length && rewardCardImages[i] != null)
            {
                Sprite charIcon = characterIcons[characterIconIndex];
                rewardCardImages[i].sprite = charIcon;
                
                // Sprite의 이미지 크기에 맞게 RectTransform 크기 조정
                if (charIcon != null && charIcon.rect.width > 0 && charIcon.rect.height > 0)
                {
                    RectTransform iconRect = rewardCardImages[i].GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        iconRect.sizeDelta = new Vector2(charIcon.rect.width, charIcon.rect.height);
                    }
                }
            }

            // Button 이벤트: 카드 선택 + confirmButton 활성화
            cardBtn.onClick.AddListener(() => OnRewardCardSelected(selectedCard, cardRect));

            rewardCardButtons.Add(cardBtn);
            rewardCardRectTransforms.Add(cardRect);
            initialCardPositions.Add(cardRect.localPosition); // 초기 위치 저장
        }
    }

    private void OnRewardCardSelected(CardModel card, RectTransform selectedCardRect)
    {
        // 이미 선택된 카드와 동일한 카드를 다시 선택한 경우 무시
        if (selectedRewardCard == card)
            return;

        selectedRewardCard = card;

        // 모든 카드의 애니메이션 상태 업데이트
        for (int i = 0; i < rewardCardRectTransforms.Count; i++)
        {
            RectTransform cardRect = rewardCardRectTransforms[i];
            if (cardRect == null) continue;

            // 기존 애니메이션 중지
            cardRect.DOKill();

            if (cardRect == selectedCardRect)
            {
                // 선택된 카드: 초기 위치로부터 부유하는 애니메이션 (Yoyo 루프로 끊김 없이 반복)
                Vector3 initialPos = initialCardPositions[i];
                cardRect.DOLocalMoveY(initialPos.y + 20f, 0.9f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                // 비선택 카드: 초기 위치로 복귀
                Vector3 initialPos = initialCardPositions[i];
                cardRect.DOLocalMove(initialPos, 0.3f).SetEase(Ease.OutQuad);
            }
        }

        // 확인 버튼 활성화
        confirmButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 로케일 기반으로 체력 보상 텍스트를 업데이트
    /// </summary>
    private void HPRewardTextUpdate(int index, CardModel card, int hpIncrease)
    {
        if (hpRewardText == null || index >= hpRewardText.Length)
            return;

        string characterName = GetCharacterNameFromCard(card);
        string text = "";

        string langCode = LocaleDataManager.CurrentLanguageCode;
        switch (langCode)
        {
            case "ja":
                // 일본어: "キャラ名のHP @増加"
                text = $"{characterName}の<color=green>HP {hpIncrease}増加</color>";
                break;
            case "en":
                // 영어: "Character Name's HP increases by @"
                text = $"{characterName}'s <color=green>HP increases by {hpIncrease}</color>";
                break;
            case "ko":
            default:
                // 한국어: "캐릭터명 체력 @증가"
                text = $"{characterName}의 <color=green>체력 {hpIncrease}증가</color>";
                break;
        }

        hpRewardText[index].text = text;
    }
    /// <summary>
    /// 카드에 해당하는 캐릭터의 이름 반환 (로케일 기반)
    /// </summary>
    private string GetCharacterNameFromCard(CardModel card)
    {
        PlayerData character = GetCharacterFromCard(card);
        if (character == null) return "Unknown";

        string langCode = LocaleDataManager.CurrentLanguageCode;
        switch (langCode)
        {
            case "ja":
                // 일본어
                if (character.CharacterClass == CharacterClass.Leon) return "レオン";
                if (character.CharacterClass == CharacterClass.Sophia) return "ソフィア";
                if (character.CharacterClass == CharacterClass.Kayla) return "カイラ";
                break;
            case "en":
                // 영어
                if (character.CharacterClass == CharacterClass.Leon) return "Leon";
                if (character.CharacterClass == CharacterClass.Sophia) return "Sophia";
                if (character.CharacterClass == CharacterClass.Kayla) return "Kayla";
                break;
            case "ko":
                // 한국어
                if (character.CharacterClass == CharacterClass.Leon) return "레온";
                if (character.CharacterClass == CharacterClass.Sophia) return "소피아";
                if (character.CharacterClass == CharacterClass.Kayla) return "카일라";
                break;
            default:
                // 한국어
                return character.CharacterName;
        }

        return character.CharacterName;
    }
    private void CharHPAdd(PlayerData character, int hpReward)
    {
        character.AddMaxHP(hpReward);
    }

    private void OnDisable()
    {
        // 팝업 닫힐 때 모든 애니메이션 정리
        foreach (var cardRect in rewardCardRectTransforms)
        {
            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localRotation = Quaternion.identity;
            }
        }
    }

    /// <summary>
    /// 카드의 캐릭터가 현재 보유한 해당 카드의 개수에 따라 체력 증가량 계산
    /// </summary>
    private int CalculateHPIncrease(CardModel card)
    {
        PlayerData targetCharacter = GetCharacterFromCard(card);
        if (targetCharacter == null) return 0;

        // 현재 보유한 같은 카드의 개수 계산
        int cardCount = targetCharacter.currentDeck.Count(c => c.index == card.index);

        // 보유 개수에 따라 체력 증가량 결정
        if (cardCount >= 5)
            return 1;
        else if (cardCount >= 3)
            return 2;
        else
            return 3;
    }

    /// <summary>
    /// 카드의 캐릭터 정보 반환
    /// </summary>
    private PlayerData GetCharacterFromCard(CardModel card)
    {
        var setting = ProgressDataManager.Instance;
        int cardIndex = card.index;

        if (cardIndex >= 1000 && cardIndex < 2000)
        {
            return setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Sophia);
        }
        else if (cardIndex >= 2000 && cardIndex < 3000)
        {
            return setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Kayla);
        }
        else if (cardIndex >= 3000 && cardIndex < 4000)
        {
            return setting.PlayerDatas.FirstOrDefault(p => p.CharacterClass == CharacterClass.Leon);
        }

        return null;
    }
}

