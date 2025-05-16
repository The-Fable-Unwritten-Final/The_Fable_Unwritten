using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "NewEffectAnimation", menuName = "Effect/EffectAnimation")]
public class EffectAnimation : ScriptableObject
{
    public string animationName;
    public List<Sprite> frames;
    public AnimationType animationType;
}

public enum AnimationType       //추후 적용 애니메이션
{
    Projectile,  //날아가는 이펙트
    OnTarget,    //대상에 바로 발생
    OnBottomTarget, //대상 bottom과 스프라이트의 bottom을 맞춰야 함
    Looping,     //대상에 지속
    AOE          //대상에 각자 적용이 아닌 대상 전체에 하나의 이펙트로 적용
}
