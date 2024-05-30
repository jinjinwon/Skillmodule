using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DealDamageAction : EffectAction
{
    [SerializeField]
    private float defaultDamage;
    // Bonus ������ �� Stat
    [SerializeField]
    private Stat bonusDamageStat;
    // Bonus ���� �� Stat�� ������ Factor
    // Stat�� �ִ� Bonus �� = bonusDamageStat.Value * bonusDamageStatFactor
    [SerializeField]
    private float bonusDamageStatFactor;
    [SerializeField]
    private float bonusDamagePerLevel;
    [SerializeField]
    private float bonusDamagePerStack;

    private float GetDefaultDamage(Effect effect)
        => defaultDamage + (effect.DataBonusLevel * bonusDamagePerLevel);

    private float GetStackDamage(int stack)
        => (stack - 1) * bonusDamagePerStack;

    private float GetBonusStatDamage(Entity user)
        => user.Stats.GetValue(bonusDamageStat) * bonusDamageStatFactor;

    private float GetTotalDamage(Effect effect, Entity user, int stack, float scale)
    {
        // Damage ��� ����
        // (defaultValue + (bonusLevel * bonusDamageByLevel)) + ((stack - 1) * bonusDamageByStack) + (bonusDamageStat.Value * bonuDamageStatFactor);
        var totalDamage = GetDefaultDamage(effect) + GetStackDamage(stack);
        if (bonusDamageStat)
            totalDamage += GetBonusStatDamage(user);

        // ���������� Effect�� Scale�� Damage�� Scaling��
        totalDamage *= scale;

        return totalDamage;
}

    public override bool Apply(Effect effect, Entity user, Entity target, int level, int stack, float scale)
    {

        var totalDamage = GetTotalDamage(effect, user, stack, scale);
        target.TakeDamage(user, effect, totalDamage);

        return true;
    }

    protected override IReadOnlyDictionary<string, string> GetStringsByKeyword(Effect effect)
    {
        var descriptionValuesByKeyword = new Dictionary<string, string>
        {
            ["defaultDamage"] = GetDefaultDamage(effect).ToString(".##"),
            ["bonusDamageStat"] = bonusDamageStat?.DisplayName ?? string.Empty,
            ["bonusDamageStatFactor"] = (bonusDamageStatFactor * 100f).ToString() + "%",
            ["bonusDamagePerLevel"] = bonusDamagePerLevel.ToString(),
            ["bonusDamagePerStack"] = bonusDamagePerStack.ToString(),
        };

        if (effect.User)
        {
            descriptionValuesByKeyword["totalDamage"] =
                GetTotalDamage(effect, effect.User, effect.CurrentStack, effect.Scale).ToString(".##");
        }
         
        return descriptionValuesByKeyword;
    }

    public override object Clone()
    {
        return new DealDamageAction()
        {
            defaultDamage = defaultDamage,
            bonusDamageStat = bonusDamageStat,
            bonusDamageStatFactor = bonusDamageStatFactor,
            bonusDamagePerLevel = bonusDamagePerLevel,
            bonusDamagePerStack = bonusDamagePerStack
        };
    }
}