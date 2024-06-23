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

    [SerializeField]
    private MonsterType type;
    [SerializeField]
    private MonsterActionPattern pattern;

    [UnderlineTitle("몬스터 프리팹")]
    [SerializeField]
    private GameObject prefab;

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
    private StatMonsterOverride[] statOverrides;

    [UnderlineTitle("몬스터 스킬")]
    [SerializeReference]
    private Skill[] skills;

    [UnderlineTitle("등장 이벤트")]
    [SerializeReference, SubclassSelector]
    private AppearanceAction[] customActionsOnAppear;

    public GameObject Prefab => prefab;

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

    public StatMonsterOverride[] StatOverrides => statOverrides;
    public Skill[] Skills => skills;

    public AppearanceAction[] CustomActionsOnAppear => customActionsOnAppear;
}
