using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AnimationUtilsWindow : EditorWindow
{

    [MenuItem("Window/Animation/Animation Utils")]
    private static void Init()
    {
        var window = (AnimationUtilsWindow)GetWindow(typeof(AnimationUtilsWindow));
        window.Show();
    }


    private Vector2 scrollPosition;
    private AnimationClip currentClip;


    // Fields
    private bool restrictToBones;
    private int targetKeysPerSecond;
    private Transform targetRemapArmature;
    private Transform sourceRemapArmature;

    [SerializeField] private List<AnimationRetarget> RetargetMap = new();

    private void OnGUI()
    {
        try
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Animation Utils");

            var selectedClip = EditorGUILayout.ObjectField("Animation Clip", currentClip, typeof(AnimationClip), false) as AnimationClip;

            if (!selectedClip)
            {
                selectedClip = currentClip;
                return;
            }

            if (currentClip != selectedClip)
            {
                // Init
                targetRemapArmature = null;
                RetargetMap.Clear();
                RetargetMap.AddRange(GetAllBoneNames(selectedClip).Select(x => new AnimationRetarget() { From = x }));

                currentClip = selectedClip;
            }

            var path = AssetDatabase.GetAssetPath(currentClip);

            if (!path.EndsWith(".anim"))
            {
                GUILayout.Label("Clip is readonly");

                if (GUILayout.Button("Clone Animation", GUILayout.Width(128), GUILayout.Height(25)))
                {
                    currentClip = CloneAnimation(currentClip);
                }

                return;
            }

            GUILayout.Space(32);
            GUILayout.Label("Cleaning");

            restrictToBones = GUILayout.Toggle(restrictToBones, "Only delete from bones (Not implemented)");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete position keys", GUILayout.Width(128), GUILayout.Height(25)))
            {
                DeleteAllPositionCurves(currentClip, restrictToBones);
            }
            if (GUILayout.Button("Delete rotation keys", GUILayout.Width(128), GUILayout.Height(25)))
            {
                DeleteAllRotationCurves(currentClip, restrictToBones);
            }
            if (GUILayout.Button("Delete scale keys", GUILayout.Width(128), GUILayout.Height(25)))
            {
                DeleteAllScaleCurves(currentClip, restrictToBones);
            }

            GUILayout.EndHorizontal();


            GUILayout.Space(32);
            GUILayout.Label("Jitter smoothing");

            targetKeysPerSecond = EditorGUILayout.IntField("Target keys per second", targetKeysPerSecond);
            if (GUILayout.Button("Decimate curves", GUILayout.Width(128), GUILayout.Height(25)))
            {
                SmoothAnimationClip(currentClip, targetKeysPerSecond);
            }



            GUILayout.Space(32);
            GUILayout.Label("Retargeting - Empty will be ignored");

            // TODO Save
            SerializedObject so = new SerializedObject(this);
            SerializedProperty stringsProperty = so.FindProperty("RetargetMap");

            sourceRemapArmature = EditorGUILayout.ObjectField("Source Armature", sourceRemapArmature, typeof(Transform), true) as Transform;
            var newTargetRemapArmature = EditorGUILayout.ObjectField("Target Armature", targetRemapArmature, typeof(Transform), true) as Transform;

            if (newTargetRemapArmature != targetRemapArmature)
            {

                targetRemapArmature = newTargetRemapArmature;

                if (newTargetRemapArmature != null)
                {
                    InitRemapper(targetRemapArmature, RetargetMap);
                }
            }
            if (targetRemapArmature != null)
            {
                if (targetRemapArmature.GetComponent<Animator>() == null)
                {
                    GUILayout.Label("WARNING: Target is missing Animator!");
                }

                EditorGUILayout.PropertyField(stringsProperty);

                if (GUILayout.Button("Retarget", GUILayout.Width(128), GUILayout.Height(25)))
                {
                    RetargetAnimationClip(currentClip, sourceRemapArmature, targetRemapArmature, RetargetMap);
                }
            }


        }
        finally
        {

            GUILayout.EndScrollView();
        }
    }

    private void RetargetAnimationClip(AnimationClip clip, Transform sourceRemapArmature, Transform targetRemapArmature, List<AnimationRetarget> retargetMap)
    {
        Undo.RecordObject(clip, "Retarget armature");


        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (EditorCurveBinding curveBinding in curveBindings)
        {
            try
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                var mappedBone = retargetMap.FirstOrDefault(x => curveBinding.path.EndsWith(x.From));

                if (mappedBone == null || (mappedBone.To == string.Empty || mappedBone.To == null))
                {
                    // Delete these
                    continue;
                }

                var sourceTransform = sourceRemapArmature.FindDeepChild(mappedBone.From);
                var targetTransform = targetRemapArmature.FindDeepChild(mappedBone.To);
                var path = AnimationUtility.CalculateTransformPath(targetTransform, targetRemapArmature);

                if (curveBinding.propertyName.Contains("Rotation"))
                {
                    var component = curveBinding.propertyName.Last();
                    var offset = Quaternion.Euler(targetTransform.localRotation.eulerAngles - sourceTransform.localRotation.eulerAngles);

                    Keyframe[] keyframes = curve.keys;

                    float offsetComponent = 0;
                    switch (component)
                    {
                        case 'x':
                            offsetComponent += offset.x;
                            break;
                        case 'y':
                            offsetComponent += offset.y;
                            break;
                        case 'z':
                            offsetComponent += offset.z;
                            break;
                        case 'w':
                            offsetComponent += offset.w;
                            break;
                        default:
                            break;
                    }

                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        keyframes[i].value += offsetComponent;
                    }
                    curve.keys = keyframes;

                }


                EditorCurveBinding newBinding = curveBinding;
                newBinding.path = path;
                AnimationUtility.SetEditorCurve(clip, newBinding, curve);

                Debug.Log($"Mapping {curveBinding.path} -> {path}");
            }
            finally
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
            }
        }

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssetIfDirty(clip);

    }

    private void SmoothAnimationClip(AnimationClip clip, int targetKeysPerSecond)
    {
        Undo.RecordObject(clip, "Smooth that animation :')");

        int smoothingWindow = 3;

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        List<AnimationCurve> newCurves = new();

        foreach (var curveBinding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            Keyframe[] originalKeyframes = curve.keys;
            int targetKeyframeCount = (int)(targetKeysPerSecond * originalKeyframes.Last().time);

            var newCurve = DownsampleAndSmoothCurve(curve, targetKeyframeCount, smoothingWindow);
            newCurves.Add(newCurve);
        }

        AnimationUtility.SetEditorCurves(clip, curveBindings, newCurves.ToArray());
    }

    // TODO: Doesnt smooth rotations that good.
    private AnimationCurve DownsampleAndSmoothCurve(AnimationCurve curve, int targetKeyframeCount, int smoothingWindow)
    {
        Keyframe[] originalKeyframes = curve.keys;

        int originalKeyframeCount = originalKeyframes.Length;

        if (originalKeyframeCount <= targetKeyframeCount)
        {
            // No need to downsample, the curve already has fewer or equal keyframes.
            return curve;
        }

        // Calculate the step size for downsampling
        float step = (float)(originalKeyframeCount - 1) / (targetKeyframeCount - 1);

        Keyframe[] downsampledKeyframes = new Keyframe[targetKeyframeCount];

        for (int i = 0; i < targetKeyframeCount; i++)
        {
            int originalIndex = Mathf.RoundToInt(i * step);
            originalIndex = Mathf.Clamp(originalIndex, 0, originalKeyframeCount - 1);

            downsampledKeyframes[i] = originalKeyframes[originalIndex];
        }

        // Apply smoothing to the downsampled keyframes
        for (int i = 0; i < targetKeyframeCount; i++)
        {
            float smoothedValue = 0f;
            int start = Mathf.Max(0, i - smoothingWindow / 2);
            int end = Mathf.Min(targetKeyframeCount - 1, i + smoothingWindow / 2);

            for (int j = start; j <= end; j++)
            {
                smoothedValue += downsampledKeyframes[j].value;
            }

            smoothedValue /= (end - start + 1);

            downsampledKeyframes[i].value = smoothedValue;
        }

        AnimationCurve smoothedCurve = new AnimationCurve(downsampledKeyframes);
        return smoothedCurve;
    }

    private void DeleteAllPositionCurves(AnimationClip clip, bool restrictToBones)
    {
        Undo.RecordObject(clip, "Delete animation position keys");

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var curveBinding in curveBindings)
        {
            if (curveBinding.propertyName.ToLower().Contains("position"))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                Debug.Log($"Deleting Key: {curveBinding.path}/{curveBinding.propertyName}");
            }
        }
    }

    private void DeleteAllRotationCurves(AnimationClip clip, bool restrictToBones)
    {
        Undo.RecordObject(clip, "Delete animation rotation keys");

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var curveBinding in curveBindings)
        {
            if (curveBinding.propertyName.ToLower().Contains("rotation"))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                Debug.Log($"Deleting Key: {curveBinding.path}/{curveBinding.propertyName}");
            }
        }
    }

    private void DeleteAllScaleCurves(AnimationClip clip, bool restrictToBones)
    {
        Undo.RecordObject(clip, "Delete animation scale keys");

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var curveBinding in curveBindings)
        {
            if (curveBinding.propertyName.ToLower().Contains("scale"))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                Debug.Log($"Deleting Key: {curveBinding.path}/{curveBinding.propertyName}");
            }
        }
    }

    private AnimationClip CloneAnimation(AnimationClip original)
    {
        // Create a new AnimationClip
        AnimationClip newClip = new AnimationClip();
        newClip.name = original.name + "_Copy";

        // Copy all curves from the source clip to the new clip
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(original);
        foreach (var curveBinding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(original, curveBinding);
            AnimationUtility.SetEditorCurve(newClip, curveBinding, curve);
        }

        AssetDatabase.CreateAsset(newClip, "Assets/" + newClip.name + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newClip;

    }

    public static string NormalizeBoneName(string boneName)
    {
        string newName = boneName;
        var lowercase = boneName.ToLower();

        if (lowercase.Contains("right"))
            newName = Regex.Replace(newName, "right", "", RegexOptions.IgnoreCase) + ".R";

        if (lowercase.Contains("left"))
            newName = Regex.Replace(newName, "left", "", RegexOptions.IgnoreCase) + ".L";

        return newName;
    }

    private string[] GetAllBoneNames(AnimationClip clip)
    {
        HashSet<string> names = new HashSet<string>();
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var curveBinding in curveBindings)
        {
            var splitPath = curveBinding.path.Split('/');
            foreach (var name in splitPath)
            {
                names.Add(name);
            }
        }

        return names.ToArray();
    }

    private string[] GetAllTransformNames(Transform transform)
    {
        return transform.GetComponentsInChildren<Transform>(true).Select(x => x.name).ToArray();
    }

    private void InitRemapper(Transform targetRemapArmature, List<AnimationRetarget> retargetMap)
    {
        const int BONE_NAME_MATCH_THRESHOLD = 80;
        var allChildren = GetAllTransformNames(targetRemapArmature);
        var allSourceNames = retargetMap.ToList();

        foreach (var child in allChildren)
        {
            var childName = NormalizeBoneName(child);

            var match = allSourceNames
                .OrderByDescending(x => StringComparer.CompareStrings(childName.ToLower(), NormalizeBoneName(x.From).ToLower()))
                .FirstOrDefault();

            if (match != null && (match.To == null || match.To == string.Empty))
            {
                float matchPercent = StringComparer.CompareStrings(childName.ToLower(), NormalizeBoneName(match.From).ToLower());

                if (matchPercent > BONE_NAME_MATCH_THRESHOLD)
                {
                    match.To = child;
                    allSourceNames.Remove(match);
                }
            }

        }
    }
}

[Serializable]
class AnimationRetarget
{
    public string From;
    public string To;
}

class StringComparer
{

    public static float CompareStrings(string s1, string s2)
    {
        int maxLength = Math.Max(s1.Length, s2.Length);

        if (maxLength == 0)
        {
            // Both strings are empty, consider them 100% similar
            return 100.0f;
        }

        int distance = LevenshteinDistance(s1, s2);

        // Calculate similarity as a percentage
        float similarity = ((maxLength - distance) / maxLength) * 100f;

        return similarity;
    }

    static int LevenshteinDistance(string s1, string s2)
    {
        int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (int j = 0; j <= s2.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}
