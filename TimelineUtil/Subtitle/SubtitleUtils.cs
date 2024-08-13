using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// collect all characters in selected TimelineAsset
// for creating TMP font characters
// generated TextAsset path should be noticed

namespace qbfox.TimelineExtend
{
	public class SubtitleUtils
	{
		[MenuItem("Custom/Timeline/GenerateAllTextInTextPlayableAsset")]
		static void GenerateAllTextInTextPlayableAsset(){
			GameObject selected = Selection.activeGameObject;
			PlayableDirector director = selected.GetComponent<PlayableDirector>();
			if(director == null)
				return;
			TimelineAsset a = director.playableAsset as TimelineAsset;
			IEnumerable<TrackAsset> tracks = a.GetOutputTracks();
			string strCollection = "";
			foreach(TrackAsset ta in tracks)
			{
				if(ta is Timeline.Samples.TextTrack)
				{
					// Debug.Log(ta.name);
					IEnumerable<TimelineClip> clips = ta.GetClips();
					foreach(TimelineClip c in clips)
					{
						// Debug.Log(c.displayName);
						Timeline.Samples.TextPlayableAsset tpa = c.asset as Timeline.Samples.TextPlayableAsset;
						// Debug.Log(tpa.template.text);
						strCollection += tpa.template.text;
						// foreach(string s in c.exposedParameters)
						//     Debug.Log(s);
					}
				}
			}

			if(strCollection.Length > 0)
			{
				// validated string data, create textAsset for TMP Font generator
				TextAsset ta = new TextAsset(strCollection);
				const string assetPath =  "Assets/UI/subTitleTexts.asset";
				AssetDatabase.CreateAsset(ta,assetPath);
				Debug.Log(string.Format("SubTitle with {0} characters saved to {1}", strCollection.Length, assetPath));
			}
			Debug.Log("No character collected, ignore this");
		}
	}
}