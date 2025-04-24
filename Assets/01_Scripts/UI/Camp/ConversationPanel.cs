using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationPanel : BaseCampPanel
{

    public void OnClickConfirm()
    {
        CharacterClass randomCharacter = GetRadomCharcter();

        Debug.Log($"{randomCharacter} 와 대화");

        // 해당 캐릭터{randomCharacter} 와 대화 시스템 가져오기
    }

    // 랜덤하게 캐릭터 가져오기
    private CharacterClass GetRadomCharcter()
    {
        CharacterClass[] characters = new CharacterClass[]
        {
            CharacterClass.Sophia,
            CharacterClass.Kayla,
            CharacterClass.Leon
        };

        int index = Random.Range(0, characters.Length);

        return characters[index];
    }
}
