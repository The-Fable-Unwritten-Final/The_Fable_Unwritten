using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어들의 데이터 및 덱을 관리하는 전용 매니저
/// </summary>
public class PlayerManager : MonoSingleton<PlayerManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private Dictionary<CharacterClass, PlayerData> playerDataMap = new();
    public Dictionary<CharacterClass, PlayerData> activePlayers = new();
    private List<CardModel> currentCardPool = new();

    /// <summary>
    /// 카드 로딩 후 플레이어들의 덱을 설정하는 초기화 메서드
    /// </summary>
    public void InitializePlayers(List<PlayerData> allPlayers, List<CardModel> cardPool)
    {
        playerDataMap.Clear();
        currentCardPool = cardPool;

        foreach (var data in allPlayers)
        {
            // 현재 덱 인덱스가 없으면 기본 덱 인덱스를 복사
            if (data.currentDeckIndexes == null || data.currentDeckIndexes.Count == 0)
            {
                data.currentDeckIndexes = new List<int>(data.defaultDeckIndexes);
            }

            // 인덱스를 기반으로 덱 구성
            data.LoadDeckFromIndexes(cardPool);

            // 등록
            playerDataMap[data.CharacterClass] = data;
        }
    }

    /// <summary>
    /// 게임 도중 새 플레이어를 하나씩 추가 (ex. 이야기 흐름 따라 동료가 합류)
    /// </summary>
    public void AddPlayerDuringGame(PlayerData newPlayerData, List<CardModel> cardPool)
    {
        var character = newPlayerData.CharacterClass;

        // 기존에 등록된 플레이어면 무시
        if (activePlayers.ContainsKey(character))
        {
            Debug.LogWarning($"이미 등록된 플레이어: {character}");
            return;
        }

        // 덱 초기화가 되어있지 않은 경우만 처리
        if (newPlayerData.currentDeckIndexes == null || newPlayerData.currentDeckIndexes.Count == 0)
        {
            newPlayerData.currentDeckIndexes = new List<int>(newPlayerData.defaultDeckIndexes);
            newPlayerData.LoadDeckFromIndexes(cardPool);
        }

        playerDataMap[character] = newPlayerData;
        activePlayers[character] = newPlayerData;
    }

    /// <summary>
    /// 게임 시작 시 초기화용: 모든 플레이어를 한 번에 등록
    /// (프리팹 생성은 외부에서 수행)
    /// </summary>
    public void RegisterAndSetupPlayers(List<PlayerData> allPlayerDatas, List<CardModel> cardPool)
    {
        InitializePlayers(allPlayerDatas, cardPool);
        activePlayers.Clear();
    }

    /// <summary>
    /// 현재 등록된 모든 플레이어 반환 (전투 컨트롤러 전달용)
    /// </summary>
    public List<IStatusReceiver> GetAllPlayers()
    {
        var result = new List<IStatusReceiver>();
        foreach (var kvp in activePlayers)
        {
            var dummy = GameObject.FindObjectsOfType<PlayerController>(); // 추정 불가능하므로 비워둠
            Debug.LogWarning($"[PlayerManager] GetAllPlayers는 PlayerController를 관리하지 않으므로 직접 BattleScene에서 생성하세요.");
        }
        return result;
    }

    public List<PlayerData> GetAllActivePlayerData()
    {
        return new List<PlayerData>(activePlayers.Values);
    }

    /// <summary>
    /// 특정 캐릭터의 덱(CardModel 리스트) 가져오기
    /// </summary>
    public List<CardModel> GetCurrentDeck(CharacterClass character)
    {
        if (playerDataMap.TryGetValue(character, out var data))
            return data.currentDeck;

        return new List<CardModel>();
    }

    /// <summary>
    /// 플레이어 데이터를 가져오기
    /// </summary>
    public PlayerData GetPlayerData(CharacterClass character)
    {
        if (playerDataMap.TryGetValue(character, out var data))
            return data;

        return null;
    }

    /// <summary>
    /// 전투 시작 시 실제 캐릭터에게 덱을 넘겨줄 때 사용
    /// </summary>
    public void AssignDeckToPlayer(IStatusReceiver receiver)
    {
        var data = GetPlayerData(receiver.ChClass);
        if (data != null)
        {
            receiver.Deck.Initialize(data.currentDeck);
        }
    }

    /// <summary>
    /// 덱 저장 전, currentDeckIndexes 최신화
    /// </summary>
    public void SyncDeckIndexes()
    {
        foreach (var kvp in playerDataMap)
        {
            kvp.Value.UpdateCurrentDeckIndexes();
        }
    }

}

public enum GameStartType
{
    New,
    Respawn
}
