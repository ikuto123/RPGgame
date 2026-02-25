using UnityEngine;
using System;

public abstract class CharacterStatus : MonoBehaviour
{
    [SerializeField] protected int HP;
    [SerializeField] protected int MP;
    [SerializeField] protected int attack;
    [SerializeField] protected int defense;
    [SerializeField] protected int speed;
    
    [SerializeField] protected string Id;

    protected int currentHP;
    protected int currentMP;
    public int Hp => HP;
    public int Mp => MP;
    public int Attack => attack;
    public int Defense => defense;
    
    public Action OnDeath;
    
    public virtual void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(damage - defense, 0);
        currentHP = Mathf.Max(currentHP - damageTaken, 0);
        if (currentHP <= 0)
        {
            OnDeath?.Invoke();//死んだときの処理
        }
    }
    
    public virtual bool UseMP(int amount)
    {
        if(amount > currentMP) return false;
        
        currentMP -= amount;
        return true;
    }
    
    public virtual void RecoverMP(int amount)
    {
        currentMP = Mathf.Min(currentMP + amount, MP);
    }
    
    public virtual void RecoverHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, HP);
    }
    
}
