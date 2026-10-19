using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "NewEffectAnimation", menuName = "Effect/EffectAnimation")]
public class EffectAnimation : ScriptableObject
{
    public string animationName;
    public AnimationType animationType;
    public EffectPlayMode playMode = EffectPlayMode.SpriteFrames;

    [Header("Sprite 방식")]
    public List<Sprite> frames;
    public int hitFrame = -1;

    [Header("Animation Clip 방식")]
    public AnimationClip animationClip;
    public float clipHitTime = -1f;

    [HideInInspector]
    public Sprite referenceSprite;

    [Header("크기")]
    public Vector2 scale = Vector2.one;

#if UNITY_EDITOR
    private void OnValidate()
    {
        referenceSprite = null;

        if (animationClip == null)
            return;

        var bindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(animationClip);

        Sprite largestSprite = null;
        float largestArea = -1f;

        foreach (var binding in bindings)
        {
            var keyframes = UnityEditor.AnimationUtility.GetObjectReferenceCurve(animationClip, binding);

            if (keyframes == null || keyframes.Length == 0)
                continue;

            foreach (var keyframe in keyframes)
            {
                if (keyframe.value is not Sprite sprite)
                    continue;

                float area = sprite.rect.width * sprite.rect.height;

                if (area > largestArea)
                {
                    largestArea = area;
                    largestSprite = sprite;
                }
            }
        }

        referenceSprite = largestSprite;
    }
#endif
}

public enum AnimationType       //추후 적용 애니메이션
{
    Projectile,  //날아가는 이펙트

    OnTarget,    //대상에 바로 발생
    OnBottomTarget, //대상 bottom과 스프라이트의 bottom을 맞춰야 함
    OnHeadPoint,     // HeadPoint: 머리 직접 타격, 머리 주변 폭발
    OnOverheadPoint, // OverheadPoint: 위에서 떨어짐, 우박, 낙뢰
    OnAheadPoint,   //대상 바로 앞
    OnAheadMidPoint,

    Looping,     //대상에 지속
    AOE          //대상에 각자 적용이 아닌 대상 전체에 하나의 이펙트로 적용
}

public enum EffectPlayMode
{
    SpriteFrames,
    AnimationClip
}