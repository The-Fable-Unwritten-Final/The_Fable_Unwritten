using System.Collections.Generic;

[System.Serializable]
public class EnemyParsed
{
    public int id;
    public string enemyName;
    public string art;
    public int hp;
    public int exp;

    public List<int> loots;

    public string attackEffect;
    public string allyEffect;

    public int[] skillIndices = new int[5];
    public float[] skillDamages = new float[5];
    public float[] skillPercents = new float[5];

    public float topPercentage;
    public float middlePercentage;
    public float bottomPercentage;

    public float atkBuff;
    public float defBuff;
    public int buffTime;

    public bool block;
    public bool blind;
    public bool stun;

    public string note;

}
