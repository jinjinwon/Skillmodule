using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System;

[System.Serializable]
public class Monster : IdentifiedObject
{
    #region EventsHandler

    #endregion

    #region Event

    #endregion

    [UnderlineTitle("카테고리")]
    [SerializeField]
    private Category[] category;

    [SerializeField]
    private MonsterType type;
    [SerializeField]
    private MonsterActionPattern pattern;

    [UnderlineTitle("몬스터 프리팹")]
    [SerializeField]
    private GameObject prefab;

    [UnderlineTitle("몬스터 애니메이터 오버라이드 컨트롤러")]
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    [UnderlineTitle("Collider Setting")]
    [SerializeField]
    private Vector3 center;

    [SerializeField]
    private float radius;

    [SerializeField]
    private float height;

    [UnderlineTitle("공격 사거리")]
    [SerializeField]
    private float attackRange;

    [UnderlineTitle("몬스터 스펙")]
    [SerializeReference]
    private StatOverride[] statOverrides;

    [UnderlineTitle("몬스터 스킬")]
    [SerializeReference]
    private Skill[] skills;

    [UnderlineTitle("등장 이벤트")]
    [SerializeReference, SubclassSelector]
    private AppearanceAction[] customActionsOnAppear;

    public GameObject Prefab => prefab;
    public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController;
    public Category[] Category => category;
    public Vector3 Center => center;
    public float Radius => radius;
    public float Height => height;

    public float AttackRange
    {
        get { return attackRange; }
        set
        {
            AttackRange = value;
        }
    }

    public StatOverride[] StatOverrides => statOverrides;
    public Skill[] Skills => skills;

    public AppearanceAction[] CustomActionsOnAppear => customActionsOnAppear;
}
