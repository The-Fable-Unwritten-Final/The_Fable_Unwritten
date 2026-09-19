public struct EnemyMechanicDisplayData
{
    public string type;
    public int value;
    public object[] tooltipArgs;

    public bool hideNumber;
    public int priority;

    public EnemyMechanicDisplayData(string type, int value = 0, bool hideNumber = false, int priority = 50, params object[] tooltipArgs)
    {
        this.type = type;
        this.value = value;
        this.hideNumber = hideNumber;
        this.priority = priority;
        this.tooltipArgs = tooltipArgs;
    }
}