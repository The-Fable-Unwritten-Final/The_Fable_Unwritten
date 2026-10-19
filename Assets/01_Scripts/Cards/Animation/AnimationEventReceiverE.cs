using UnityEngine;

public class AnimationEventReceiverE : MonoBehaviour
{
    private Enemy owner;

    private void Awake()
    {
        owner = GetComponentInParent<Enemy>();
    }

    public void OnAttackHitEvent()
    {
        owner?.OnAttackHitEvent();
    }
}