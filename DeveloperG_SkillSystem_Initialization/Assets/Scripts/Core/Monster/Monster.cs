using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class Monster : IdentifiedObject
{
    #region EventsHandler

    #endregion

    #region Event

    #endregion

    [SerializeField]
    private MonsterType type;
    [SerializeField]
    private MonsterActionPattern pattern;

    [UnderlineTitle("애니메이터 컨트롤러")]
    [SerializeField]
    private AnimatorController animatorController;

    [UnderlineTitle("공격 사거리")]
    [SerializeField]
    private float attackRange;

    [UnderlineTitle("몬스터 스펙")]
    [SerializeReference]
    private StatMonsterOverride[] statOverrides;

    [UnderlineTitle("등장 이벤트")]
    [SerializeReference, SubclassSelector]
    public AppearanceAction[] customActionsOnAppear;
}
