using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FiniteStateMachine<T> : ISerializationCallbackReceiver
{
    [SerializeReference] private List<FSMInternalState> editorStates;
    [SerializeField] private List<FSMInternalTransition> editorTransitions;
    private FSMState<T> editorStartState;

    [SerializeField] private T entity;

    private readonly Dictionary<string, FSMState<T>> states;
    private readonly Dictionary<FSMState<T>, List<FSMTransition>> transitions;
    private readonly List<FSMTransition> anyTransitions;

    public string StateId => currentState.Id;
    public string StateTag => currentState.Tag;
    public string StateClass { get; private set; }

    private FSMState<T> currentState;

    public FiniteStateMachine()
    {
        this.states = new Dictionary<string, FSMState<T>>();
        this.transitions = new Dictionary<FSMState<T>, List<FSMTransition>>();
        this.anyTransitions = new List<FSMTransition>();
    }

    //public FiniteStateMachine(T entity)
    //{
    //    this.states = new Dictionary<string, FSMState<T>>();
    //    this.transitions = new Dictionary<FSMState<T>, List<FSMTransition>>();
    //    this.anyTransitions = new List<FSMTransition>();
    //    this.entity = entity;
    //}

    /// <summary>
    /// Init the FSM with a custom Entity. This could be useful when the entity is NOT serializable.
    /// </summary>
    /// <param name="entity"></param>
    public void Init(T entity)
    {
        if (currentState != null)
            return; // Already running

        this.entity = entity;

        foreach (var item in states.Values)
            item.Entity = entity;

        if (editorStartState != null)
        {
            SetState(editorStartState);
            editorStartState = null;
        }

#if !UNITY_EDITOR
        // Allow edits in inspector
        editorTransitions.Clear();
        editorStates.Clear();
#endif
    }

    public FiniteStateMachine<T> Add(FSMState<T> state, bool isStartState = false)
    {
        if (state == null) return this;

        var id = state.Id ?? state.GetType().Name;

        // Duplicate states
        if (states.ContainsKey(id))
        {
            id += Guid.NewGuid();
        }

        states.Add(id, state);
        state.Entity = entity;
        state.Complete = null;

        if (isStartState)
            SetState(id);

        return this;
    }

    public FiniteStateMachine<T> AddTransition(string from, string to, Func<bool> predicate = null, float probability = 1f)
    {
        return AddTransition(from, states[to], predicate, probability);
    }

    public FiniteStateMachine<T> AddTransition(FSMState<T> from, string to, Func<bool> predicate = null, float probability = 1f)
    {
        return AddTransition(from, states[to], predicate, probability);
    }

    public FiniteStateMachine<T> AddTransition(string from, FSMState<T> to, Func<bool> predicate = null, float probability = 1f)
    {
        if (from == null)
        {
            anyTransitions.Add(new FSMTransition(to, predicate, probability));
            return this;
        }

        return AddTransition(states[from], to, predicate, probability);
    }

    /// <summary>
    /// AddTransition
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="predicate">Null means this transition is used when from is set to Complete.</param>
    public FiniteStateMachine<T> AddTransition(FSMState<T> from, FSMState<T> to, Func<bool> predicate = null, float probability = 1f)
    {
        if (transitions.TryGetValue(from, out var list) == false)
        {
            list = new List<FSMTransition>();
            transitions[from] = list;
        }

        list.Add(new FSMTransition(to, predicate, probability));
        return this;
    }

    public void Update()
    {
        if (currentState == null)
        {
            if (entity != null)
                Init(entity);
            else
                Logger.Warn("Trying to update FSM without state");
            return;
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

    public bool CompareTag(string tag)
    {
        return currentState.Tag == tag;
    }

    public void ForceComplete()
    {
        currentState.Complete();
    }

    public bool ForceState(string id)
    {
        return SetState(id);
    }

    public bool ForceState<TState>()
    {
        foreach (var state in states)
        {
            if (state.Value is TState)
                return SetState(state.Key);
        }

        return false;
    }

    /// <summary>
    /// Avoid fetching states directly
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public TState GetState<TState>()
    {
        foreach (var state in states)
        {
            if (state.Value is TState validState)
                return validState;
        }

        return default;
    }

    private bool SetState(string id)
    {
        if (!states.ContainsKey(id))
            return false;

        if (id == currentState.Id)
            return true;

        SetState(states[id]);
        return true;
    }

    private void SetState(FSMState<T> newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.Complete = SetNextStateTemplate;
        }

        currentState = newState;
        StateClass = newState.GetType().Name;
        newState.Complete = SetNextState;
        newState.enterTime = Time.time;

        Logger.Debug($"New FSM State: {StateClass} [{StateId}] ({entity})");
        newState.OnEnter();
    }

    private void SetNextStateTemplate() { }

    private void SetNextState()
    {
        List<FSMTransition> list = transitions.GetValueOrDefault(currentState);
        if (list != null)
        {
            foreach (FSMTransition transition in list)
            {
                if (transition.To.GuardCheck())
                {
                    if (transition.Probability < 1 && UnityEngine.Random.value > transition.Probability)
                        continue;

                    SetState(transition.To);
                    return;
                }
            }
        }

        foreach (FSMTransition anyTransition in anyTransitions)
        {
            if ((anyTransition.Condition == null || anyTransition.Condition())
                && anyTransition.To.GuardCheck())
            {
                SetState(anyTransition.To);
                return;
            }
        }

        Logger.Error($"Missing State for {entity} after '{StateClass}'");
    }

    private void SetStateFail(string id)
    {
        Logger.Error("Error trying to SetState from an inactive state: " + entity.ToString() + "::" + id);
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (editorStates != null && editorStates.Count > 0 && states.Count == 0)
        {
            editorStartState = editorStates[0] as FSMState<T>;
            foreach (var state in editorStates)
            {
                Add(state as FSMState<T>);
            }

            foreach (var transition in editorTransitions)
            {
                AddTransition(transition.From != "Any" ? transition.From : null, transition.To, null, transition.Probability);
            }
        }
    }

    /*
     * Exits the FSM
     * Calls Exit on current state
     */
    public void Dispose()
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.Complete = SetNextStateTemplate;
            currentState = null;
        }
    }

    private class FSMTransition
    {
        public Func<bool> Condition { get; }

        public FSMState<T> To;

        public float Probability = 1f;

        public FSMTransition(FSMState<T> to, Func<bool> condition, float probability)
        {
            To = to;
            Condition = condition;
            Probability = Mathf.Clamp01(probability);
        }
    }

}

[Serializable]
public abstract class FSMInternalState
{
    [field: HideInInspector]
    [field: SerializeField]
    public string Id { get; set; }
    public virtual string Tag => string.Empty;

    public Action Complete;
    public void CompleteCallback() => Complete(); // Safe way to use as callback

    public abstract void OnEnter();

    public abstract void OnUpdate();

    public abstract void OnExit();

    public virtual bool GuardCheck() => true;
}

[Serializable]
internal class FSMInternalTransition
{
    public string From;
    public string To;
    public float Probability = 1f;
}

public abstract class FSMState<T> : FSMInternalState
{
    [field: NonSerialized]
    public T Entity { get; internal set; }

    internal float enterTime;
    public float StateTime => Time.time - enterTime;
}
