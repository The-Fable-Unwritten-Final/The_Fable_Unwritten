#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

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

    private List<EnemyData> enemyList = new();
    private EnemyData selectedEnemy;

    private Vector2 enemyLeftScroll;
    private Vector2 enemyRightScroll;

    private List<EnemyAct> enemyActList = new();
    private EnemyAct selectedEnemyAct;

    private Vector2 enemySkillLeftScroll;
    private Vector2 enemySkillRightScroll;

    private List<CardJsonData> cardList = new();
    private CardJsonData selectedCard;

    private Vector2 cardLeftScroll;
    private Vector2 cardRightScroll;

    private string playerSearch = "";
    private string cardSearch = "";
    private string enemySearch = "";
    private string enemySkillSearch = "";

    [MenuItem("TFU/Data Editor")]
    public static void Open()
    {
        GetWindow<TFUDataEditorWindow>("TFU Data Editor");
    }

    private void OnEnable()
    {
        RefreshPlayers();
        RefreshEnemies();
        RefreshEnemySkills();
        RefreshCards();
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
                DrawCardTab();
                break;

            case Tab.Enemy:
                DrawEnemyTab();
                break;

            case Tab.EnemySkill:
                DrawEnemySkillTab();
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

        playerSearch = EditorGUILayout.TextField("검색", playerSearch);

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        foreach (var player in playerList)
        {
            if (player == null)
                continue;

            if (!IsPlayerSearchMatch(player))
                continue;

            string label = $"{player.IDNum} - {player.CharacterName}";

            bool selected = selectedPlayer == player;

            if (GUILayout.Toggle(selected, label, "Button"))
                selectedPlayer = player;
        }


        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private bool IsPlayerSearchMatch(PlayerData player)
    {
        if (string.IsNullOrWhiteSpace(playerSearch))
            return true;

        string search = playerSearch.Trim();

        return player.IDNum.ToString().Contains(search) ||
               (!string.IsNullOrEmpty(player.CharacterName) &&
                player.CharacterName.ToLower().Contains(search.ToLower()));
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
    private void DrawEnemyTab()
    {
        EditorGUILayout.BeginHorizontal();

        DrawEnemyList();
        DrawEnemyInspector();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEnemyList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220));

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("적", EditorStyles.boldLabel);

        if (GUILayout.Button("새로고침", GUILayout.Width(70)))
            RefreshEnemies();

        EditorGUILayout.EndHorizontal();

        enemySearch = EditorGUILayout.TextField("검색", enemySearch);

        enemyLeftScroll = EditorGUILayout.BeginScrollView(enemyLeftScroll);

        foreach (var enemy in enemyList)
        {
            if (enemy == null)
                continue;

            if (!IsEnemySearchMatch(enemy))
                continue;

            string label = $"{enemy.IDNum} - {enemy.EnemyName}";
            bool selected = selectedEnemy == enemy;

            if (GUILayout.Toggle(selected, label, "Button"))
                selectedEnemy = enemy;
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }
    private bool IsEnemySearchMatch(EnemyData enemy)
    {
        if (string.IsNullOrWhiteSpace(enemySearch))
            return true;

        string search = enemySearch.Trim();

        return enemy.IDNum.ToString().Contains(search) ||
               (!string.IsNullOrEmpty(enemy.EnemyName) &&
                enemy.EnemyName.ToLower().Contains(search.ToLower()));
    }

    private void DrawEnemyInspector()
    {
        EditorGUILayout.BeginVertical();

        if (selectedEnemy == null)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                "왼쪽에서 적을 선택하세요.",
                EditorStyles.centeredGreyMiniLabel
            );
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
            return;
        }

        enemyRightScroll = EditorGUILayout.BeginScrollView(enemyRightScroll);

        EditorGUILayout.LabelField(
            $"{selectedEnemy.EnemyName} Enemy Data",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        SerializedObject so = new SerializedObject(selectedEnemy);

        so.Update();

        DrawEnemyBasicInfo(so);
        DrawEnemyStats(so);
        DrawEnemySkills(so);
        DrawEnemyAnchor(so);
        DrawEnemyStance(so);
        DrawEnemyVisual(so);
        DrawEnemyLoot(so);

        so.ApplyModifiedProperties();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("저장", GUILayout.Height(30)))
            SaveEnemy();

        if (GUILayout.Button("되돌리기", GUILayout.Height(30)))
            ReloadEnemy();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawEnemyBasicInfo(SerializedObject so)
    {
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("idNum"),
            new GUIContent("ID")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("enemyName"),
            new GUIContent("이름")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("type"),
            new GUIContent("적 타입")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("exp"),
            new GUIContent("EXP")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("note"),
            new GUIContent("메모")
        );

        GUILayout.Space(10);
    }

    private void DrawEnemyStats(SerializedObject so)
    {
        EditorGUILayout.LabelField("기본 스탯", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("maxHP"),
            new GUIContent("Max HP")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("aTKValue"),
            new GUIContent("ATK")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("dEFValue"),
            new GUIContent("DEF")
        );

        GUILayout.Space(10);
    }

    private void DrawEnemySkills(SerializedObject so)
    {
        EditorGUILayout.LabelField("스킬", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("skillList"),
            new GUIContent("스킬 목록"),
            true
        );

        GUILayout.Space(10);
    }

    private void DrawEnemyAnchor(SerializedObject so)
    {
        EditorGUILayout.LabelField("Anchor", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("anchorData"),
            new GUIContent("Anchor Data"),
            true
        );

        GUILayout.Space(10);
    }

    private void DrawEnemyStance(SerializedObject so)
    {
        EditorGUILayout.LabelField("태세", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("TopStance"),
            new GUIContent("Top")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("MiddleStance"),
            new GUIContent("Middle")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("BottomStance"),
            new GUIContent("Bottom")
        );

        GUILayout.Space(10);
    }

    private void DrawEnemyVisual(SerializedObject so)
    {
        EditorGUILayout.LabelField("연출", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("illust"),
            new GUIContent("일러스트")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("animationController"),
            new GUIContent("Animator Controller")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("AttackSkillEffect"),
            new GUIContent("공격 기본 이펙트")
        );

        EditorGUILayout.PropertyField(
            so.FindProperty("AllySkillEffect"),
            new GUIContent("아군 기본 이펙트")
        );

        GUILayout.Space(10);
    }

    private void DrawEnemyLoot(SerializedObject so)
    {
        EditorGUILayout.LabelField("드롭", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            so.FindProperty("loot"),
            new GUIContent("Loot"),
            true
        );

        GUILayout.Space(10);
    }
    private void RefreshEnemies()
    {
        enemyList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:EnemyData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (enemy != null)
                enemyList.Add(enemy);
        }

        enemyList.Sort((a, b) => a.IDNum.CompareTo(b.IDNum));

        if (selectedEnemy != null && !enemyList.Contains(selectedEnemy))
            selectedEnemy = null;
    }

    private void SaveEnemy()
    {
        if (selectedEnemy == null)
            return;

        EditorUtility.SetDirty(selectedEnemy);
        AssetDatabase.SaveAssets();

        Debug.Log($"[TFUDataEditor] EnemyData 저장 완료: {selectedEnemy.EnemyName}");
    }

    private void ReloadEnemy()
    {
        if (selectedEnemy == null)
            return;

        string path = AssetDatabase.GetAssetPath(selectedEnemy);

        AssetDatabase.ImportAsset(
            path,
            ImportAssetOptions.ForceUpdate
        );

        selectedEnemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

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

    private void RefreshEnemySkills()
    {
        enemyActList.Clear();

        TextAsset csv = Resources.Load<TextAsset>("ExternalFiles/EnemyActV2");

        if (csv == null)
        {
            Debug.LogError("[TFUDataEditor] EnemyActV2를 찾을 수 없습니다.");
            return;
        }

        enemyActList = EnemyActCSVParser.ParseEnemyActV2(csv.text);

        enemyActList.Sort((a, b) => a.index.CompareTo(b.index));

        if (selectedEnemyAct != null && !enemyActList.Contains(selectedEnemyAct))
            selectedEnemyAct = null;
    }

    private void DrawEnemySkillTab()
    {
        EditorGUILayout.BeginHorizontal();

        DrawEnemySkillList();
        DrawEnemySkillInspector();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEnemySkillList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220));

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("적 스킬", EditorStyles.boldLabel);

        if (GUILayout.Button("새로고침", GUILayout.Width(70)))
            RefreshEnemySkills();

        EditorGUILayout.EndHorizontal();

        enemySkillSearch = EditorGUILayout.TextField("검색", enemySkillSearch);

        enemySkillLeftScroll =
            EditorGUILayout.BeginScrollView(enemySkillLeftScroll);

        foreach (var act in enemyActList)
        {
            if (act == null)
                continue;

            if (!IsEnemySkillSearchMatch(act))
                continue;

            bool selected = selectedEnemyAct == act;

            if (GUILayout.Toggle(
                selected,
                $"Skill {act.index}",
                "Button"))
            {
                selectedEnemyAct = act;
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private bool IsEnemySkillSearchMatch(EnemyAct act)
    {
        if (string.IsNullOrWhiteSpace(enemySkillSearch))
            return true;

        string search = enemySkillSearch.Trim();

        return act.index.ToString().Contains(search);
    }

    private void DrawEnemySkillInspector()
    {
        EditorGUILayout.BeginVertical();

        if (selectedEnemyAct == null)
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                "왼쪽에서 적 스킬을 선택하세요.",
                EditorStyles.centeredGreyMiniLabel
            );

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
            return;
        }

        enemySkillRightScroll =
            EditorGUILayout.BeginScrollView(enemySkillRightScroll);

        EditorGUILayout.LabelField(
            $"Enemy Skill {selectedEnemyAct.index}",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        DrawEnemySkillBasic();
        DrawEnemySkillTargets();
        DrawEnemySkillEffect1();
        DrawEnemySkillEffect2();
        DrawEnemySkillCondition();
        DrawEnemySkillModifier();
        DrawEnemySkillSpecial();
        DrawEnemySkillVisual();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("저장", GUILayout.Height(30)))
            SaveEnemySkills();

        if (GUILayout.Button("되돌리기", GUILayout.Height(30)))
            ReloadEnemySkills();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawEnemySkillBasic()
    {
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

        selectedEnemyAct.index =
            EditorGUILayout.IntField("Index", selectedEnemyAct.index);

        GUILayout.Space(10);
    }

    private void DrawEnemySkillTargets()
    {
        EditorGUILayout.LabelField("대상", EditorStyles.boldLabel);

        selectedEnemyAct.targetType =
            (TargetType)EditorGUILayout.EnumPopup(
                "Target Type",
                selectedEnemyAct.targetType
            );

        selectedEnemyAct.targetNum =
            EditorGUILayout.IntField(
                "Target Num",
                selectedEnemyAct.targetNum
            );

        selectedEnemyAct.target_front =
            EditorGUILayout.Toggle(
                "Front",
                selectedEnemyAct.target_front
            );

        selectedEnemyAct.target_center =
            EditorGUILayout.Toggle(
                "Center",
                selectedEnemyAct.target_center
            );

        selectedEnemyAct.target_back =
            EditorGUILayout.Toggle(
                "Back",
                selectedEnemyAct.target_back
            );

        GUILayout.Space(10);
    }

    private void DrawEnemySkillEffect1()
    {
        EditorGUILayout.LabelField("Effect 1", EditorStyles.boldLabel);

        selectedEnemyAct.arg_effect1 =
            (EnemyEffectType)EditorGUILayout.EnumPopup(
                "Effect",
                selectedEnemyAct.arg_effect1
            );

        selectedEnemyAct.arg1 =
            EditorGUILayout.IntField(
                "Value",
                selectedEnemyAct.arg1
            );

        selectedEnemyAct.arg_target1 =
            (EnemyEffectTarget)EditorGUILayout.EnumPopup(
                "Target",
                selectedEnemyAct.arg_target1
            );

        GUILayout.Space(10);
    }

    private void DrawEnemySkillEffect2()
    {
        EditorGUILayout.LabelField("Effect 2", EditorStyles.boldLabel);

        selectedEnemyAct.arg_effect2 =
            (EnemyEffectType)EditorGUILayout.EnumPopup(
                "Effect",
                selectedEnemyAct.arg_effect2
            );

        selectedEnemyAct.arg2 =
            EditorGUILayout.IntField(
                "Value",
                selectedEnemyAct.arg2
            );

        selectedEnemyAct.arg_target2 =
            (EnemyEffectTarget)EditorGUILayout.EnumPopup(
                "Target",
                selectedEnemyAct.arg_target2
            );

        GUILayout.Space(10);
    }
    private void DrawEnemySkillCondition()
    {
        EditorGUILayout.LabelField("사용 조건", EditorStyles.boldLabel);

        selectedEnemyAct.useCondition =
            (EnemyUseCondition)EditorGUILayout.EnumPopup(
                "Condition",
                selectedEnemyAct.useCondition
            );

        selectedEnemyAct.useConditionValue =
            EditorGUILayout.IntField(
                "Value",
                selectedEnemyAct.useConditionValue
            );

        GUILayout.Space(10);
    }
    private void DrawEnemySkillModifier()
    {
        EditorGUILayout.LabelField("수치 보정", EditorStyles.boldLabel);

        selectedEnemyAct.valueModifier =
            (EnemyValueModifier)EditorGUILayout.EnumPopup(
                "Modifier",
                selectedEnemyAct.valueModifier
            );

        selectedEnemyAct.modifierStatus =
            (EnemyEffectType)EditorGUILayout.EnumPopup(
                "Status",
                selectedEnemyAct.modifierStatus
            );

        selectedEnemyAct.modifierValue =
            EditorGUILayout.IntField(
                "Value",
                selectedEnemyAct.modifierValue
            );

        selectedEnemyAct.modifierConditionValue =
            EditorGUILayout.IntField(
                "Condition Value",
                selectedEnemyAct.modifierConditionValue
            );

        GUILayout.Space(10);
    }
    private void DrawEnemySkillSpecial()
    {
        EditorGUILayout.LabelField("특수 로직", EditorStyles.boldLabel);

        selectedEnemyAct.specialLogic =
            (EnemySpecialLogic)EditorGUILayout.EnumPopup(
                "Special Logic",
                selectedEnemyAct.specialLogic
            );

        GUILayout.Space(10);
    }

    private void DrawEnemySkillVisual()
    {
        EditorGUILayout.LabelField("연출", EditorStyles.boldLabel);

        selectedEnemyAct.skilleffect =
            EditorGUILayout.TextField(
                "Skill Effect",
                selectedEnemyAct.skilleffect
            );

        GUILayout.Space(10);
    }

    private void SaveEnemySkills()
    {
        string[] guids = AssetDatabase.FindAssets("EnemyActV2 t:TextAsset");

        if (guids.Length == 0)
        {
            Debug.LogError("[TFUDataEditor] EnemyActV2.csv를 찾을 수 없습니다.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "index,target_type,target_num,target_front,target_center,target_back," +
            "arg_effect1,arg1,arg_target1,arg_effect2,arg2,arg_target2," +
            "use_condition,use_condition_value,value_modifier,modifier_status," +
            "modifier_value,modifier_condition_value,special_logic,effect"
        );

        foreach (var act in enemyActList)
            sb.AppendLine(BuildEnemySkillCsvLine(act));

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));

        AssetDatabase.Refresh();

        Debug.Log($"[TFUDataEditor] EnemyActV2 저장 완료: {path}");
    }

    private string BuildEnemySkillCsvLine(EnemyAct act)
    {
        return string.Join(",",
            act.index,
            (int)act.targetType,
            act.targetNum,
            act.target_front ? 1 : 0,
            act.target_center ? 1 : 0,
            act.target_back ? 1 : 0,

            act.arg_effect1.ToString(),
            act.arg1,
            act.arg_target1.ToString(),

            act.arg_effect2.ToString(),
            act.arg2,
            act.arg_target2.ToString(),

            act.useCondition.ToString(),
            act.useConditionValue,

            act.valueModifier.ToString(),
            act.modifierStatus.ToString(),
            act.modifierValue,
            act.modifierConditionValue,

            act.specialLogic.ToString(),

            act.skilleffect ?? ""
        );
    }

    private void ReloadEnemySkills()
    {
        int selectedIndex =
            selectedEnemyAct != null ? selectedEnemyAct.index : -1;

        RefreshEnemySkills();

        if (selectedIndex < 0)
            return;

        selectedEnemyAct =
            enemyActList.Find(x => x.index == selectedIndex);
    }

    private void RefreshCards()
    {
        int selectedIndex = selectedCard != null ? selectedCard.index : -1;

        cardList = CardJsonLoader.Load("ExternalFiles/CardDataFinal");

        if (cardList == null)
            cardList = new List<CardJsonData>();

        cardList.Sort((a, b) => a.index.CompareTo(b.index));

        selectedCard =
            selectedIndex >= 0
            ? cardList.Find(x => x.index == selectedIndex)
            : null;
    }

    private void DrawCardTab()
    {
        EditorGUILayout.BeginHorizontal();

        DrawCardList();
        DrawCardInspector();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCardList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(240));

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("카드", EditorStyles.boldLabel);

        if (GUILayout.Button("새로고침", GUILayout.Width(70)))
            RefreshCards();

        EditorGUILayout.EndHorizontal();

        cardSearch = EditorGUILayout.TextField("검색", cardSearch);

        cardLeftScroll = EditorGUILayout.BeginScrollView(cardLeftScroll);

        foreach (var card in cardList)
        {
            if (card == null)
                continue;

            if (!IsCardSearchMatch(card))
                continue;

            string label = $"{card.index} - {card.name}";
            bool selected = selectedCard == card;

            if (GUILayout.Toggle(selected, label, "Button"))
                selectedCard = card;
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private bool IsCardSearchMatch(CardJsonData card)
    {
        if (string.IsNullOrWhiteSpace(cardSearch))
            return true;

        string search = cardSearch.Trim();

        return card.index.ToString().Contains(search) ||
               (!string.IsNullOrEmpty(card.name) &&
                card.name.ToLower().Contains(search.ToLower()));
    }
    private void DrawCardInspector()
    {
        EditorGUILayout.BeginVertical();

        if (selectedCard == null)
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                "왼쪽에서 카드를 선택하세요.",
                EditorStyles.centeredGreyMiniLabel
            );

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
            return;
        }

        cardRightScroll = EditorGUILayout.BeginScrollView(cardRightScroll);

        EditorGUILayout.LabelField(
            $"Card {selectedCard.index}",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        DrawCardBasic();
        DrawCardTarget();
        DrawCardVisual();
        DrawCardEvolution();
        DrawCardKeywords();
        DrawCardEffects();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("저장", GUILayout.Height(30)))
            SaveCards();

        if (GUILayout.Button("되돌리기", GUILayout.Height(30)))
            RefreshCards();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawCardBasic()
    {
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

        selectedCard.index =
            EditorGUILayout.IntField("Index", selectedCard.index);

        selectedCard.cost =
            EditorGUILayout.IntField("Cost", selectedCard.cost);

        selectedCard.name =
            EditorGUILayout.TextField("Name", selectedCard.name);

        selectedCard.text =
            EditorGUILayout.TextField("Text", selectedCard.text);

        selectedCard.type =
            EditorGUILayout.IntField("Type", selectedCard.type);

        selectedCard.@class =
            EditorGUILayout.IntField("Class", selectedCard.@class);

        selectedCard.note =
            EditorGUILayout.TextField("Note", selectedCard.note);

        selectedCard.flavortext =
            EditorGUILayout.TextField("Flavor Text", selectedCard.flavortext);

        GUILayout.Space(10);
    }
    private void DrawCardTarget()
    {
        EditorGUILayout.LabelField("대상", EditorStyles.boldLabel);

        selectedCard.target_type =
            EditorGUILayout.IntField(
                "Target Type",
                selectedCard.target_type
            );

        selectedCard.target_num =
            EditorGUILayout.IntField(
                "Target Num",
                selectedCard.target_num
            );

        GUILayout.Space(10);
    }

    private void DrawCardVisual()
    {
        EditorGUILayout.LabelField("연출", EditorStyles.boldLabel);

        selectedCard.illustration =
            EditorGUILayout.TextField(
                "Illustration",
                selectedCard.illustration
            );

        selectedCard.cardframe =
            EditorGUILayout.TextField(
                "Card Frame",
                selectedCard.cardframe
            );

        selectedCard.skilleffect =
            EditorGUILayout.TextField(
                "Skill Effect",
                selectedCard.skilleffect
            );

        GUILayout.Space(10);
    }

    private void DrawCardEvolution()
    {
        EditorGUILayout.LabelField("진화", EditorStyles.boldLabel);

        selectedCard.evolveCount =
            EditorGUILayout.IntField(
                "Evolve Count",
                selectedCard.evolveCount
            );

        selectedCard.evolveTarget =
            EditorGUILayout.IntField(
                "Evolve Target",
                selectedCard.evolveTarget
            );

        GUILayout.Space(10);
    }

    private void DrawCardKeywords()
    {
        EditorGUILayout.LabelField("키워드", EditorStyles.boldLabel);

        if (selectedCard.keywords == null)
            selectedCard.keywords = new List<string>();

        for (int i = 0; i < selectedCard.keywords.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            selectedCard.keywords[i] =
                EditorGUILayout.TextField(
                    $"Keyword {i + 1}",
                    selectedCard.keywords[i]
                );

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                selectedCard.keywords.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Keyword"))
            selectedCard.keywords.Add("");

        selectedCard.switchType =
            EditorGUILayout.TextField(
                "Switch Type",
                selectedCard.switchType
            );

        GUILayout.Space(10);
    }

    private void DrawCardEffects()
    {
        EditorGUILayout.LabelField("효과", EditorStyles.boldLabel);

        if (selectedCard.effects == null)
            selectedCard.effects = new List<CardEffect>();

        for (int i = 0; i < selectedCard.effects.Count; i++)
        {
            CardEffect effect = selectedCard.effects[i];

            if (effect == null)
            {
                effect = new CardEffect();
                selectedCard.effects[i] = effect;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Effect {i + 1}",
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                selectedCard.effects.RemoveAt(i);
                i--;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }

            EditorGUILayout.EndHorizontal();

            DrawCardEffect(effect);

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }

        if (GUILayout.Button("+ Effect"))
            selectedCard.effects.Add(new CardEffect());

        GUILayout.Space(10);
    }

    private void DrawCardEffect(CardEffect effect)
    {
        effect.type =
            EditorGUILayout.TextField(
                "Type",
                effect.type
            );

        effect.value =
            EditorGUILayout.IntField(
                "Value",
                effect.value
            );

        effect.duration =
            EditorGUILayout.IntField(
                "Duration",
                effect.duration
            );

        effect.target =
            EditorGUILayout.IntField(
                "Target",
                effect.target
            );

        effect.owner =
            EditorGUILayout.TextField(
                "Owner",
                effect.owner
            );

        if (effect.condition != null)
            DrawCardCondition(effect);

        if (effect.result != null)
            DrawCardResult(effect);

        EditorGUILayout.BeginHorizontal();

        if (effect.condition == null)
        {
            if (GUILayout.Button("+ Condition"))
                effect.condition = new EffectCondition();
        }
        else
        {
            if (GUILayout.Button("- Condition"))
                effect.condition = null;
        }

        if (effect.result == null)
        {
            if (GUILayout.Button("+ Result"))
                effect.result = new ResultEffect();
        }
        else
        {
            if (GUILayout.Button("- Result"))
                effect.result = null;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCardCondition(CardEffect effect)
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField(
            "Condition",
            EditorStyles.boldLabel
        );

        effect.condition.trigger =
            EditorGUILayout.TextField(
                "Trigger",
                effect.condition.trigger
            );

        if (effect.condition.value == null)
            effect.condition.value = new List<string>();

        for (int i = 0; i < effect.condition.value.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            effect.condition.value[i] =
                EditorGUILayout.TextField(
                    $"Value {i + 1}",
                    effect.condition.value[i]
                );

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                effect.condition.value.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Condition Value"))
            effect.condition.value.Add("");
    }

    private void DrawCardResult(CardEffect effect)
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField(
            "Result",
            EditorStyles.boldLabel
        );

        effect.result.type =
            EditorGUILayout.TextField(
                "Type",
                effect.result.type
            );

        effect.result.value =
            EditorGUILayout.IntField(
                "Value",
                effect.result.value
            );

        effect.result.duration =
            EditorGUILayout.IntField(
                "Duration",
                effect.result.duration
            );

        effect.result.target =
            EditorGUILayout.IntField(
                "Target",
                effect.result.target
            );
    }

    private void SaveCards()
    {
        string[] guids =
            AssetDatabase.FindAssets("CardDataFinal t:TextAsset");

        if (guids.Length == 0)
        {
            Debug.LogError(
                "[TFUDataEditor] CardDataFinal.json을 찾을 수 없습니다."
            );
            return;
        }

        string path =
            AssetDatabase.GUIDToAssetPath(guids[0]);

        string json =
            JsonUtilityWrapper.ToJsonList(cardList);

        File.WriteAllText(
            path,
            json,
            new UTF8Encoding(false)
        );

        AssetDatabase.Refresh();

        Debug.Log(
            $"[TFUDataEditor] CardDataFinal 저장 완료: {path}"
        );
    }
}
#endif