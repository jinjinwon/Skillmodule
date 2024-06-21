using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public struct MonsterData
{
    public int level;
    
    [UnderlineTitle("애니메이터 컨트롤러")]
    public AnimatorController animatorController;

    [UnderlineTitle("공격 사거리")]
    public float attackRange;

    [UnderlineTitle("몬스터 스펙")]
    [SerializeReference]
    private StatMonsterOverride[] statOverrides;

    [SerializeReference, SubclassSelector]
    public AppearanceAction[] customActionsOnAppear;
}
