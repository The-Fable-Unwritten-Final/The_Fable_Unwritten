using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlotPool : MonoBehaviour
{
    [SerializeField] private GameObject recipePrefab;
    [SerializeField] private Transform recipeParent;

    [SerializeField] private Dictionary<CharacterClass,List<GameObject>> pooled = new();

    [SerializeField] private List<Sprite> lootIcons;
    [SerializeField] private GameObject materialSlot;
    [SerializeField] private GameObject plusTemplate;
    [SerializeField] private GameObject equalTemplate;
    [SerializeField] private GameObject resultIcon;


    private void Start()
    {
        PreloadAllRecipes();
    }

    private void PreloadAllRecipes()
    {
        var allRecipes = DataManager.Instance.LoadedRecipes;

        foreach (var recipe in allRecipes)
        {
            var go = Instantiate(recipePrefab, recipeParent);
            
            for(int i = 0; i < recipe.materials.Count; i++)
            {
                var materialGO = Instantiate(materialSlot, go.transform);
                var icon = materialGO.transform.Find("Image").GetComponent<Image>();
                var count = materialGO.transform.Find("Text").GetComponent<TextMeshProUGUI>();

                icon.sprite = lootIcons[recipe.materials[i].index];
                count.text = recipe.materials[i].count.ToString();

                if (i < recipe.materials.Count - 1)
                {
                    Instantiate(plusTemplate, go.transform);
                }
            }
            Instantiate(equalTemplate, go.transform);
            var resultGO = Instantiate(resultIcon, go.transform);
            var button = resultGO.GetComponentInChildren<Button>();
            button.image.sprite = GetResultIcon(recipe.result);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log("합성 시도: " + recipe.result);
                CardUnlocker.TryUnlock(recipe);
            });

            go.SetActive(false);

            if (!pooled.ContainsKey(recipe.character))
                pooled[recipe.character] = new List<GameObject>();

            pooled[recipe.character].Add(go);
        }
    }

    public void ReturnAll()
    {
        foreach (var list in pooled.Values)
        {
            foreach (var obj in list)
            {
                obj.SetActive(false);
            }
        }
    }


    public void OnCharacterRecipe(CharacterClass character)
    {
        if (!pooled.ContainsKey(character))
            return;

        foreach (var obj in pooled[character])
        {
            obj.SetActive(true);
        }
    }

    public void OffCharacterRecipe(CharacterClass character)
    {
        if (!pooled.ContainsKey(character))
            return;

        foreach(var obj in pooled[character])
        {
            obj.SetActive(false);
        }
    }


    private Sprite GetResultIcon(CardType type)
    {
        // CardType에 따라 리소스에서 아이콘 로드 (예: Resources/Type/type_0.png)
        return Resources.Load<Sprite>($"Cards/Type/type_{(int)type}");
    }
}
