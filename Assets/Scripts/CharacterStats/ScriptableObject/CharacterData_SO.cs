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

    public int angerNum;//������:��10Ϊһ������4��
    public float dirtyNum;//��Ⱦ��
    public int bloodNum;//Ѫ����������ż��������ʼΪ0��
    public bool isDead;//�Ƿ�����

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
    //�����ж�
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
