using UnityEngine;
using System.Collections;

/// <summary>
/// 通用打击特效
/// </summary>

[RequireComponent(typeof(Animator))]
public class qfHitEffect : MonoBehaviour
{
	public static string ON_HIT_KEY = "onHit";

	public bool DebugOn = false;

	private Animator _animator;
	private nsHitPause _hitPause;

	private Vector3 _size;

	public void OnHit(Collider2D col, Vector3 pos)
	{
		transform.position = pos;
		_size = col.bounds.size;
		_animator.Play(ON_HIT_KEY, 0, 0f);

		_hitPause.ShowHitPause();
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
