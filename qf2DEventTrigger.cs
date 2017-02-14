using UnityEngine;
using System.Collections;

/// <summary>
/// 事件触发器
/// 配合qf2DEventHandler使用
/// 触发器激活后handler将触发情况以delegate形式传递出去
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class qf2DEventTrigger : MonoBehaviour, LevelEvent.IActiveSth
{
	public bool OneTimeTriggerEnter = false;
	public bool OneTimeTriggerExit = false;
	public string TriggerTag;
	public qf2DEventHandler[] TriggerTargets;
	public GameObject[] WhiteListedGo;

    public bool AlreadyForActive = true;

	private Collider2D _collider2D;

    private bool _haveValidCollider = false;

	void Start()
	{
	    _collider2D = GetComponent<Collider2D>();
	    _haveValidCollider = _collider2D != null;
        if(!_haveValidCollider)
            Debug.LogError("collider not ready");
	}

    public void ToggleFreeze(bool on)
    {
        _collider2D.enabled = on;
    }

	public void OnTriggerEnter2D(Collider2D collision)
	{
	    if (!_haveValidCollider)
	        return;

		if (!AlreadyForActive)
		{
			Debug.Log("not ready for active");
			return;
		}

		if (WhiteListedGo.Length > 0 && System.Array.IndexOf(WhiteListedGo, collision.gameObject) < 0)
			return;

		if(string.IsNullOrEmpty(TriggerTag))
		{
			Debug.LogError("invalid trigger tag");
			return;
		}

		if(collision.tag != TriggerTag)
			return;

		ActivateTriggers(collision, TriggerType.Enter);

	}

	public void OnTriggerExit2D(Collider2D collision)
	{
        if(!_haveValidCollider)
            return;

		if (WhiteListedGo.Length > 0 && System.Array.IndexOf(WhiteListedGo, collision.gameObject) < 0)
			return;

		if(string.IsNullOrEmpty(TriggerTag))
		{
			Debug.LogError("invalid trigger tag");
			return;
		}

		if(collision.tag != TriggerTag)
			return;
		ActivateTriggers(collision, TriggerType.Exit);

		if (OneTimeTriggerExit)
		{
			_collider2D.enabled = false;
			Destroy(gameObject);
		}
	}

	void ActivateTriggers(Collider2D col, TriggerType type)
	{
		for (int i = 0; i < TriggerTargets.Length; i++)
			TriggerTargets[i].TriggerActivate(col, type);

		if (OneTimeTriggerEnter)
		{
			_collider2D.enabled = false;
			Destroy(gameObject);
		}
	}

	void OnDrawGizmos()
	{
	    if (_collider2D == null)
	        _collider2D = GetComponent<Collider2D>();

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_collider2D.transform.position, _collider2D.bounds.size);
//		Gizmos.DrawWireCube(transform.position, _collider2D.bounds.size);

		if (TriggerTargets != null)
		{
			Gizmos.color = Color.magenta;
			for (int i = 0; i < TriggerTargets.Length; i++)
				Gizmos.DrawLine(transform.position, TriggerTargets[i].transform.position);
		}
	}

	public enum TriggerType
	{
		Enter = 0,
		Stay,
		Exit,
	}

	public void Toggle(bool on)
	{
//		throw new System.NotImplementedException();
		AlreadyForActive = on;
	}
}

public interface IqfEventTriggerHandler
{
	bool TriggerActivate(Collider2D collider, qf2DEventTrigger.TriggerType type);
	bool TriggerActivate(Collider collider, qf2DEventTrigger.TriggerType type);
}