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
        Invoke(nameof(StartTutorial), 1f);
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
        if (pmd.CurrentBattleNode == null) return;

        string scene = SceneManager.GetActiveScene().name;
        int stage = pmd.StageIndex;
        int col = pmd.CurrentBattleNode.columnIndex;

        if (stage == 1 && pmd.IsNewStage && scene == SceneNameData.CombatScene)
        {
            ShowTutorial(0);
            ShowTutorial(1);
        }
        else if (stage == 1 && col == 1 && scene == SceneNameData.StageScene)
        {
            ShowTutorial(2);
        }
        else if (stage == 1 && col == 2 && scene == SceneNameData.CombatScene)
        {
            ShowTutorial(3);
            ShowTutorial(4);
        }
        else if (stage == 1 && col == 3 && scene == SceneNameData.CombatScene)
        {
            ShowTutorial(5);
        }
        else if (stage == 2 && pmd.IsNewStage && scene == SceneNameData.StageScene)
        {
            ShowTutorial(6);
            ProgressDataManager.Instance.IsSecondGame = true; // 튜토리얼 스테이지 클리어 판정 
            ProgressDataManager.Instance.SaveProgress(true); // 클리어 판정 save
        }
        else if (pmd.IsNewCamp && scene == SceneNameData.CampScene)
        {
            ShowTutorial(7);
        }
    }

    public void ShowTutorial(int index)
    {

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
