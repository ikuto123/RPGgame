using UnityEngine;

//キャラクターステータスも見ろ!!!
public class PlayerStatus : CharacterStatus
{
    [SerializeField] private int level;
    [SerializeField] private int experience;
    [SerializeField] private int coin;

    public int Level => level;
    public int Experience => experience;
    public int Coin => coin;
    
    [SerializeField] PlayerData playerData;
    
    private void Start()
    {
        HP = playerData.MaxHP;
        MP = playerData.MaxMP;
        attack = playerData.MaxAttack;
        defense = playerData.MaxDefense;
        level = 1;
        coin = 100;
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
    public void UpCoin(int amount) => coin += Mathf.Min(amount,99999);

    public void DownCoin(int amount) => coin = Mathf.Max(coin - amount, 0);
    
    public void GainExperience(int amount)
    {
        experience += amount;
        if (experience >= GetExperienceForNextLevel())
        {
            experience -= GetExperienceForNextLevel();
            level += Mathf.Min(level + 1 , 99);
        }
    }

    private int GetExperienceForNextLevel()
    {
        return 0;
    }
}