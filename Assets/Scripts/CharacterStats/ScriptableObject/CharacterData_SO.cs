using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Data",menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    public int angerNum;//蓄力槽:满10为一格，最满4格
    public float dirtyNum;//污染条
    public int bloodNum;//血条（海豹玩偶数量，初始为0）
    public bool isDead;//是否死亡

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;
    public float LevelMultiplier
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;
        if (currentExp >= baseExp)
            LevelUp();
    }

    private void LevelUp()
    {
        currentLevel = Mathf.Clamp(currentLevel + 1,0,maxLevel);
        baseExp += (int)(baseExp * LevelMultiplier);

        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;

        Debug.Log("LEVEL UP!" + currentLevel + "Max Health:" + maxHealth);
    }
    //死亡判定
    public bool CheckIsSealDead()
    {
        if(bloodNum < 0)
        {
            isDead = true;
            Debug.Log("die because of being captured!!!!!");
        }
        if(dirtyNum == Const.DIRTY_MAX)
        {
            isDead = true;
            Debug.Log("die because of being poisoned!!!!!");
        }
        return isDead;
    }
}
