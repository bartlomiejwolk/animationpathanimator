﻿using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using ATP.LoggingTools;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorEventsComponent {

    [RequireComponent(typeof (AnimationPathAnimator))]
    public class AnimatorEvents : MonoBehaviour {
        #region FIELDS

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private AnimationPathAnimator animator;

#pragma warning disable 0414 
        [SerializeField]
        private bool drawMethodNames = true;
#pragma warning restore 0414 

        [SerializeField]
        private List<NodeEventSlot> nodeEventSlots;

        [SerializeField]
        private AnimatorEventsSettings settings;

        [SerializeField]
        private GUISkin skin;

        #endregion

        #region PROPERTIES

        public AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public AnimatorEventsSettings Settings {
            get { return settings; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public List<NodeEventSlot> NodeEventSlots {
            get { return nodeEventSlots; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnDisable() {
            UnsubscribeFromAnimatorEvents();
            UnsubscribeFromPathEvents();
        }

        private void UnsubscribeFromAnimatorEvents() {
            // Guard agains null reference.
            if (Animator == null) Animator = GetComponent<AnimationPathAnimator>();

            Animator.NodeReached -= Animator_NodeReached;
            Animator.PathDataRefChanged -= Animator_PathDataRefChanged;
            Animator.UndoRedoPerformed -= Animator_UndoRedoPerformed;
        }

        private void OnEnable() {
            Debug.Log("OnEnable()");
            if (Animator == null) return;

            UnsubscribeFromAnimatorEvents();
            SubscribeToAnimatorEvents();
        }

        private void OnValidate() {
            Logger.LogCall();
            UnsubscribeFromAnimatorEvents();
            SubscribeToAnimatorEvents();
            UnsubscribeFromPathEvents();
            SubscribeToPathEvents();
        }

        void PathData_NodeRemoved(object sender, NodeAddedRemovedEventArgs e) {
            NodeEventSlots.RemoveAt(e.NodeIndex);
        }

        void PathData_NodeAdded(object sender, NodeAddedRemovedEventArgs e) {
            Debug.Log("NodeAdded event");
            NodeEventSlots.Insert(e.NodeIndex, new NodeEventSlot());
        }

        private void Reset() {
            Animator = GetComponent<AnimationPathAnimator>();
            nodeEventSlots = new List<NodeEventSlot>();

            InitializeSlots();
            LoadRequiredResources();
            UnsubscribeFromAnimatorEvents();
            SubscribeToAnimatorEvents();
        }

        private void InitializeSlots() {
            if (Animator.PathData == null) return;

            // Get number of nodes in the path.
            var nodesNo = Animator.PathData.NodesNo;

            // Calculate how many slots to add/remove.
            var slotsDiff = NodeEventSlots.Count - nodesNo;

            if (slotsDiff > 0) {
                // Remove slots.
                for (int i = 0; i < slotsDiff; i++) {
                    NodeEventSlots.RemoveAt(NodeEventSlots.Count - 1);
                }
            }
            else {
                // Add slots
                for (int i = 0; i < Mathf.Abs(slotsDiff); i++) {
                    NodeEventSlots.Add(new NodeEventSlot());
                }
            }

            Utilities.Assert(
                () => nodesNo == NodeEventSlots.Count,
                string.Format("Number of nodes ({0}) in the path and event slots ({1}) differ.",
                nodesNo,
                NodeEventSlots.Count));
        }

        private void LoadRequiredResources() {
            settings =
                Resources.Load<AnimatorEventsSettings>("DefaultAnimatorEventsSettings");
            skin = Resources.Load("DefaultAnimatorEventsSkin") as GUISkin;
        }

        private void SubscribeToAnimatorEvents() {
            // Guard agains null reference.
            if (Animator == null) Animator = GetComponent<AnimationPathAnimator>();

            Animator.NodeReached += Animator_NodeReached;
            Animator.PathDataRefChanged += Animator_PathDataRefChanged;
            Animator.UndoRedoPerformed += Animator_UndoRedoPerformed;
        }


        void Animator_UndoRedoPerformed(object sender, System.EventArgs e) {
            // During animator undo event, reference to path data could have been changed.
            //HandlePathDataRefChange();
        }

        void Animator_PathDataRefChanged(object sender, System.EventArgs e) {
            HandlePathDataRefChange();
        }

        private void HandlePathDataRefChange() {
            if (Animator.PathData == null) {
                UnsubscribeFromPathEvents();
            }
            else {
                UnsubscribeFromPathEvents();
                SubscribeToPathEvents();
                InitializeSlots();
            }
        }

        private void SubscribeToPathEvents() {
            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded += PathData_NodeAdded;
                Animator.PathData.NodeRemoved += PathData_NodeRemoved;
                Animator.PathData.NodePositionChanged += PathData_NodePositionChanged;
            }
        }

        void PathData_NodePositionChanged(object sender, System.EventArgs e) {
            AssertSlotsInSyncWithPath();
        }

        private void AssertSlotsInSyncWithPath() {
            Utilities.Assert(
                () =>
                    Animator.PathData.NodesNo
                    == NodeEventSlots.Count,
                string.Format(
                    "Path nodes number ({0}) and event slots number ({1}) differ.",
                    Animator.PathData.NodesNo,
                    NodeEventSlots.Count));
        }


        private void UnsubscribeFromPathEvents() {
            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded -= PathData_NodeAdded;
                Animator.PathData.NodeRemoved -= PathData_NodeRemoved;
                Animator.PathData.NodePositionChanged -= PathData_NodePositionChanged;
            }
        }

        #endregion

        #region EVENT HANDLERS

        private void Animator_NodeReached(
            object sender,
            NodeReachedEventArgs arg) {

            // Get event slot.
            var nodeEvent = NodeEventSlots[arg.NodeIndex];
            // Return if source GO was not specified in the event slot.
            if (nodeEvent.SourceGO == null) return;
            // Get method metadata.
            var methodInfo = nodeEvent.SourceCo.GetType()
                .GetMethod(nodeEvent.SourceMethodName);
            // Return if method info couldn't be loaded.
            if (methodInfo == null) return;
            // Get method parameters.
            var methodParams = methodInfo.GetParameters();

            // Method has no parameters.
            if (methodParams.Length == 0) {
                // Invoke method.
                methodInfo.Invoke(nodeEvent.SourceCo, null);
            }
            // Method has one parameter.
            else if (methodParams.Length == 1) {
                // Return if the parameter is not a string.
                if (methodParams[0].ParameterType.Name != "String") return;
                // Create string parameter argument.
                var stringParam = new object[] { nodeEvent.MethodArg };
                // Invoke method with string parameter.
                methodInfo.Invoke(nodeEvent.SourceCo, stringParam);
            }
        }

        #endregion

        #region METHODS

        private bool RequiredAssetsLoaded() {
            if (Settings != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        private string[] GetMethodNames() {
            // Return empty array if slots list was not yet initalized.
            if (NodeEventSlots == null) return new string[0];

            var methodNames = new string[NodeEventSlots.Count];

            for (var i = 0; i < NodeEventSlots.Count; i++) {
                methodNames[i] = NodeEventSlots[i].SourceMethodName;
            }

            return methodNames;
        }

        private List<Vector3> GetNodePositions(int nodesNo) {
            var nodePositions =
                Animator.GetGlobalNodePositions(nodesNo);

            return nodePositions;
        }

        #endregion

    }

}