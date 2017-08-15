using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 伪场景切换
/// </summary>
public class FakeSceneSwitcher : MonoBehaviour
{
	public const string ROOT_GO_NAME = "_container";

	[Tooltip("起始场景的Index")]
	public int StartSceneIndex = 0;

	private int _idx = -1;	

	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	void Start()
	{
		InitialSetup();
	}

	void InitialSetup()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if( i == StartSceneIndex)
				continue;
			PackGoInScene(SceneManager.GetSceneAt(i));
		}

		SceneManager.SetActiveScene(SceneManager.GetSceneAt(StartSceneIndex));
	}

	void PackGoInScene(Scene s)
	{
		GameObject[] prevGos = s.GetRootGameObjects();
		GameObject root = new GameObject(ROOT_GO_NAME);
		for (int i = 0; i < prevGos.Length; i++)
		{
			prevGos[i].transform.parent = root.transform;
		}

		root.SetActive(false);
	}

	[ContextMenu("NextScene")]
	public void NextScene()
	{
		PackGoInScene(SceneManager.GetActiveScene());

		if (++_idx > SceneManager.sceneCount - 1)
			_idx = 0;

		SceneManager.SetActiveScene(SceneManager.GetSceneAt(_idx));

		GameObject[] newGos = SceneManager.GetActiveScene().GetRootGameObjects();
		if (newGos.Length == 1 && newGos[0].name == ROOT_GO_NAME)
		{
			newGos[0].transform.DetachChildren();
			Destroy(newGos[0]);
		}
	}
}
