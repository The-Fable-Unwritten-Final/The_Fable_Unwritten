using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    [SerializeField] Tutorial[] tutorials;

    public GameObject brulImg;

    private void Start()
    {
        Invoke(nameof(StartTutorial), 0.5f);
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
        }
        else if (pmd.IsNewCamp && scene == SceneNameData.CampScene)
        {
            ShowTutorial(7);
        }
    }

    public void ShowTutorial(int index)
    {
        if (ProgressDataManager.Instance.ProgressTutorial.Contains(index)) return;
        
        ProgressDataManager.Instance.AddProgressTutorial(index);
        brulImg.SetActive(true);
        var obj = tutorials[index];

        obj.gameObject.SetActive(true);

        if (obj.EmphasizeObject == null) return;

        for (int i = 0; i < obj.EmphasizeObject.Length; i++)
        {
            var clon =Instantiate(obj.EmphasizeObject[i], tutorials[index].transform);
            clon.transform.SetAsFirstSibling();
        }

        if (!obj.firstTutorial)
            obj.gameObject.SetActive(false);
    }

    public void GoStageScene()
    {
        UIManager.Instance.nextSceneFade.StartSceneTransition(SceneNameData.SubTitleScene);
    }
}
