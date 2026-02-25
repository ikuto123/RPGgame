using UnityEngine;

public class EnemyStatus : CharacterStatus
{
    [SerializeField] protected EnemyData enemyData;

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
