using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qfGame
{
    [CreateAssetMenu(fileName = "qfGameEvt", menuName = "qfGameEvt/Event", order = 0)]
    public class qfGameEvt : ScriptableObject {

        readonly List<qfGameEvtListener> eventListeners = new List<qfGameEvtListener>();

        public void RaiseWithParam<T>(T evt) where T : System.EventArgs
        {
            for(int i=eventListeners.Count - 1; i>=0; i--)
            {
                eventListeners[i].OnEventRaised(evt);
            }
        }

        public void Register(qfGameEvtListener listener)
        {
            if(!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void Unregister(qfGameEvtListener listener)
        {
            if(eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}