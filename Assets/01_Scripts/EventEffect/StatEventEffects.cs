using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatEventEffects : EventEffects
{
    // 적용 대상
    public bool sophia;
    public bool kyla;
    public bool leon;
    public bool enemy;

    // value 값
    public int hp; // 단순 증감
    public float hpPercent; // 퍼센트 변화
    public int atk; // 단순 증감
    public int def; // 단순 증감

    public override void Apply()
    {

    }
    public override EventEffects Clone()
    {
        return Instantiate(this);
    }
}
