using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class FSMEventTriggerClip :PlayableAsset, ITimelineClipAsset
{
	public ClipCaps clipCaps
	{
		get
		{
			return ClipCaps.None;
		}
	}

	[SerializeField]
	private FSMEventBehaviour _template = new FSMEventBehaviour();

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<FSMEventBehaviour>.Create(graph, _template);
	}
}
