using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingPanel : BaseCampPanel
{
    public void OnClickTarget(int characterIndex)
    {
        CharacterClass selectCharacter = (CharacterClass)characterIndex;
        Debug.Log($" {selectCharacter} 선택됨 - 공격력 2턴 증가 예정");
        // 캐릭터의 2턴 동안 공격력 증가 버프 함수 추가

        //고유사운드 출력
        StartCoroutine(FadeExit());
    }

    public void OnSophiaClicked() => OnClickTarget(CharacterClass.Sophia);
    public void OnKaylaClicked() => OnClickTarget(CharacterClass.Kayla);
    public void OnLeonClicked() => OnClickTarget(CharacterClass.Leon);

    public void OnClickTarget(CharacterClass characterClass)
    {
        SoundManager.Instance.PlaySFX(SoundCategory.Player, (int)characterClass);
        switch (characterClass)
        {
            case CharacterClass.Sophia:
                EventEffectManager.Instance.AddEventEffect(1); // 소피아 수련
                break;
            case CharacterClass.Kayla:
                EventEffectManager.Instance.AddEventEffect(2); // 카일라 수련
                break;
            case CharacterClass.Leon:
                EventEffectManager.Instance.AddEventEffect(3); // 레온 수련
                break;
        }
  
        StartCoroutine(FadeExit());
    }
}
