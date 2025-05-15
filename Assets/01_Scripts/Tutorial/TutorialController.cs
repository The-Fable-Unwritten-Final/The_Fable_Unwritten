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

        if (pmd.StageIndex == 1 && pmd.IsNewStage && SceneManager.GetActiveScene().name == SceneNameData.CombatScene)
        {
            ShowTutorial(0);
            ShowTutorial(1);
        }
        else if(pmd.StageIndex == 1 && pmd.CurrentBattleNode.columnIndex == 1 && SceneManager.GetActiveScene().name == SceneNameData.StageScene)
        {
            ShowTutorial(2);
        }
        else if(pmd.StageIndex == 1 && pmd.CurrentBattleNode.columnIndex == 2 && SceneManager.GetActiveScene().name == SceneNameData.CombatScene)
        {
            ShowTutorial(3);
            ShowTutorial(4);
        }
        else if (pmd.StageIndex == 1 && pmd.CurrentBattleNode.columnIndex == 3 && SceneManager.GetActiveScene().name == SceneNameData.CombatScene)
        {
            ShowTutorial(5);
        }
        else if(pmd.StageIndex == 2 && pmd.IsNewStage && SceneManager.GetActiveScene().name == SceneNameData.StageScene)
        {
            ShowTutorial(6);
        }
    }

    public void ShowTutorial(int index)
    {
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

    public static void LastTutorial(int index)
    {

    }
}
