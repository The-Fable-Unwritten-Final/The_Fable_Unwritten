using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CampCardBook : PopupUI_Book
{
    public CharacterClass Character { get; set; }

    private void OnEnable()
    {
        SetBook(Character);
    }

    private void SetBook(CharacterClass characterClass)
    {
        switch (characterClass)
        {
            case CharacterClass.Sophia:
                OnClick0();
                break;
            case CharacterClass.Kayla:
                OnClick1();
                break;
            case CharacterClass.Leon:
                OnClick2();
                break;
        }
    }
}
