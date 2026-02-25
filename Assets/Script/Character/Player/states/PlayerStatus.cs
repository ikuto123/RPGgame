using UnityEngine;

public class PlayerStatus : CharacterStatus
{
    [SerializeField] private int level;
    [SerializeField] private int experience;

    public int Level => level;
    public int Experience => experience;
    
    [SerializeField] PlayerData playerData;
    
    private void Start()
    {
        HP = playerData.MaxHP;
        MP = playerData.MaxMP;
        attack = playerData.MaxAttack;
        defense = playerData.MaxDefense;
        level = 1;
    }
    
    private void status(int lvl)
    {
        
        
    }

    private (int,int) Status_Expression(int minStatus, int maxStatus , int maxLevel)
    {
        int minLevel = 1;
        int a = (maxStatus - minStatus) / (maxLevel - minLevel);
        int b = maxLevel - a * maxStatus;
        
        return (a , b);
    }

    
    

}