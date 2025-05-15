using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] GameObject[] emphasizeObject;
    [SerializeField] GameObject nextTutorial;
    public bool firstTutorial;    

    public GameObject[] EmphasizeObject 
    { 
        get { return emphasizeObject; }
        private set { emphasizeObject = value; }
    }

    public void NextTutorial()
    {
        gameObject.SetActive(false);

        if (nextTutorial != null)
        {
            nextTutorial.SetActive(true);
            return;
        }
        
        GetComponentInParent<TutorialController>().brulImg.SetActive(false);
    }
}

