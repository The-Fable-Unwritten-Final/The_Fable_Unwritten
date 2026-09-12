using UnityEngine;
using UnityEngine.UI;

public class StanceIconDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;

    [Header("Sophia")]
    [SerializeField] private Sprite seekIcon;
    [SerializeField] private Sprite insightIcon;

    [Header("Kayla")]
    [SerializeField] private Sprite mercyIcon;
    [SerializeField] private Sprite disciplineIcon;

    [Header("Leon")]
    [SerializeField] private Sprite rushIcon;
    [SerializeField] private Sprite defenseIcon;
    private void Awake()
    {
        if (icon != null)
            icon.enabled = false;
    }

    public void UpdateIcon(StancType stance)
    {
        if (icon == null)
            return;

        Sprite sprite = stance switch
        {
            StancType.Seek => seekIcon,
            StancType.Insight => insightIcon,

            StancType.Mercy => mercyIcon,
            StancType.Discipline => disciplineIcon,

            StancType.Rush => rushIcon,
            StancType.Defense => defenseIcon,

            _ => null
        };

        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }
}