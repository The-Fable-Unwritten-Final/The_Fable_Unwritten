using UnityEngine;
using DG.Tweening;

public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void OnAttackHitEvent()
    {
        if (playerController != null)
        {
            playerController.OnAttackHitEvent();
        }
    }
}