using System.Collections.Generic;

public enum StageTheme
{
    Tutorial = 0, // 1스테이지 고정
    Wisdom = 1,
    Love = 2,
    Courage = 3,
    TrueEnd = 4   // 5스테이지 고정
}

public class StageData
{
    public int stageIndex;                        // 스테이지 인덱스
    public int columnCount;                       // 스테이지 보유 열 개수
    public List<List<GraphNode>> columns = new(); // 스테이지 보유 열(노드리스트) 정보 리스트
}
