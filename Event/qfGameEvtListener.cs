using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qfGame
{
    public class qfGameEvtListener : MonoBehaviour
    {
        [SerializeField]
        qfGameEvt qfEvent;
        [HideInInspector]
        public UnityEngine.Events.UnityEvent<EventArgs> EvtHandlers;

        internal void OnEventRaised<T>(T evt) where T : EventArgs
        {
            EvtHandlers.Invoke(evt);
        }

        private void OnEnable() {
            qfEvent.Register(this);  
        }

        private void OnDisable() {
            qfEvent.Unregister(this);  
        }
    }
}