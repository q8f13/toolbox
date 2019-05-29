using UnityEngine;

public class Spawner : MonoBehaviour, ISpawner {
	public GameObject TemplatePrefab;

	public T SpawnNew<T>()
	{
		GameObject g = Instantiate(TemplatePrefab).gameObject;
		T t = g.GetComponent<T>();
		return t;
	}
}
