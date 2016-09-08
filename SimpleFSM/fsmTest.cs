using UnityEngine;
using System.Collections;
using qbfox;

public class fsmTest : MonoBehaviour
{
	public enum State
	{
		Idle,
		Rotating,
	}

	private FSMSystem fsm;

	// Use this for initialization
	void Start () {
		fsm = new FSMSystem(this);
	    FSMState idle =	fsm.AddState(State.Idle.ToString(), new IdleState());
		FSMState rotating = fsm.AddState(State.Rotating.ToString(), new RotatingState());
		
		idle.MarkNextState(rotating.ID);
		rotating.MarkNextState(idle.ID);

		fsm.ChangeState(State.Idle.ToString());
	}

	void ToggleState()
	{
		fsm.CurrentState.TryChangeState(false);
		Debug.Log("State changed to " + fsm.CurrentState.ID);
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyUp(KeyCode.Space))
		{
			ToggleState();
		}

		if (!fsm.IsChanging && fsm.CurrentState != null)
		{
			fsm.CurrentState.Run();
		}
	}

	public class IdleState : FSMState
	{
		public override void Activate()
		{
			base.Activate();
			Debug.Log("Activating idle state");
		}

		public override void Run()
		{
			base.Run();
			Debug.Log("idle state running:" + Time.realtimeSinceStartup);
		}

		public override void Deactivate()
		{
			base.Deactivate();
			Debug.Log("Deactivating idle state");
		}
	}

	public class RotatingState : FSMState
	{
		public override void Activate()
		{
			base.Activate();
			Debug.Log("Activating rotate state");
		}

		public override void Run()
		{
			base.Run();
			Debug.Log("rotating state running:" + Time.realtimeSinceStartup);
		}

		public override void MarkActivatingComplete()
		{
			base.MarkActivatingComplete();
			Debug.Log("Activating rotating complete, delegate triggered");
		}

	}
}
