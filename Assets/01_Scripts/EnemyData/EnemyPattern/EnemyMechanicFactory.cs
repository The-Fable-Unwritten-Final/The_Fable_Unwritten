public static class EnemyMechanicFactory
{
    public static IEnemyMechanic Create(Enemy enemy)
    {
        if (enemy?.enemyData == null)
            return null;

        return enemy.enemyData.IDNum switch
        {
            // todo : 실제 ID에 맞게
            23 => new MarnasMechanic(),
            24 => new ParmanoMechanic(),
            25 => new IzkalMechanic(),
            34 => new GrollyMechanic(),


            _ => null
        };
    }
}