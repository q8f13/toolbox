using UnityEngine;
using System.Collections;

/// <summary>
/// 通用打击特效
/// </summary>

[RequireComponent(typeof(Animator))]
public class qfHitEffect : MonoBehaviour
{
	public static string ON_HIT_KEY = "onHit";
	public static string ON_DEFEND_KEY = "onDefendBlock";

	public bool DebugOn = false;

	private Animator _animator;
	private nsHitPause _hitPause;

	private Vector3 _size;

	public void OnHit(Collider2D col, Vector3 pos)
	{
		UpdateBound(col, pos);
		Debug.DrawLine(pos, col.transform.parent.position, Color.yellow, 5.0f);
		_animator.Play(ON_HIT_KEY, 0, 0f);

		// 击中位置到攻击者重心划线，作为旋转角度。攻击者重心为原点
		if (pos.y < col.bounds.center.y)
			transform.localRotation = Quaternion.Euler(0, 0, 45);
		else
			transform.localRotation = Quaternion.Euler(0, 0, -45);

		_hitPause.ShowHitPause();
	}

	public void OnDefend(Collider2D col, Vector3 pos)
	{
		UpdateBound(col, pos);
		_animator.Play(ON_DEFEND_KEY, 0, 0f);
	}

	void UpdateBound(Collider2D col, Vector3 pos)
	{
		transform.position = pos;
		_size = col.bounds.size;
	}

	void Start()
	{
		_animator = GetComponent<Animator>();
		_hitPause = GetComponent<nsHitPause>();
	}

	void OnDrawGizmos()
	{
		if (!DebugOn)
			return;

		if (_animator != null && _animator.GetCurrentAnimatorStateInfo(0).IsName(ON_HIT_KEY))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(transform.position, _size);
		}
	}
}

