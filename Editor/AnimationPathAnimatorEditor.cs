﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(AnimationPathAnimator))]
	public class AnimatorEditor: Editor {

        #region FIELDS
        /// <summary>
        /// Reference to target script.
        /// </summary>
		private AnimationPathAnimator script;

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

        #endregion
        #region SERIALIZED PROPERTIES
        // Serialized properties
		private SerializedProperty duration;
		private SerializedProperty rotationSpeed;
		private SerializedProperty animTimeRatio;
        private SerializedProperty easeAnimationCurve;
        private SerializedProperty zAxisRotationCurve;
        private SerializedProperty animatedObject;
        private SerializedProperty animatedObjectPath;
        private SerializedProperty followedObject;
        private SerializedProperty followedObjectPath;
        #endregion
        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        void OnEnable() {
            // Get target script reference.
			script = (AnimationPathAnimator)target;

            // Initialize serialized properties.
			duration = serializedObject.FindProperty("duration");
		    rotationSpeed = serializedObject.FindProperty("rotationSpeed");
			animTimeRatio = serializedObject.FindProperty("animTimeRatio");
		    easeAnimationCurve = serializedObject.FindProperty("easeCurve");
		    zAxisRotationCurve = serializedObject.FindProperty("zAxisRotationCurve");
		    animatedObject = serializedObject.FindProperty("animatedObject");
		    animatedObjectPath = serializedObject.FindProperty("animatedObjectPath");
		    followedObject = serializedObject.FindProperty("animatedObject");
		    followedObjectPath = serializedObject.FindProperty("followedObjectPath");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Slider(
					animTimeRatio,
					0,
					1);

			EditorGUILayout.PropertyField(duration);
			EditorGUILayout.PropertyField(rotationSpeed);

		    EditorGUILayout.PropertyField(
		        easeAnimationCurve,
		        new GUIContent(
		            "Ease Curve",
		            ""));

		    EditorGUILayout.PropertyField(
		        zAxisRotationCurve,
		        new GUIContent(
		            "Tilting Curve",
		            ""));

            EditorGUILayout.Space();

		    EditorGUILayout.PropertyField(
		        animatedObject,
		        new GUIContent(
		            "Object",
		            ""));

		    EditorGUILayout.PropertyField(
                animatedObjectPath,
		        new GUIContent(
		            "Object Path",
		            ""));

		    EditorGUILayout.PropertyField(
                followedObject,
		        new GUIContent(
		            "Target",
		            ""));

		    EditorGUILayout.PropertyField(
                followedObjectPath,
		        new GUIContent(
		            "Target Path",
		            ""));

			// Save changes
			serializedObject.ApplyModifiedProperties();
		}

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        void OnSceneGUI() {
			serializedObject.Update();

            // Update modifier key state.
			UpdateModifierKey();

            // Change current animation time with arrow keys.
			ChangeTimeWithArrowKeys();

			// Save changes
			serializedObject.ApplyModifiedProperties();

		    script.UpdateAnimation();
		}
        #endregion
        #region PRIVATE METHODS
        /// <summary>
        /// Change current animation time with arrow keys.
        /// </summary>
		private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
			if (Event.current.type == EventType.keyDown
                    // and modifier key is pressed also..
					&& modKeyPressed) {

			    HandleModifiedShortcuts();
			}
			// Modifier key not pressed.
			else if (Event.current.type == EventType.keyDown) {
                HandleUnmodifiedShortcuts();
			}

		
		}

        private void HandleUnmodifiedShortcuts() {
// Helper variable.
            float newAnimationTimeRatio;
            switch (Event.current.keyCode) {
                // Jump backward.
                case AnimationPathAnimator.JumpBackward:
                    Event.current.Use();

                    // Calculate new time ratio.
                    newAnimationTimeRatio = animTimeRatio.floatValue
                                            - AnimationPathAnimator.ShortJumpValue;
                    // Apply rounded value.
                    animTimeRatio.floatValue =
                        (float) (Math.Round(newAnimationTimeRatio, 3));

                    break;
                // Jump forward.
                case AnimationPathAnimator.JumpForward:
                    Event.current.Use();

                    newAnimationTimeRatio = animTimeRatio.floatValue
                                            + AnimationPathAnimator.ShortJumpValue;
                    animTimeRatio.floatValue =
                        (float) (Math.Round(newAnimationTimeRatio, 3));

                    break;
                case AnimationPathAnimator.JumpToStart:
                    Event.current.Use();

                    animTimeRatio.floatValue = 1;

                    break;
                case AnimationPathAnimator.JumpToEnd:
                    Event.current.Use();

                    animTimeRatio.floatValue = 0;

                    break;
            }
        }

        private void HandleModifiedShortcuts() {
// Check what key is pressed..
            switch (Event.current.keyCode) {
                // Jump backward.
                case AnimationPathAnimator.JumpBackward:
                    Event.current.Use();

                    // Update animation time.
                    animTimeRatio.floatValue -=
                        AnimationPathAnimator.JumpValue;

                    break;
                // Jump forward.
                case AnimationPathAnimator.JumpForward:
                    Event.current.Use();

                    // Update animation time.
                    animTimeRatio.floatValue +=
                        AnimationPathAnimator.JumpValue;

                    break;
                case AnimationPathAnimator.JumpToStart:
                    Event.current.Use();

                    // Jump to next node.
                    animTimeRatio.floatValue = GetNearestNodeForwardTimestamp();

                    break;
                case AnimationPathAnimator.JumpToEnd:
                    Event.current.Use();

                    // Jump to next node.
                    animTimeRatio.floatValue = GetNearestNodeBackwardTimestamp();

                    break;
            }
        }

        // TODO Rename to GetNearestForwardNodeTimestamp().
        private float GetNearestNodeForwardTimestamp() {
            var targetPathTimestamps = script.GetTargetPathTimestamps();

            foreach (var timestamp in targetPathTimestamps
                .Where(timestamp => timestamp > animTimeRatio.floatValue)) {

                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        // TODO Rename to GetNearestBackwardNodeTimestamp().
        private float GetNearestNodeBackwardTimestamp() {
            var targetPathTimestamps = script.GetTargetPathTimestamps();

            for (var i = targetPathTimestamps.Length - 1; i >= 0; i--) {
                if (targetPathTimestamps[i] < animTimeRatio.floatValue) {
                    return targetPathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        /// <summary>
        ///     Checked if modifier key is pressed and remember it in a class
        ///     field.
        /// </summary>
        private void UpdateModifierKey() {
            // Check if modifier key is currently pressed.
            if (Event.current.type == EventType.keyDown
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                // Remember key state.
                modKeyPressed = true;
            }
            // If modifier key was released..
            if (Event.current.type == EventType.keyUp
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                modKeyPressed = false;
            }
        }
        #endregion
    }
}
