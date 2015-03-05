﻿using ATP.SimplePathAnimator.Animator;
using ATP.SimplePathAnimator.PathEvents;
using UnityEditor;

namespace ATP.SimplePathAnimator {

    // TODO Specify name for newly created asset.
    public class AssetCreator {

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path")]
        private static void CreatePathAsset() {
            ScriptableObjectUtility.CreateAsset<PathData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Events")]
        private static void CreateAnimatorEventsDataAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorEventsData>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Animator Settings")]
        private static void CreateAnimatorSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<AnimatorSettings>();
        }

        [MenuItem("Assets/Create/ATP/SimplePathAnimator/Path Events Settings")]
        private static void CreatePathEventsSettingsAsset() {
            ScriptableObjectUtility.CreateAsset<PathEventsSettings>();
        }
    }

}