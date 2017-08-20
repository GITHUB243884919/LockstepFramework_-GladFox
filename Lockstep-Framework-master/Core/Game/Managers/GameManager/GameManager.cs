﻿using Lockstep;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TypeReferences;
using System;
namespace Lockstep {
    public abstract class GameManager : MonoBehaviour{

        [SerializeField]
        BehaviourHelper[] _helpers;
        BehaviourHelper[] Helpers {get {return _helpers;}}


        public static GameManager Instance { get; private set; }


        string replayLoadScene;
        static int hashFrame;
        static long prevHash;
        static long stateHash;
        static bool hashChecked;

        public abstract NetworkHelper MainNetworkHelper {
            get;
        }

        private static RTSInterfacingHelper _defaultHelper = new RTSInterfacingHelper();
        public virtual InterfacingHelper MainInterfacingHelper {
            get {
                return _defaultHelper;
            }
        }

        public void ScanForHelpers () {
            //Currently deterministic but not guaranteed by Unity
            _helpers = this.gameObject.GetComponents<BehaviourHelper> ();
        }

        public virtual void GetBehaviourHelpers (FastList<BehaviourHelper> output) {
            //if (Helpers == null)
                ScanForHelpers ();
            if (Helpers != null) {
                for (int i = 0; i < Helpers.Length; i++) {
                    output.Add(Helpers[i]);
                }
            }
        }
    
        protected void Start () {
            Instance = this;
            LockstepManager.Initialize (this);
            this.Startup();
        }

        protected virtual void Startup () {

        }
    

        protected void FixedUpdate () {
            LockstepManager.Simulate ();
            if (ReplayManager.IsPlayingBack) {
                if (hashChecked == false) {
                    if (LockstepManager.FrameCount == hashFrame) {
                        hashChecked = true;
                        long newHash = AgentController.GetStateHash ();
                        if (newHash != prevHash) {
                            Debug.Log ("Desynced!");
                        } else {
                            Debug.Log ("Synced!");
                        }
                    }
                }
            } else {
                hashFrame = LockstepManager.FrameCount - 1;
                prevHash = stateHash;
                stateHash = AgentController.GetStateHash ();
                hashChecked = false;
            }
        }
    
        private float timeToNextSimulate;

        protected void Update () {
            timeToNextSimulate -= Time.smoothDeltaTime * Time.timeScale;
            if (timeToNextSimulate <= float.Epsilon) {
                timeToNextSimulate = LockstepManager.BaseDeltaTime;
            }
            LockstepManager.Visualize ();
            CheckInput ();
        }
    
        protected virtual void CheckInput () {
        
        }

        void LateUpdate () {
            LockstepManager.LateVisualize ();
        }

        public static void GameStart () {
            Instance.OnGameStart ();
        }

        protected virtual void OnGameStart () {
            //When the game starts (first simulation frame)
        }
    
        void OnDisable () {
            //LockstepManager.Deactivate ();
        }

        void OnApplicationQuit () {
            LockstepManager.Quit ();
        }
    
        void OnGUI () {
        
            if (CommandManager.sendType == SendState.Network) {
                return;
            }
            if (ReplayManager.IsPlayingBack) {
                if (GUILayout.Button ("Play")) {
                    ReplayManager.Stop ();
                    Application.LoadLevel (Application.loadedLevel);
                }
            } else {
                if (GUILayout.Button ("Replay")) {
                    ReplayManager.Save ();
                    ReplayManager.Play ();
                    Application.LoadLevel (Application.loadedLevel);
                }
            }
        }
    }
}