using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisualTween))]
public class TweenComponentEditor : Editor
{
    private SerializedProperty duration;
    private SerializedProperty delay;
    private SerializedProperty space;
    private SerializedProperty tweenType;
    private SerializedProperty endValue;
    private SerializedProperty endColor;
    private SerializedProperty easeType;
    private SerializedProperty autoPlay;
    private SerializedProperty ignoreTimeScale;
    private SerializedProperty loop;
    private SerializedProperty loopType;
    private SerializedProperty loopCount;
    private SerializedProperty destroyOnComplete;
    private SerializedProperty resetOnDisable;
    private SerializedProperty onStart;
    private SerializedProperty onComplete;

    // Player
    public Vector3 PlayPosition = Vector3.zero;
    public Vector3 PlayRotation = Vector3.zero;
    public Vector3 PlayScale = Vector3.one;
    private Tween PlayTween;

    private void OnEnable()
    {
        duration = serializedObject.FindProperty("Duration");
        delay = serializedObject.FindProperty("Delay");
        space = serializedObject.FindProperty("Space");
        tweenType = serializedObject.FindProperty("Type");
        endValue = serializedObject.FindProperty("EndValue");
        endColor = serializedObject.FindProperty("EndColor");
        easeType = serializedObject.FindProperty("Ease");
        autoPlay = serializedObject.FindProperty("AutoPlay");
        ignoreTimeScale = serializedObject.FindProperty("IgnoreTimeScale");
        loop = serializedObject.FindProperty("Loop");
        loopType = serializedObject.FindProperty("LoopType");
        loopCount = serializedObject.FindProperty("LoopCount");
        destroyOnComplete = serializedObject.FindProperty("DestroyOnComplete");
        resetOnDisable = serializedObject.FindProperty("ResetOnDisable");
        onStart = serializedObject.FindProperty("OnStart");
        onComplete = serializedObject.FindProperty("OnComplete");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(delay);
        EditorGUILayout.PropertyField(autoPlay);
        EditorGUILayout.PropertyField(ignoreTimeScale);
        EditorGUILayout.PropertyField(destroyOnComplete);
        if (!destroyOnComplete.boolValue)
            EditorGUILayout.PropertyField(resetOnDisable);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tween", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tweenType);
        EditorGUI.indentLevel++;

        switch ((TweenType)tweenType.enumValueIndex)
        {
            case TweenType.Color:
                EditorGUILayout.PropertyField(endColor);
                break;
            default:
                EditorGUILayout.PropertyField(endValue);
                break;
        }

        EditorGUILayout.PropertyField(space);
        EditorGUILayout.PropertyField(easeType);
        EditorGUI.indentLevel--;


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Looping", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(loop);
        if (loop.boolValue)
        {
            EditorGUILayout.PropertyField(loopCount);
            EditorGUILayout.PropertyField(loopType);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(onStart);
        EditorGUILayout.PropertyField(onComplete);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (!DOTweenEditorPreview.isPreviewing && GUILayout.Button("Play Tween"))
        {
            VisualTween tweenComponent = (VisualTween)target;

            PlayPosition = tweenComponent.transform.position;
            PlayRotation = tweenComponent.transform.rotation.eulerAngles;
            PlayScale = tweenComponent.transform.localScale;

            PlayTween = tweenComponent.PlaySequence();
            DOTweenEditorPreview.PrepareTweenForPreview(PlayTween, true, false);
            DOTweenEditorPreview.Start();
        }

        if (DOTweenEditorPreview.isPreviewing && GUILayout.Button("Stop Tween"))
        {
            ResetPlayer();
        }

        GUILayout.EndHorizontal();
    }

    private void OnDisable()
    {
        ResetPlayer();
    }

    private void ResetPlayer()
    {
        if (!DOTweenEditorPreview.isPreviewing) return;

        DOTween.KillAll();
        DOTweenEditorPreview.Stop();

        VisualTween tweenComponent = (VisualTween)target;
        tweenComponent.transform.position = PlayPosition;
        tweenComponent.transform.rotation = Quaternion.Euler(PlayRotation);
        tweenComponent.transform.localScale = PlayScale;
    }
}
