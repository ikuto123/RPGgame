using UnityEngine;

public class BossStatus : EnemyStatus
{

    private void Start()
    {
        HP = enemyData.MaxHP;
        currentHP = enemyData.MaxHP;
        MP = enemyData.MaxMP;
        currentMP = enemyData.MaxMP;
        
        attack = enemyData.Attack;
        defense = enemyData.Defense;
        
        
    }
}
