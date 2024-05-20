using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// --- ���ʸ� Ŭ���� ��뿡 ���� ����
// �ǵ�ġ ���� ������ Ÿ�� �Է� ����
// ���� �� �߰����� ����ȯ �ʿ����
// �ڽ�, ��ڽ̿� ���� ��� ����

// --- ���ʸ� Ŭ������ Where�� ���� ����
//where T : struct�� �� Ÿ���� ���Ŀ� ���� ���� �����̴�.
//where T : class�� ���� Ÿ���� ���Ŀ� ���� ���� �����̴�.
//where T : new()�� �Ű� ������ ���� �⺻ �����ڸ� ������ ���Ŀ� ���� ���� �����̴�.
//where T : ParentClass�� �Ļ��� ���Ŀ� ���� ���� �����̴�. ������ ParentClass�� ����ؾ��Ѵ�.
//where T : ISomeInterface�� ISomeInterface �������̽��� �����ϴ� ���Ŀ� ���� ���� �����̴�.
//where T : struct, ISomeInterface�� �� Ÿ�� ���� + �������̽� ���Ŀ� ���� ���� �����̴�.

// EntityType�� State�� �����ϴ� Entity�� Type
// StateMachine�� EntityType�� ��ġ�ؾ���
public class StateTransition<EntityType>
{
    // Transition Command�� ������ ��Ÿ��
    // nullable�ε� ���� �� ������ int.MinValue�� �־� ��� ���� (������ ��)
    public const int kNullCommand = int.MinValue;

    // Transition�� ���� ���� �Լ�, ���ڴ� ���� State, ������� ���� ���� ����(bool)
    // �ִϸ����� Transition�� �����ϸ� �����ϱ� ���� 
    private Func<State<EntityType>, bool> transitionCondition;

    // ���� State���� �ٽ� ���� State�� ���̰� ���������� ���� ����
    // �ִϸ����Ϳ� �����ϴ� Any ��� �����ϸ� �����ϱ� ����
    public bool CanTrainsitionToSelf { get; private set; }
    // ���� State
    public State<EntityType> FromState { get; private set; }
    // ������ State
    public State<EntityType> ToState { get; private set; }
    // ���� ��ɾ�
    public int TransitionCommand { get; private set; }

    // ���� ���� ����(Condition ���� ���� ����)
    public bool IsTransferable => transitionCondition == null || transitionCondition.Invoke(FromState);

    // ������ �Լ�
    public StateTransition(State<EntityType> fromState,
        State<EntityType> toState,
        int transitionCommand,
        Func<State<EntityType>, bool> transitionCondition,
        bool canTrainsitionToSelf)
    {
        // �ǹ̾��� Transition ������
        Debug.Assert(transitionCommand != kNullCommand || transitionCondition != null,
            "StateTransition - TransitionCommand�� TransitionCondition�� �� �� null�� �� �� �����ϴ�.");

        FromState = fromState;
        ToState = toState;
        TransitionCommand = transitionCommand;
        this.transitionCondition = transitionCondition;
        CanTrainsitionToSelf = canTrainsitionToSelf;
    }
}