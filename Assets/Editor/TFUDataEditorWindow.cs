#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TFUDataEditorWindow : EditorWindow
{
    private enum Tab
    {
        Player,
        Card,
        Enemy,
        EnemySkill
    }

    private Tab currentTab;

    private List<PlayerData> playerList = new();
    private PlayerData selectedPlayer;

    private Vector2 leftScroll;
    private Vector2 rightScroll;

    [MenuItem("TFU/Data Editor")]
    public static void Open()
    {
        GetWindow<TFUDataEditorWindow>("TFU Data Editor");
    }

    private void OnEnable()
    {
        RefreshPlayers();
    }

    private void OnGUI()
    {
        DrawToolbar();

        GUILayout.Space(5);

        switch (currentTab)
        {
            case Tab.Player:
                DrawPlayerTab();
                break;

            case Tab.Card:
                DrawEmptyTab("Card Editor");
                break;

            case Tab.Enemy:
                DrawEmptyTab("Enemy Editor");
                break;

            case Tab.EnemySkill:
                DrawEmptyTab("Enemy Skill Editor");
                break;
        }
    }

    private void DrawToolbar()
    {
        currentTab = (Tab)GUILayout.Toolbar(
            (int)currentTab,
            new string[] { "플레이어", "카드", "적", "적 스킬" }
        );
    }

    private void DrawPlayerTab()
    {
        EditorGUILayout.BeginHorizontal();

        DrawPlayerList();
        DrawPlayerInspector();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawPlayerList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220));

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("플레이어", EditorStyles.boldLabel);

        if (GUILayout.Button("새로고침", GUILayout.Width(70)))
            RefreshPlayers();

        EditorGUILayout.EndHorizontal();

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        foreach (var player in playerList)
        {
            if (player == null)
                continue;

            string label = $"{player.IDNum} - {player.CharacterName}";

            bool selected = selectedPlayer == player;

            if (GUILayout.Toggle(selected, label, "Button"))
                selectedPlayer = player;
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawPlayerInspector()
    {
        EditorGUILayout.BeginVertical();

        if (selectedPlayer == null)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("왼쪽에서 플레이어를 선택하세요.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
            return;
        }

        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

        EditorGUILayout.LabelField(
            $"{selectedPlayer.CharacterName} Player Data",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();

        DrawBasicInfo();
        DrawStats();
        DrawGrowthValues();
        DrawInitialSettings();
        DrawAnimation();

        bool changed = EditorGUI.EndChangeCheck();

        if (changed)
            EditorUtility.SetDirty(selectedPlayer);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("저장", GUILayout.Height(30)))
            SavePlayer();

        if (GUILayout.Button("되돌리기", GUILayout.Height(30)))
            ReloadPlayer();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

        selectedPlayer.IDNum = EditorGUILayout.IntField(
            "ID",
            selectedPlayer.IDNum
        );

        selectedPlayer.CharacterName = EditorGUILayout.TextField(
            "이름",
            selectedPlayer.CharacterName
        );

        selectedPlayer.Icon = (Sprite)EditorGUILayout.ObjectField(
            "아이콘",
            selectedPlayer.Icon,
            typeof(Sprite),
            false
        );

        GUILayout.Space(10);
    }

    private void DrawStats()
    {
        EditorGUILayout.LabelField("기본 스탯", EditorStyles.boldLabel);

        selectedPlayer.MaxHP = EditorGUILayout.FloatField(
            "Max HP",
            selectedPlayer.MaxHP
        );

        selectedPlayer.ATK = EditorGUILayout.FloatField(
            "ATK",
            selectedPlayer.ATK
        );

        selectedPlayer.DEF = EditorGUILayout.FloatField(
            "DEF",
            selectedPlayer.DEF
        );

        GUILayout.Space(10);
    }

    private void DrawGrowthValues()
    {
        EditorGUILayout.LabelField("성장 수치", EditorStyles.boldLabel);

        SerializedObject serializedObject = new SerializedObject(selectedPlayer);

        SerializedProperty hpValue = serializedObject.FindProperty("HPValue");
        SerializedProperty atkValue = serializedObject.FindProperty("ATKValue");

        serializedObject.Update();

        EditorGUILayout.PropertyField(hpValue, true);
        EditorGUILayout.PropertyField(atkValue, true);

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);
    }

    private void DrawInitialSettings()
    {
        EditorGUILayout.LabelField("초기 설정", EditorStyles.boldLabel);

        selectedPlayer.currentStance = (StancType)EditorGUILayout.EnumPopup(
            "기본 태세",
            selectedPlayer.currentStance
        );

        SerializedObject serializedObject = new SerializedObject(selectedPlayer);
        SerializedProperty deckProperty = serializedObject.FindProperty("defaultDeckIndexes");

        serializedObject.Update();

        EditorGUILayout.PropertyField(
            deckProperty,
            new GUIContent("기본 덱"),
            true
        );

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);
    }

    private void DrawAnimation()
    {
        EditorGUILayout.LabelField("연출", EditorStyles.boldLabel);

        selectedPlayer.animationController =
            (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                "Animator Controller",
                selectedPlayer.animationController,
                typeof(RuntimeAnimatorController),
                false
            );

        GUILayout.Space(10);
    }

    private void RefreshPlayers()
    {
        playerList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:PlayerData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            PlayerData player =
                AssetDatabase.LoadAssetAtPath<PlayerData>(path);

            if (player != null)
                playerList.Add(player);
        }

        playerList.Sort((a, b) => a.IDNum.CompareTo(b.IDNum));

        if (selectedPlayer != null && !playerList.Contains(selectedPlayer))
            selectedPlayer = null;
    }

    private void SavePlayer()
    {
        if (selectedPlayer == null)
            return;

        EditorUtility.SetDirty(selectedPlayer);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"[TFUDataEditor] PlayerData 저장 완료: {selectedPlayer.CharacterName}"
        );
    }

    private void ReloadPlayer()
    {
        if (selectedPlayer == null)
            return;

        string path = AssetDatabase.GetAssetPath(selectedPlayer);

        AssetDatabase.ImportAsset(
            path,
            ImportAssetOptions.ForceUpdate
        );

        selectedPlayer =
            AssetDatabase.LoadAssetAtPath<PlayerData>(path);

        Repaint();
    }

    private void DrawEmptyTab(string name)
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label(
            $"{name} 준비 중",
            EditorStyles.centeredGreyMiniLabel
        );
        GUILayout.FlexibleSpace();
    }
}
#endif