using UnityEngine;
using System.Collections;

/// <summary>
/// 事件触发器
/// 配合qf2DEventTrigger使用
/// trigger激活后handler将触发情况以delegate形式传递出去
/// </summary>
public class qf2DEventHandler : MonoBehaviour, IqfEventTriggerHandler
{
	public System.Action<Collider2D, qf2DEventTrigger.TriggerType> OnTrigger2DActivateAction;
	public System.Action<Collider, qf2DEventTrigger.TriggerType> OnTriggerActivateAction;

	public bool TriggerActivate(Collider2D collider, qf2DEventTrigger.TriggerType type)
	{
//		throw new System.NotImplementedException();
		if (OnTrigger2DActivateAction != null)
		{
			OnTrigger2DActivateAction(collider, type);
			return true;
		}

		return false;
	}

	public bool TriggerActivate(Collider collider, qf2DEventTrigger.TriggerType type)
	{
		if (OnTriggerActivateAction != null)
		{
			OnTriggerActivateAction(collider, type);
			return true;
		}

		return false;
	}
}
