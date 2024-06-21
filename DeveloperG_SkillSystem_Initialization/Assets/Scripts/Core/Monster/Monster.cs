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

    [UnderlineTitle("�ִϸ����� ��Ʈ�ѷ�")]
    [SerializeField]
    private AnimatorController animatorController;

    [UnderlineTitle("���� ��Ÿ�")]
    [SerializeField]
    private float attackRange;

    [UnderlineTitle("���� ����")]
    [SerializeReference]
    private StatMonsterOverride[] statOverrides;

    [UnderlineTitle("���� �̺�Ʈ")]
    [SerializeReference, SubclassSelector]
    public AppearanceAction[] customActionsOnAppear;
}
