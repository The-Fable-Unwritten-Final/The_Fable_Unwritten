using System.Collections.Generic;
using System.IO;

public static class CardCSVParser
{
    public static List<CardData> Parse(string path)
    {
        var list = new List<CardData>();
        var lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');

            var data = new CardData
            {
                index = int.Parse(tokens[0]),
                cost = int.Parse(tokens[1]),
                illustration = tokens[2],
                name = tokens[3],
                text = tokens[4],
                type = int.Parse(tokens[5]),
                classIndex = int.Parse(tokens[6]),
                cardImage = tokens[7],
                damage = float.Parse(tokens[8]),
                draw = int.Parse(tokens[9]),
                redraw = int.Parse(tokens[10]),
                atkBuff = float.Parse(tokens[11]),
                defBuff = float.Parse(tokens[12]),
                selfDamage = float.Parse(tokens[13]),
                block = tokens[14] == "1",
                blind = tokens[15] == "1",
                stun = tokens[16] == "1",
                target = tokens[17],
                characterStance = tokens[18],
                characterIgnite = tokens[19] == "1"
            };
            list.Add(data);
        }
        return list;
    }
}
