using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCameraFocusAnimation : StateMachineBehaviour
{
    // 이상 실현 등의 애니메이션 시전시 줌인 된 카메라를, 애니메이션이 끝날때 자동으로 종료해주는 스크립트
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.Instance.combatCameraController.StopCoroutine(GameManager.Instance.combatCameraController.combatCameraCoroutine);
        GameManager.Instance.combatCameraController.combatCameraCoroutine = null;
    }
}
