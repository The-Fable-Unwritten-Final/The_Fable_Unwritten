using UnityEngine;

public class IdealData
{
    public enum IdealId
    {
        Sophia_ArcaneMage,   // 비전술사
        Sophia_Alchemist,    // 연금술사
        Kayla_Sanctify,      // 성화
        Kayla_Judge,         // 심판자
        Leon_Lionheart,      // 라이온하트
        Leon_ShadowWarrior,  // 그림자 전사
    }

    public enum IdealAvailability
    {
        Locked,
        Used,
        Unavailable,
        Available
    }

    public abstract class IdealSkillBase : ScriptableObject
    {
        public IdealId Id;
        public abstract string DisplayName { get; }
        public Sprite Icon;
        public Sprite Portrait;
        public string Description;


        /// <summary>
        /// 지금 전투/스테이지에서 발동 가능한가(스테이지 1회 제한 포함)
        /// </summary>
        /// <param name="owner">대상자</param>
        /// <returns></returns>
        public bool CanActivate(PlayerController owner)
            => GetUseInfo(owner) == IdealAvailability.Available;

        /// <summary>
        /// UI가 회색 처리/툴팁 표시에 쓰는 종합 판단(해금 여부 + 현재 가능여부)
        /// </summary>
        /// <param name="owner">대상자</param>
        /// <returns></returns>
        public IdealAvailability GetUseInfo(PlayerController owner)
        {
            if (isUnlocked(Id)) return IdealAvailability.Locked;
            if (owner.IsIdealUsed) return IdealAvailability.Used;
            if (!owner.IsAlive() || owner.IsStunned()) return IdealAvailability.Unavailable;
            return IdealAvailability.Available;
        }

        /// <summary>
        /// 실제 발동. 여기서 owner.HasUsedIdealThisStage = true; 를 반드시 셋
        /// </summary>
        /// <param name="owner">대상자</param>
        public abstract void Activate(PlayerController owner);

        public static bool isUnlocked(IdealId id)
        {
            return false;
            //return ProgressDataManager.Instance?.IsIdealUnlocked(id) ?? false;
        }
    }
}
