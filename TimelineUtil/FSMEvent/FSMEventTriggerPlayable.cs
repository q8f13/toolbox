using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class FSMEventBehaviour : PlayableBehaviour
{
	public string EventKey;

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		Bootloader.Instance.SendFSMEvent(EventKey);
	}
}
