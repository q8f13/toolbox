using System;
using Object = UnityEngine.Object;

/// <summary>
/// 可遍历的循环队列
/// </summary>
/// <typeparam name="T"></typeparam>

namespace qbfox
{
	public class qfLoopQueue<T> : Object
	{
		private int _count;
		private T[] _array;

		private int _front;
		private int _rear;

		public qfLoopQueue(int count)
		{
			_count = count;
			_array = new T[_count];
			_front = 0;
			_rear = 0;
		}

		public void Enqueue(T value)
		{
			if (IsFull())
				throw new Exception("queue is full");
			_array[_rear++] = value;
		}

		public T Dequeue()
		{
			if(ElementCount() == 0)
				throw new Exception("queue is empty");
			T result = _array[_front];
			_array[_front--] = default(T);
			return result;
		}

		public T GetElementAtIdx(int index)
		{
			if(index < 0 || index >= _count)
				throw new Exception("index is out of bound");
		
			int idx = _front + index;
			if (idx > _count - 1)
				idx -= _count; 
			return _array[_front + idx];
		}

		public bool IsFull()
		{
			if (_front == 0 && _rear == _count - 1)
				return true;

			if (_rear == _front - 1)
				return true;

			return false;
		}

		public int ElementCount()
		{
			if (_front == _rear)
				return 0;

			if (_front < _rear)
				return _rear - _front + 1;
			else
				return _count - (_front - _rear) + 1;
		}

		public int Length()
		{
			return _count;
		}
	}
}
