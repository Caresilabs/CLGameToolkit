using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FiniteStateMachine<T> : ISerializationCallbackReceiver
{
    [SerializeReference] private List<FSMInternalState> editorStates;
    [SerializeField] private List<FSMInternalTransition> editorTransitions;
    [SerializeField] private T entity;

    private Dictionary<string, FSMState<T>> states;
    private Dictionary<FSMState<T>, List<FSMTransition>> transitions;
    private List<FSMTransition> anyTransitions;

    public string State { get { return currentState.GetType().Name; } }
    private FSMState<T> currentState;

    public FiniteStateMachine()
    {
        this.states = new Dictionary<string, FSMState<T>>();
        this.transitions = new Dictionary<FSMState<T>, List<FSMTransition>>();
        this.anyTransitions = new List<FSMTransition>();
    }

    public FiniteStateMachine(T entity)
    {
        this.states = new Dictionary<string, FSMState<T>>();
        this.transitions = new Dictionary<FSMState<T>, List<FSMTransition>>();
        this.anyTransitions = new List<FSMTransition>();
        this.entity = entity;
    }

    /// <summary>
    /// Init the FSM with a custom Entity. This could be useful when the entity is NOT serializable.
    /// </summary>
    /// <param name="entity"></param>
    public void Init(T entity)
    {
        this.entity = entity;
        foreach (var item in states.Values)
        {
            item.Entity = entity;
        }
        SetState(editorStates[0] as FSMState<T>);
    }

    public FiniteStateMachine<T> Add(FSMState<T> state, bool isStartState = false)
    {
        var id = state.Id ?? state.GetType().Name;

        // Duplicate states
        if (states.ContainsKey(id))
        {
            id += Guid.NewGuid();
        }

        states.Add(id, state);
        state.Entity = entity;
        state.SetState = SetStateFail;
        state.Complete = null;

        if (isStartState)
            SetState(id);

        return this;
    }

    public FiniteStateMachine<T> AddTransition(string from, string to, Func<bool> predicate = null)
    {
        return AddTransition(from, states[to], predicate);
    }

    public FiniteStateMachine<T> AddTransition(FSMState<T> from, string to, Func<bool> predicate = null)
    {
        return AddTransition(from, states[to], predicate);
    }

    public FiniteStateMachine<T> AddTransition(string from, FSMState<T> to, Func<bool> predicate = null)
    {
        if (from == null)
        {
            anyTransitions.Add(new FSMTransition(to, predicate));
            return this;
        }

        return AddTransition(states[from], to, predicate);
    }


    /// <summary>
    /// AddTransition
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="predicate">Null means this transition is used when from is set to Complete.</param>
    public FiniteStateMachine<T> AddTransition(FSMState<T> from, FSMState<T> to, Func<bool> predicate = null)
    {
        if (transitions.TryGetValue(from, out var list) == false)
        {
            list = new List<FSMTransition>();
            transitions[from] = list;
        }

        list.Add(new FSMTransition(to, predicate));
        return this;
    }

    public void Update()
    {
        //Query any transitions
        if (currentState == null)
        {
            foreach (var anyTransition in anyTransitions)
            {
                if (anyTransition.Condition == null || anyTransition.Condition())
                    SetState(anyTransition.To);
            }
        }

        // Query Conditions
        List<FSMTransition> currentTransitions = transitions.GetValueOrDefault(currentState);
        if (currentTransitions != null && currentTransitions.Count > 0)
        {
            foreach (var currentTransition in currentTransitions)
            {
                if (currentTransition.Condition != null && currentTransition.Condition())
                    SetState(currentTransition.To);
            }
        }

        currentState.OnUpdate();
    }

    public void ForceState(string id)
    {
        SetState(id);
    }

    private void SetState(string id)
    {
        SetState(states[id]);
    }

    private void SetState(FSMState<T> newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.SetState = SetStateFail;
            currentState.Complete = null;
        }

        Logger.Debug("New FSM State: " + newState.GetType());
        /// State = id; TODOOO
        currentState = newState;
        newState.SetState = SetState;
        newState.Complete = SetNextState;

        newState.OnEnter();
    }

    private void SetNextState()
    {
        List<FSMTransition> list = transitions.GetValueOrDefault(currentState, null);
        if (list == null || list.Count == 0)
        {
            Logger.Error($"Ghost: MISSING STATE for {entity} after '{State}'");
            return;
        }

        FSMTransition nextTransition = list[0];
        SetState(nextTransition.To);
    }

    private void SetStateFail(string id)
    {
        Logger.Error("Error trying to SetState from an inactive state: " + entity.ToString() + "::" + id);
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize()
    { 
        if (editorStates != null && states.Count == 0)
        {
            foreach (var state in editorStates)
            {
                Add(state as FSMState<T>);
            }

            foreach (var transition in editorTransitions)
            {
                AddTransition(transition.From != "Any" ? transition.From : null, transition.To);
            }

#if !UNITY_EDITOR
            editorTransitions.Clear();
            editorStates.Clear();

             if (entity != null)
            {
                Init(entity);
            }
#endif
        }

    }

    private class FSMTransition
    {
        public Func<bool> Condition { get; }

        public FSMState<T> To;

        public FSMTransition(FSMState<T> to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }

}

[Serializable]
public abstract class FSMInternalState
{
    [field: HideInInspector]
    [field: SerializeField]
    public string Id { get; set; }

    internal Action<string> SetState;
    internal Action Complete;

    public abstract void OnEnter();

    public abstract void OnUpdate();

    public abstract void OnExit();
}

[Serializable]
internal class FSMInternalTransition
{
    public string From;
    public string To;
}

public abstract class FSMState<T> : FSMInternalState
{
    new private string Id;

    [field: System.NonSerialized]
    internal T Entity { get; set; }
}
