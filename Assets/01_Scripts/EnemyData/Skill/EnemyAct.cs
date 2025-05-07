using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EnemyAct
{
    public int index;               //적 스킬의 고유 번호입니다.
    public TargetType targetType; // 적 기준이므로 2번이 적 기준 아군이고, 1번이 적 기준 적, 0번은 대상 없음
    public int targetNum;       //스킬의 효과가 적용될 수 있는 최대 대상 수입니다.
    public bool target_front;
    public bool target_center;
    public bool target_back;        //앞, 중간, 뒤 플레이어블 캐릭터 공격이 가능한지 표시

    public int atk_buff;
    public int def_buff;       //공 방 버프량 (음수는 디버프)

    public int buff_time;       //버프 시간

    public bool block;          //블록 버프 존재 여부
    public int stun;           //스턴 상태이상 지속 턴
}
