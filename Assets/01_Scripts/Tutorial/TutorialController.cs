using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [SerializeField] Tutorial[] tutorials;

    public GameObject brulImg;

    private void Start()
    {
        GameManager.Instance.RegisterTutorialController(this);
        // Stage 2 진입 시 더 긴 대기 시간 필요 (노드 선택 전 튜토리얼 방지)
        float delayTime = ProgressDataManager.Instance.StageIndex == 2 ? 2f : 1f;
        Invoke(nameof(StartTutorial), delayTime);
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    void OnSceneUnloaded(Scene scene)
    {
        GameManager.Instance.UnRegisterTutorialController();
    }

    private void StartTutorial()
    {
        var pmd = ProgressDataManager.Instance;
        
        // Stage 2는 노드 선택 없이 시작되므로, Stage 2가 아닐 때만 null 체크
        int stage = pmd.StageIndex;
        if (stage != 2 && pmd.CurrentNode == null) return;

        string scene = SceneManager.GetActiveScene().name;
        int col = stage == 2 ? 0 : pmd.CurrentNode.columnIndex;

        if (stage == 1 && pmd.IsNewStage && scene == SceneNameData.CombatScene)
        {
            ShowTutorial(0);
            ShowTutorial(1);
        }
        else if (stage == 1 && col == 1 && scene == SceneNameData.StageScene)
        {
            ShowTutorial(2); // >> 문체의 효과에 대한 짧은 설명 (1-1 끝낸 후 출력)
        }
        else if (stage == 1 && col == 2 && scene == SceneNameData.CombatScene)
        {
            // 1-2 전투 시작 시 출력
            // // 지금은 스탠스 내용인데, 이걸 제거 후, 상태 이상 아이콘들을 보여 주며
            // 대상 체력바 하단에 등장하는 상태이상은 마우스를 올리면 상세 내용이 확인 가능함을 통보.
            ShowTutorial(3); 
        }
        else if (stage == 1 && col == 3 && scene == SceneNameData.CombatScene)
        {
            ShowTutorial(4); // 카드 체인 강화 효과 설명인데 테두리 발광 이미지와 함께하는 추가 설명이 필요해 보임
            // 색상에 따른 체인 설명.(현재 선택중인 카드 색상~~ 오렌지, 사용 시 강화되는 카드 ~~ 보라색,현재 강화 상태인 카드 ~~~ 파란색 색상 기반 예시 설명)
        }
        else if (stage == 2 && pmd.IsNewStage && scene == SceneNameData.StageScene)
        {
            ShowTutorial(5); // 각 노드들에 대한 간단한 설명으로 변경하기

            ProgressDataManager.Instance.IsSecondGame = true; // 튜토리얼 스테이지 클리어 판정 
            ProgressDataManager.Instance.SaveProgress(true); // 클리어 판정 save
        }
        else if (pmd.IsNewCamp && scene == SceneNameData.CampScene)
        {
            //ShowTutorial(6); // 캠프의 각 요소들에 대한 설명
            ProgressDataManager.Instance.AddProgressTutorial(6);
            // 캠프 튜토리얼 표시 안함. 충분히 직관적인 UI에, 버튼 호버 시 상세 설명이 표기됨.
        }
        else
        {
            //Debug.Log("No tutorial condition matched");
        }
    }

    public void ShowTutorial(int index)
    {
        GameManager.Instance.analyticsLogger.LogTutorialStep(index); // 애널리틱스 기록

#if !UNITY_EDITOR
        if (ProgressDataManager.Instance.ProgressTutorial.Contains(index) ||
        ProgressDataManager.Instance.IsEndingClear) return;
#endif

        ProgressDataManager.Instance.AddProgressTutorial(index);
        brulImg.SetActive(true);
        var obj = tutorials[index];

        obj.gameObject.SetActive(true);

        if (obj.EmphasizeObject == null) return;

        for (int i = 0; i < obj.EmphasizeObject.Length; i++)
        {
            if (obj.EmphasizeObject[i] == null) continue;
            
            var clon =Instantiate(obj.EmphasizeObject[i], tutorials[index].transform);
            clon.transform.SetAsFirstSibling();
            if(clon.TryGetComponent(out CardDisplay deck))
            {
                for (int j = 0; j < deck.cardsInHand.Count; j++)
                {
                    deck.cardsInHand[j].GetComponent<Image>().raycastTarget = false; // 튜토리얼용 카드는 비활성화
                }
            }
        }

        if (!obj.firstTutorial)
            obj.gameObject.SetActive(false);
    }

    public void GoStageScene()
    {
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
    }
}
