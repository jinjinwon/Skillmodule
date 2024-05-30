using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class SkillSystem : MonoBehaviour
{
    #region Event
    public delegate void SkillRegisteredHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillUnregisteredHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillStateChangedHandler(SkillSystem skillSystem, Skill skill,
        State<Skill> newState, State<Skill> prevState, int layer);
    public delegate void SkillActivatedHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillDeactivatedHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillAppliedHandler(SkillSystem skillSystem, Skill skill, int currentApplyCount);
    public delegate void SkillUsedHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillCanceledHandler(SkillSystem skillSystem, Skill skill);
    public delegate void SkillTargetSelectionCompleted(SkillSystem skillSystem, Skill skill,
        TargetSearcher targetSearcher, TargetSelectionResult result);

    public delegate void EffectStartedHandler(SkillSystem skillSystem, Effect effect);
    public delegate void EffectAppliedHandler(SkillSystem skillSystem, Effect effect, int currentApplyCount, int prevApplyCount);
    public delegate void EffectReleasedHandler(SkillSystem skillSystem, Effect effect);
    public delegate void EffectStackChangedHandler(SkillSystem skillSystem, Effect effect, int currentStack, int prevStack);
    #endregion

    [SerializeField]
    private Skill[] defaultSkills;

    private List<Skill> ownSkills = new();
    private Skill reservedSkill;

    private List<Skill> runningSkills = new();

    private List<Effect> runningEffects = new();
    private Queue<Effect> destroyEffectQueue = new();

    public Entity Owner { get; private set; }
    public IReadOnlyList<Skill> OwnSkills => ownSkills;
    public IReadOnlyList<Skill> RunningSkills => runningSkills;
    public IReadOnlyList<Effect> RunningEffects => runningEffects;

    public event SkillRegisteredHandler onSkillRegistered;
    public event SkillUnregisteredHandler onSkillUnregistered;
    public event SkillStateChangedHandler onSkillStateChanged;
    public event SkillActivatedHandler onSkillActivated;
    public event SkillDeactivatedHandler onSkillDeactivated;
    public event SkillUsedHandler onSkillUsed;
    public event SkillAppliedHandler onSkillApplied;
    public event SkillCanceledHandler onSkillCanceled;
    public event SkillTargetSelectionCompleted onSkillTargetSelectionCompleted;

    public event EffectStartedHandler onEffectStarted;
    public event EffectAppliedHandler onEffectApplied;
    public event EffectReleasedHandler onEffectReleased;
    public event EffectStackChangedHandler onEffectStackChanged;

    private void OnDestroy()
    {
        foreach (var skill in ownSkills)
            Destroy(skill);

        foreach (var effect in runningEffects)
            Destroy(effect);
    }

    private void Update()
    {
        UpdateSkills();
        UpdateRunningEffects();
        DestroyReleasedEffects();
        UpdateReservedSkill();
    }

    public void Setup(Entity entity)
    {
        Owner = entity;
        Debug.Assert(Owner != null, "SkillSystem::Awake - Owner�� null�� �� �� �����ϴ�.");
        SetupSkills();
    }

    private void SetupSkills()
    {
        foreach (var skill in defaultSkills)
            RegisterWithoutCost(skill);
    }

    public Skill RegisterWithoutCost(Skill skill, int level = 0)
    {
        Debug.Assert(!ownSkills.Exists(x => x.ID == skill.ID), "SkillSystem::Register - �̹� �����ϴ� Skill�Դϴ�.");

        var clone = skill.Clone() as Skill;
        if (level > 0)
            clone.Setup(Owner, level);
        else
            clone.Setup(Owner);

        clone.onStateChanged += OnSkillStateChanged;
        clone.onActivated += OnSkillActivated;
        clone.onDeactivated += OnSkillDeactivated;
        clone.onApplied += OnSkillApplied;
        clone.onUsed += OnSkillUsed;
        clone.onCanceled += OnSkillCanceled;
        clone.onTargetSelectionCompleted += OnSkillTargetSelectionCompleted;

        ownSkills.Add(clone);

        onSkillRegistered?.Invoke(this, clone);

        return clone;
    }

    public Skill Register(Skill skill, int level = 0)
    {
        Debug.Assert(!ownSkills.Exists(x => x.ID == skill.ID), "SkillSystem::Register - �̹� �����ϴ� Skill�Դϴ�.");
        Debug.Assert(skill.HasEnoughAcquisitionCost(Owner), "SkillSystem::Register - ������ ���� Cost�� �����մϴ�.");

        skill.UseAcquisitionCost(Owner);
        skill = RegisterWithoutCost(skill, level);

        return skill;
    }

    public bool Unregister(Skill skill)
    {
        skill = Find(skill);
        if (skill == null)
            return false;

        skill.Cancel(true);
        ownSkills.Remove(skill);

        onSkillUnregistered?.Invoke(this, skill);

        Destroy(skill);

        return true;
    }

    private void UpdateSkills()
    {
        foreach (var skill in ownSkills)
            skill.Update();
    }

    private void UpdateRunningEffects()
    {
        // Update�� Effect�� ���ؼ� ���ο� Effect�� runningEffects�� �߰��� ���� �����Ƿ�,
        // foreach���� �ƴ� for������ ��ȸ��
        int count = runningEffects.Count;
        for (int i = 0; i < count; i++)
        {
            var effect = runningEffects[i];
            if (effect.IsReleased)
                continue;

            effect.Update();

            if (effect.IsFinished)
                RemoveEffect(effect);
        }
    }

    private void DestroyReleasedEffects()
    {
        while (destroyEffectQueue.Count > 0)
        {
            var effect = destroyEffectQueue.Dequeue();
            runningEffects.Remove(effect);
            Destroy(effect);
        }
    }

    private void UpdateReservedSkill()
    {
        if (!reservedSkill)
            return;

        var selectionResult = reservedSkill.TargetSelectionResult;
        // selectionResult�� Target�̸� �ش� Target�� ��ġ��, �ƴϸ� ���õ� ��ġ�� ������.
        var targetPosition = selectionResult.selectedTarget?.transform.position ?? selectionResult.selectedPosition;
        // Target�� Skill�� ���� �ȿ� ������ ��,
        // Skill�� ��� ������ ���¸� ���, ����� �Ұ����ϴٸ� ��� ������ �����
        if (reservedSkill.IsInRange(targetPosition))
        {
            if (reservedSkill.IsUseable)
                reservedSkill.UseImmediately(targetPosition);
            reservedSkill = null;
        }
    }

    public void ReserveSkill(Skill skill) => reservedSkill = skill;

    public void CancelReservedSkill() => reservedSkill = null;

    private void ApplyNewEffect(Effect effect)
    {
        var newEffect = effect.Clone() as Effect;
        newEffect.SetTarget(Owner);
        newEffect.onStarted += OnEffectStarted;
        newEffect.onApplied += OnEffectApplied;
        newEffect.onReleased += OnEffectReleased;
        newEffect.onStackChanged += OnEffectStackChanged;

        newEffect.Start();
        if (newEffect.IsApplicable)
            newEffect.Apply();

        if (newEffect.IsFinished)
        {
            newEffect.Release();
            Destroy(newEffect);
        }
        else
            runningEffects.Add(newEffect);
    }

    public void Apply(Effect effect)
    {
        var runningEffect = Find(effect);
        // ���ο� Effect�ų� Effect�� �ߺ� ������ ���ȴٸ� Effect�� ������
        if (runningEffect == null || effect.IsAllowDuplicate)
            ApplyNewEffect(effect);
        else
        {
            // Stack�� ���̴� Effect��� Stack�� ����
            if (runningEffect.MaxStack > 1)
                runningEffect.CurrentStack++;
            // Effect�� RemoveDuplicateTargetOption�� Old(�̹� ���� ���� Effect)��� ���� Effect�� �����, Effect�� ���� ������
            else if (runningEffect.RemoveDuplicateTargetOption == EffectRemoveDuplicateTargetOption.Old)
            {
                RemoveEffect(runningEffect);
                ApplyNewEffect(effect);
            }
            // �� ���� ���� RemoveDuplicateTargetOption�� New��� �ǹ��̹Ƿ� ���� ���� Effect�� ������
        }
    }

    public void Apply(IReadOnlyList<Effect> effects)
    {
        foreach (var effect in effects)
            Apply(effect);
    }

    public void Apply(Skill skill)
    {
        Apply(skill.Effects);
    }

    public bool Use(Skill skill)
    {
        skill = Find(skill);

        Debug.Assert(skill != null,
            $"SkillSystem::IncreaseStack({skill.CodeName}) - Skill�� System�� ��ϵ��� �ʾҽ��ϴ�.");

        return skill.Use();
    }

    public bool Cancel(Skill skill, bool isForce = false)
    {
        skill = Find(skill);
        return skill?.Cancel(isForce) ?? false;
    }

    public void CancelAll(bool isForce = false)
    {
        CancelTargetSearching();

        foreach (var skill in runningSkills.ToArray())
            skill.Cancel(isForce);
    }

    public Skill Find(Skill skill)
        => skill.Owner == Owner ? skill : ownSkills.Find(x => x.ID == skill.ID);

    public Skill Find(System.Predicate<Skill> match)
        => ownSkills.Find(match);

    public Effect Find(Effect effect)
        => effect.Target == Owner ? effect : runningEffects.Find(x => x.ID == effect.ID);

    public Effect Find(System.Predicate<Effect> match)
        => runningEffects.Find(match);

    public List<Skill> FindAll(System.Predicate<Skill> match)
        => ownSkills.FindAll(match);

    public List<Effect> FindAll(System.Predicate<Effect> match)
        => runningEffects.FindAll(match);

    public bool Contains(Skill skill)
        => Find(skill) != null;

    public bool Contains(Effect effect)
        => Find(effect) != null;

    public bool RemoveEffect(Effect effect)
    {
        effect = Find(effect);

        if (effect == null || destroyEffectQueue.Contains(effect))
            return false;

        effect.Release();

        destroyEffectQueue.Enqueue(effect);

        return true;
    }

    public bool RemoveEffect(System.Predicate<Effect> predicate)
    {
        var target = runningEffects.Find(predicate);
        return target != null && RemoveEffect(target);
    }

    public bool RemoveEffect(Category category)
        => RemoveEffect(x => x.HasCategory(category));

    public void RemoveEffectAll()
    {
        foreach (var target in runningEffects)
            RemoveEffect(target);
    }

    public void RemoveEffectAll(System.Func<Effect, bool> predicate)
    {
        var targets = runningEffects.Where(predicate);
        foreach (var target in targets)
            RemoveEffect(target);
    }

    public void RemoveEffectAll(Effect effect) => RemoveEffectAll(x => x.ID == effect.ID);

    public void RemoveEffectAll(Category category) => RemoveEffectAll(x => x.HasCategory(category));

    public void CancelTargetSearching()
        => ownSkills.Find(x => x.IsInState<SearchingTargetState>())?.Cancel();

    // Animation���� ȣ��� Animation Event �Լ�
    // ���� ���� Skill�� �ߵ�(Apply)��Ŵ
    private void ApplyCurrentRunningSkill()
    {
        if (Owner.StateMachine.GetCurrentState() is InSkillActionState ownerState)
        {
            var runnsingSkill = ownerState.RunningSkill;
            runnsingSkill.Apply(runnsingSkill.ExecutionType != SkillExecutionType.Input);
        }
    }

    #region Event Callbacks
    private void OnSkillStateChanged(Skill skill, State<Skill> newState, State<Skill> prevState, int layer)
        => onSkillStateChanged?.Invoke(this, skill, newState, prevState, layer);

    private void OnSkillActivated(Skill skill)
    {
        runningSkills.Add(skill);

        onSkillActivated?.Invoke(this, skill);
    }

    private void OnSkillDeactivated(Skill skill)
    {
        runningSkills.Remove(skill);

        onSkillDeactivated?.Invoke(this, skill);
    }

    private void OnSkillUsed(Skill skill) => onSkillUsed?.Invoke(this, skill);

    private void OnSkillCanceled(Skill skill) => onSkillCanceled?.Invoke(this, skill);

    private void OnSkillApplied(Skill skill, int currentApplyCount)
        => onSkillApplied?.Invoke(this, skill, currentApplyCount);

    private void OnSkillTargetSelectionCompleted(Skill skill, TargetSearcher targetSearcher, TargetSelectionResult result)
    {
        if (result.resultMessage == SearchResultMessage.FindTarget || result.resultMessage == SearchResultMessage.FindPosition)
            reservedSkill = null;

        onSkillTargetSelectionCompleted?.Invoke(this, skill, targetSearcher, result);
    }

    private void OnEffectStarted(Effect effect) => onEffectStarted?.Invoke(this, effect);

    private void OnEffectApplied(Effect effect, int currentApplyCount, int prevApplyCount)
        => onEffectApplied?.Invoke(this, effect, currentApplyCount, prevApplyCount);

    private void OnEffectReleased(Effect effect) => onEffectReleased?.Invoke(this, effect);

    private void OnEffectStackChanged(Effect effect, int currentStack, int prevStack)
        => onEffectStackChanged?.Invoke(this, effect, currentStack, prevStack);
    #endregion
}