using Unity.VisualScripting;
using UnityEngine;
using static IdealData;

public class IdealManager : MonoBehaviour
{
    [SerializeField] private IdealDatabase database;
    [SerializeField] private IdealOverlayUI overlay;

    public void OpenFor(PlayerController pc)
    {
        if (!database || !overlay || !pc) return;
        database.TryGetPair(pc.ChClass, out var a, out var b);
        // 기획: 발동 불가/미해금이어도 동일 UI가 뜨고, 버튼만 비활성/실루엣 처리
        overlay.Open(pc, a, b, OnSelected);
    }

    private void OnSelected(IdealSkillBase skill, PlayerController owner)
    {
        if (skill != null && owner != null && skill.CanActivate(owner))
        {
            skill.Activate(owner);
            // 연출/스프라이트 교체/애니메이션 등 필요시 여기서
            // owner.Animator.SetTrigger("Ideal"); 등
        }
    }
}