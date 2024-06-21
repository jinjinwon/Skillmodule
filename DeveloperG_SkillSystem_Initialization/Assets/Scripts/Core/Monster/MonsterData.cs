using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public struct MonsterData
{
    public int level;
    
    [UnderlineTitle("�ִϸ����� ��Ʈ�ѷ�")]
    public AnimatorController animatorController;

    [UnderlineTitle("���� ��Ÿ�")]
    public float attackRange;

    [UnderlineTitle("���� ����")]
    [SerializeReference]
    private StatMonsterOverride[] statOverrides;

    [SerializeReference, SubclassSelector]
    public AppearanceAction[] customActionsOnAppear;
}
