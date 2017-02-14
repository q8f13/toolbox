using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 简易状态机
/// </summary>

namespace qbfox
{
	public class FSMSystem
	{
		protected Dictionary<string, FSMState> m_States;
		public FSMState CurrentState { get { return m_CurrentState; } }
		private FSMState m_CurrentState = null;
		public FSMState LastState { get { return m_LastState; } }
		private FSMState m_LastState;

		private MonoBehaviour _base;
		public MonoBehaviour Base { get { return _base; } }

		protected bool m_StateIsChanging = false;
		public bool IsChanging { get { return m_StateIsChanging; }}

		/// <summary>
		/// 创建实例的时候需要一个monobehavior作为基础
		/// </summary>
		/// <param name="managerBase"></param>
		public FSMSystem (MonoBehaviour managerBase)
		{
			m_States = new Dictionary<string, FSMState>();
			_base = managerBase;
		}

		public FSMState AddState(string id, FSMState newState)
		{
			if (m_States.ContainsKey(id))
			{
				Debug.LogError(string.Format("Fail to add state with id {0}: Already exist", id));
				return null;
			}

			m_States.Add(id, newState.Create(id,this));
			return newState;
		}

		public bool IsValid(string id)
		{
			return m_States != null
					&& m_States.ContainsKey(id)
					&& m_States[id] != null;
		}

		public void ChangeState(string id, bool needAsync = false)
		{
			if (!m_States.ContainsKey(id))
			{
				Debug.LogError(string.Format("Fail to change state with id {0}: Can't find the key of state"));
				return;
			}

			if (m_CurrentState != null)
			{
				m_CurrentState.Deactivate();
				m_LastState = m_CurrentState;
			}
			m_CurrentState = m_States[id];
			m_CurrentState.Activate();
	//		m_CurrentState.OnActivatingComplete = OnActivatingCompleteHandler;
			if (needAsync)
			{
				m_CurrentState.OnActivatingComplete = state => m_StateIsChanging = false;
				m_StateIsChanging = true;
			}
		}
	}

	public abstract class FSMState 
	{
		public System.Action<FSMState> OnActivatingComplete;

		private string m_StateId;
		public string ID{get { return m_StateId; }}

		private string m_NextStateId;
		public string NextStateId { get { return m_NextStateId; } }

		protected FSMSystem m_Controller;

		public FSMState Create(string stateId, FSMSystem controller)
		{
			m_StateId = stateId;
			m_Controller = controller;
			return this;
		}

		public void MarkNextState(string stateId)
		{
			if (m_Controller == null || !m_Controller.IsValid(stateId))
			{
				Debug.LogError(string.Format("fail to mark next state: invalid id {0}", stateId));
				return;
			}

			m_NextStateId = stateId;
		}

		public virtual void Activate()
		{
			Debug.Log(string.Format("active {0} state", m_StateId));
		}

		public virtual void MarkActivatingComplete()
		{
			if (OnActivatingComplete != null)
				OnActivatingComplete(this);
		}

		public virtual void Deactivate()
		{
			Debug.Log(string.Format("deactive {0} state", m_StateId));
		}
		public virtual void Run(){}

		public void TryChangeState(bool needAsync = false)
		{
			if (m_Controller.IsChanging)
			{
				Debug.LogError(string.Format("Changing state failed: State '{0}' is still in changing state", m_Controller.CurrentState.ID));
				return;
			}

			if (!string.IsNullOrEmpty(m_NextStateId))
			{
				m_Controller.ChangeState(m_NextStateId, needAsync);
			}
		}
	}
}

