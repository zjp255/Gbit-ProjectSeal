using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;

    public AttackData_SO attackData;
    private AttackData_SO baseAttackData;
    private RuntimeAnimatorController baseAnimator;

    [Header("Weapon")]
    public Transform weaponSlot;

    [HideInInspector]
    public bool isCritical;
    public event Action<int> OnAngerChanged;
    public event Action<int> OnBloodChanged;
    public event Action<float> OnDirtyChanged;

    private void Awake()
    {
        if(templateData != null)
            characterData = Instantiate(templateData);

        baseAttackData = Instantiate(attackData);
        baseAnimator = GetComponent<Animator>().runtimeAnimatorController;
    }

    #region Read from Data_SO
    //新的
    public int AngerNum
    {
        get { return characterData != null ? characterData.angerNum : Const.ANGER_MIN; }
        set {
            if (value < Const.ANGER_MIN) characterData.angerNum = Const.ANGER_MIN;
            else if (value > Const.ANGER_MAX) characterData.angerNum = Const.ANGER_MAX;
            else characterData.angerNum = value;
            OnAngerChanged?.Invoke(characterData.angerNum);
        }
    }
    public float DirtyNum
    {
        get { return characterData != null ? characterData.dirtyNum : 0; }
        set {
            if (value < Const.DIRTY_MIN) characterData.dirtyNum = Const.DIRTY_MIN;
            else if (value > Const.DIRTY_MAX) characterData.dirtyNum = Const.DIRTY_MAX;
            else characterData.dirtyNum = value;
            OnDirtyChanged?.Invoke(characterData.dirtyNum); 
        }
    }
    public int BloodNum
    {
        get { return characterData != null ? characterData.bloodNum : 0; }
        set {
            if (value < Const.BLOOD_MIN) characterData.bloodNum = Const.BLOOD_MIN;
            else if (value > Const.BLOOD_MAX) characterData.bloodNum = Const.BLOOD_MAX;
            else characterData.bloodNum = value;
            OnBloodChanged?.Invoke(characterData.bloodNum);
        }
    }
    public bool IsDead
    {
        get { return characterData != null ? characterData.CheckIsSealDead() : false; }
    }
    //下面是老的
    public int MaxHealth
    {
        get { return characterData != null ? characterData.maxHealth: 0; }
        set { characterData.maxHealth = value; }
    }
    public int CurrentHealth
    {
        get { return characterData != null ? characterData.currentHealth : 0; }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence
    {
        get { return characterData != null ? characterData.baseDefence : 0; }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence
    {
        get { return characterData != null ? characterData.currentDefence : 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat
    public void TakeDamage(CharacterStats attacker,CharacterStats defender)
    {
        //防止出现负数伤害
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence,0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        //Update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth,MaxHealth);
        //经验update
        if(CurrentHealth <= 0)
            attacker.characterData.UpdateExp(characterData.killPoint);
    }

    public void TakeDamage(int damage,CharacterStats defender)
    {
        int currentDamage = Mathf.Max(damage - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击！" + coreDamage);
        }
        return (int)coreDamage;
    }
    #endregion

    #region Equip Weapon

    public void ChangeWeapon(ItemData_SO weapon)
    {
        UnEquipWeapon();
        EquipWeapon(weapon);
    }

    public void EquipWeapon(ItemData_SO weapon)
    {
        if(weapon.weaponPrefab != null)
        {
            Instantiate(weapon.weaponPrefab,weaponSlot);
        }
        attackData.ApplyWeaponData(weapon.weaponData);
        GetComponent<Animator>().runtimeAnimatorController = weapon.weaponAnimator;
    }

    public void UnEquipWeapon()
    {
        if(weaponSlot.transform.childCount != 0)
        {
            for(int i = 0;i < weaponSlot.transform.childCount; i++)
            {
                Destroy(weaponSlot.transform.GetChild(i).gameObject);
            }
        }
        attackData.ApplyWeaponData(baseAttackData);
        //TODO：切换动画
        GetComponent<Animator>().runtimeAnimatorController = baseAnimator;
    }

    #endregion

    #region Apply Data Change
    public void ApplyHealth(int amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }
    #endregion
}
