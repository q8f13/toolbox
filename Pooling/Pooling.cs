using System.Collections.Generic;

public class Pooling{
	private Stack<IPoolable> _stack;

	private ISpawner _spawner;

	public Pooling(int startSize, ISpawner spawner, bool prewarm)
	{
		_stack = new Stack<IPoolable>(startSize);
		_spawner = spawner;

		if(prewarm)
		{
			int count = startSize;
			while(count > 0)
			{
				PoolIn(_spawner.SpawnNew<IPoolable>());
				count--;
			}
		}
	}

	public void PoolIn(IPoolable item)
	{
		item.Deactive();
		_stack.Push(item);
	}

	public IPoolable PoolOut()
	{
		IPoolable item = null;
		if(_stack.Count == 0)
			item = _spawner.SpawnNew<IPoolable>();
		else
			item = _stack.Pop();

		item.Active();

		return item;
	}
}

public interface ISpawner
{
	T SpawnNew<T>();
}

public interface IPoolable
{
	void Active();
	void Deactive();
}