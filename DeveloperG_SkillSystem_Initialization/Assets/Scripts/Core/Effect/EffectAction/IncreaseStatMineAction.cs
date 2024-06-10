using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class IncreaseStatMineAction : EffectAction
{
    [SerializeField]
    private Stat stat;
    [SerializeField]
    private float defaultValue;
    [SerializeField]
    private Stat bonusValueStat;
    [SerializeField]
    private float bonusValueStatFactor;
    [SerializeField]
    private float bonusValuePerLevel;
    [SerializeField]
    private float bonusValuePerStack;
    // ������ ���� Stat�� DefaultValue�� ���� ���ΰ�? Bonus Value�� �߰��� ���ΰ�?
    [SerializeField]
    private bool isBonusType = true;
    // ������ ���� Release�� �� �ǵ��� ���ΰ�?
    [SerializeField]
    private bool isUndoOnRelease = true;

    private float totalValue;

    private float GetDefaultValue(Effect effect)
        => defaultValue + (effect.DataBonusLevel * bonusValuePerLevel);

    private float GetStackValue(int stack)
        => (stack - 1) * bonusValuePerStack;

    private float GetBonusStatValue(Entity user)
        => user.Stats.GetValue(bonusValueStat) * bonusValueStatFactor;

    private float GetTotalValue(Effect effect, Entity user, int stack, float scale)
    {
        totalValue = GetDefaultValue(effect) + GetStackValue(stack);
        if (bonusValueStat)
            totalValue += GetBonusStatValue(user);

        totalValue *= scale;

        return totalValue;
    }

    public override bool Apply(Effect effect, Entity user, Entity target, int level, int stack, float scale)
    {
        totalValue = GetTotalValue(effect, user, stack, scale);

        if (isBonusType)
            user.Stats.SetBonusValue(stat, this, totalValue);
        else
            user.Stats.IncreaseDefaultValue(stat, totalValue);

        return true;
    }

    public override void Release(Effect effect, Entity user, Entity target, int level, float scale)
    {
        if (!isUndoOnRelease)
            return;

        if (isBonusType)
            user.Stats.RemoveBonusValue(stat, this);
        else
            user.Stats.IncreaseDefaultValue(stat, -totalValue);
    }

    public override void OnEffectStackChanged(Effect effect, Entity user, Entity target, int level, int stack, float scale)
    {
        if (!isBonusType)
            Release(effect, user, target, level, scale);

        Apply(effect, user, target, level, stack, scale);
    }

    protected override IReadOnlyDictionary<string, string> GetStringsByKeyword(Effect effect)
    {
        var descriptionValuesByKeyword = new Dictionary<string, string>
        {
            { "stat", stat.DisplayName },
            { "defaultValue", GetDefaultValue(effect).ToString("0.##") },
            { "bonusDamageStat", bonusValueStat?.DisplayName ?? string.Empty },
            { "bonusDamageStatFactor", (bonusValueStatFactor * 100f).ToString() + "%" },
            { "bonusDamageByLevel", bonusValuePerLevel.ToString() },
            { "bonusDamageByStack", bonusValuePerStack.ToString() },
        };

        if (effect.Owner != null)
        {
            descriptionValuesByKeyword.Add("totalValue",
                GetTotalValue(effect, effect.User, effect.CurrentStack, effect.Scale).ToString("0.##"));
        }

        return descriptionValuesByKeyword;

    }

    public override object Clone()
    {
        return new IncreaseStatMineAction()
        {
            stat = stat,
            defaultValue = defaultValue,
            bonusValueStat = bonusValueStat,
            bonusValueStatFactor = bonusValueStatFactor,
            bonusValuePerLevel = bonusValuePerLevel,
            bonusValuePerStack = bonusValuePerStack,
            isBonusType = isBonusType,
            isUndoOnRelease = isUndoOnRelease
        };
    }
}
