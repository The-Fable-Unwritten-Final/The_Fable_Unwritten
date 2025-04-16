public class CardData           //csv 인덱스에 따라 데이터를 받아오는 파서
{
    public bool isUnlocked = false;              //카드가 해금 되었는지
    public int index;                  // 카드 고유 번호
    public int cost;                   // 카드 코스트
    public string illustration;        // 일러스트 이름
    public string cardName;            // 카드 이름
    public string text;                // 카드 설명
    public int type;                   // 카드 타입 (0~11)
    public int classIndex;             // 클래스 인덱스 (0~2)
    public string cardFrame;           // 카드 프레임 리소스명

    public int targetType;             // 대상 타입 (0=없음, 1=플레이어, 2=에너미)
    public int targetNum;              // 타겟 수 (0~3)

    public float damage;               // 피해량 (음수일 경우 회복량)
    public int discount;               // 코스트 감소량
    public int draw;                   // 카드 드로우 수 (음수면 버리기)
    public int redraw;                 // 다시 뽑기 수

    public float atkBuff;              // 공격력 버프량
    public float defBuff;              // 방어력 버프량
    public int buffTime;               // 버프 지속 턴 수

    public float selfDamage;           // 자해 데미지
    public int block;                  // 블록 여부 (0 또는 1)
    public int blind;                  // 블라인드 위치 (0: 상단, 1: 중단, 2: 하단)
    public int stun;                   // 스턴 여부 (0 또는 1)

    public string characterStance;     // 요구 자세
    public string description;         // 조건부 효과 또는 부가 설명
}
