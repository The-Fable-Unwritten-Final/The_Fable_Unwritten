public struct IdealProgressDelta
{
    public int Arcane_RunSuccess;       // 회차 중 '같은 타입 30회' 달성 시 +1
    public int Alchemist_BattleSuccess; // 전투 중 '서로 다른 3타입' 달성 시 +1
    public int Sanctify_RunSuccess;     // 회차 중 '카일라 누적 피해 100' 달성 시 +1
    public int Judge_Kills;             // 이번 전투에서 카일라 처치 수
    public int Lionheart_Guard;         // 이번 전투에서 수호 발동 수
    public int Shadow_Debuff;           // 이번 전투에서 레온 디버프 부여 수

    public bool IsEmpty =>
        Arcane_RunSuccess == 0 && Alchemist_BattleSuccess == 0 && Sanctify_RunSuccess == 0 &&
        Judge_Kills == 0 && Lionheart_Guard == 0 && Shadow_Debuff == 0;
}

public static class IdealCounterKeys
{
    public const string Arcane_RunSuccess = "Arcane_RunSuccess";
    public const string Alchemist_BattleSuccess = "Alchemist_BattleSuccess";
    public const string Sanctify_RunSuccess = "Sanctify_RunSuccess";
    public const string Judge_Kills = "Judge_Kills";
    public const string Lionheart_Guard = "Lionheart_Guard";
    public const string Shadow_Debuff = "Shadow_Debuff";
}