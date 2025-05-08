public static class DialogueTriggerParser
{
    public static string ParseNoteToKey(string note)
    {
        if (note.Contains("시작 전"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"StageStart:{stage}";
        }
        if (note.Contains("전투 노드 클릭"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(note, "\\d+번째");
            var node = match.Value.Replace("번째", "");
            var stage = System.Text.RegularExpressions.Regex.Match(note, "스테이지 (\\d+)").Groups[1].Value;
            return $"Battle:{stage}_{node}";
        }
        if (note.Contains("엘리트 전투 후"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"EliteEnd:{stage}";
        }
        if (note.Contains("보스 전투 승리 후"))
        {
            var stage = System.Text.RegularExpressions.Regex.Match(note, "\\d+").Value;
            return $"Boss:{stage}";
        }
        if (note.StartsWith("야영"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(note, @"야영\s*(\S+)\s*(\d+)");
            if (match.Success)
            {
                string charName = match.Groups[1].Value.Trim();
                string index = match.Groups[2].Value.Trim();
                return $"Camp:{charName}:{index}";
            }
        }

        return null;
    }
}