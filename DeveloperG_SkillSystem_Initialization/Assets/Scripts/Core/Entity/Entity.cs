using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

// Entitiy�� Control ��ü�� ��Ÿ���� ���� enum
public enum EntityControlType
{
    Player,
    AI
}

public class Entity : MonoBehaviour
{
    #region 6-11
    #region Events
    public delegate void TakeDamageHandler(Entity entity, Entity instigator, object causer, float damage);
    public delegate void DeadHandler(Entity entity);
    #endregion
    #endregion

    // ���⼭ Category�� ���� �Ʊ��� �����ϱ� ���� �뵵�� ����
    [SerializeField]
    private Category[] categories;
    [SerializeField]
    private EntityControlType controlType;

    // socket�� Entity Script�� ���� GameObject�� �ڽ� GameObject�� �ǹ���
    // ��ų�� �߻� ��ġ��, � Ư�� ��ġ�� �����صΰ� �ܺο��� ã�ƿ������� ����
    private Dictionary<string, Transform> socketsByName = new();

    public EntityControlType ControlType => controlType;
    public IReadOnlyList<Category> Categories => categories;
    public bool IsPlayer => controlType == EntityControlType.Player;

    public Animator Animator { get; private set; }
    #region 6-10
    public Stats Stats { get; private set; }
    public bool IsDead => Stats.HPStat != null && Mathf.Approximately(Stats.HPStat.DefaultValue, 0f);
    #endregion
    #region 7-1
    public EntityMovement Movement { get; private set; }
    #endregion
    #region 8-1
    public MonoStateMachine<Entity> StateMachine { get; private set; }
    #endregion
    #region 14-1
    public SkillSystem SkillSystem { get; private set; }
    #endregion
    // Target�� �� �״�� ��ǥ ������� Entity�� �����ؾ��ϴ� Target�� ���� �ְ�, ġ���ؾ��ϴ� Target�� ���� ����
    public Entity Target { get; set; }

    #region 6-12
    public event TakeDamageHandler onTakeDamage;
    public event DeadHandler onDead;
    #endregion

    private void Awake()
    {
        Animator = GetComponent<Animator>();

        #region 6-13
        Stats = GetComponent<Stats>();
        Stats.Setup(this);
        #endregion

        #region 7-2
        Movement = GetComponent<EntityMovement>();
        Movement?.Setup(this);
        #endregion

        #region 8-2
        StateMachine = GetComponent<MonoStateMachine<Entity>>();
        StateMachine?.Setup(this);
        #endregion

        #region 14-2
        SkillSystem = GetComponent<SkillSystem>();
        SkillSystem?.Setup(this);
        #endregion
    }

    #region 6-14
    // ������ ���ĵ� �������� ���� ������ �� �ְ� ����ٸ�
    // ����� ���Ӹ��� �ٸ� ������ ������ ����Ѵٴ���, ĳ���͸��� �ٸ� ������ ������ ����ϴ� ��
    // ������ ������ Entity Ŭ������ ���� �� ����.
    public void TakeDamage(Entity instigator, object causer, float damage)
    {
        if (IsDead)
            return;

        float prevValue = Stats.HPStat.DefaultValue;
        Stats.HPStat.DefaultValue -= damage;

        onTakeDamage?.Invoke(this, instigator, causer, damage);

        if (Mathf.Approximately(Stats.HPStat.DefaultValue, 0f))
            OnDead();
    }

    private void OnDead()
    {
        #region 7-3
        if (Movement)
            Movement.enabled = false;
        #endregion

        SkillSystem.CancelAll(true);

        onDead?.Invoke(this);
    }
    #endregion

    // root transform�� �ڽ� transform���� ��ȸ�ϸ� �̸��� socketName�� GameObject�� Transform�� ã�ƿ� 
    private Transform GetTransformSocket(Transform root, string socketName)
    {
        if (root.name == socketName)
            return root;

        // root transform�� �ڽ� transform���� ��ȸ
        foreach (Transform child in root)
        {
            // ����Լ��� ���� �ڽĵ� �߿� socketName�� �ִ��� �˻���
            var socket = GetTransformSocket(child, socketName);
            if (socket)
                return socket;
        }

        return null;
    }

    // ������ִ� Socket�� �������ų� ��ȸ�� ���� ã�ƿ�
    public Transform GetTransformSocket(string socketName)
    {
        // dictionary���� socketName�� �˻��Ͽ� �ִٸ� return
        if (socketsByName.TryGetValue(socketName, out var socket))
            return socket;

        // dictionary�� �����Ƿ� ��ȸ �˻�
        socket = GetTransformSocket(transform, socketName);
        // socket�� ã���� dictionary�� �����Ͽ� ���Ŀ� �ٽ� �˻��� �ʿ䰡 ������ ��
        if (socket)
            socketsByName[socketName] = socket;

        return socket;
    }

    public bool HasCategory(Category category) => categories.Any(x => x.ID == category.ID);

    #region 8-3
    public bool IsInState<T>() where T : State<Entity>
        => StateMachine.IsInState<T>();

    public bool IsInState<T>(int layer) where T : State<Entity>
        => StateMachine.IsInState<T>(layer);
    #endregion
}