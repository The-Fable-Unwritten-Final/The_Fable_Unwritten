using System.Collections.Generic;

[System.Serializable]
public class JsonCutsceneData
{
    public string id;                    // 씬 ID
    public string type;                  // "basic", "animation", "blackout"
    public string nameString;           // 대화창에 표시될 이름
    public string chatString;           // 실제 대사 텍스트
    public string animationName;        // 연출용 애니메이션 이름
    public string bgName;               // 배경 이미지 (필요 시)
    public string sfx;                  // 효과음 이름 (필요 시)

    public List<JsonCharacterData> charactersData = new();  // 등장 캐릭터
    public List<JsonPropsData> propsData = new();            // 등장 소품
}