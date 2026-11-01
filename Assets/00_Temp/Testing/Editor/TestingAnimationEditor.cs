using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

[CustomEditor(typeof(TestingAnimation))]
public class TestingAnimationEditor : Editor
{
    private static readonly Dictionary<int, PlayableGraph> activeGraphs = new Dictionary<int, PlayableGraph>();

    static TestingAnimationEditor()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        AssemblyReloadEvents.beforeAssemblyReload += StopAllAnimations;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animator Parameters", EditorStyles.boldLabel);

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play Mode to change Animator parameters.", MessageType.Info);

        var animators = serializedObject.FindProperty("animators");
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            for (int animatorIndex = 0; animatorIndex < animators.arraySize; animatorIndex++)
            {
                var animator = animators.GetArrayElementAtIndex(animatorIndex).objectReferenceValue as Animator;
                if (animator == null) continue;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(animator.name, EditorStyles.boldLabel);

                    if (animator.runtimeAnimatorController == null)
                    {
                        EditorGUILayout.LabelField("No Animator Controller assigned.", EditorStyles.miniLabel);
                        continue;
                    }

                    foreach (var parameter in animator.parameters)
                        DrawParameter(animator, parameter);

                    DrawAnimationClipButtons(animator);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawAnimationClipButtons(Animator animator)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Clips", EditorStyles.boldLabel);

        var clips = animator.runtimeAnimatorController.animationClips;
        var drawnClipIds = new HashSet<int>();
        bool hasClips = false;

        foreach (var clip in clips)
        {
            if (clip == null || !drawnClipIds.Add(clip.GetInstanceID())) continue;

            hasClips = true;
            if (GUILayout.Button($"Play: {clip.name}"))
                PlayClip(animator, clip);
        }

        if (!hasClips)
            EditorGUILayout.LabelField("No animation clips found.", EditorStyles.miniLabel);
    }

    private static void PlayClip(Animator animator, AnimationClip clip)
    {
        StopAnimation(animator);

        var graph = PlayableGraph.Create($"TestingAnimation_{animator.name}_{clip.name}");
        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        var clipPlayable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(clipPlayable);
        graph.Play();

        activeGraphs[animator.GetInstanceID()] = graph;
    }

    private static void StopAnimation(Animator animator)
    {
        int animatorId = animator.GetInstanceID();
        if (!activeGraphs.TryGetValue(animatorId, out var graph)) return;

        if (graph.IsValid()) graph.Destroy();
        activeGraphs.Remove(animatorId);
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            StopAllAnimations();
    }

    private static void StopAllAnimations()
    {
        foreach (var graph in activeGraphs.Values)
        {
            if (graph.IsValid()) graph.Destroy();
        }

        activeGraphs.Clear();
    }

    private static void DrawParameter(Animator animator, AnimatorControllerParameter parameter)
    {
        switch (parameter.type)
        {
            case AnimatorControllerParameterType.Bool:
                var boolValue = EditorGUILayout.Toggle(parameter.name, animator.GetBool(parameter.nameHash));
                animator.SetBool(parameter.nameHash, boolValue);
                break;

            case AnimatorControllerParameterType.Int:
                var intValue = EditorGUILayout.IntField(parameter.name, animator.GetInteger(parameter.nameHash));
                animator.SetInteger(parameter.nameHash, intValue);
                break;

            case AnimatorControllerParameterType.Float:
                var floatValue = EditorGUILayout.FloatField(parameter.name, animator.GetFloat(parameter.nameHash));
                animator.SetFloat(parameter.nameHash, floatValue);
                break;

            case AnimatorControllerParameterType.Trigger:
                if (GUILayout.Button($"Trigger: {parameter.name}"))
                    animator.SetTrigger(parameter.nameHash);
                break;
        }
    }
}