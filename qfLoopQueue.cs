﻿using System;
using Object = UnityEngine.Object;

/// <summary>
/// 可遍历的循环队列
/// from: http://www.cs.bu.edu/teaching/c/queue/array/types.html
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
			_rear = _rear >= _count ? 0 : _rear;
		}

		public T Dequeue()
		{
			if(ElementCount() == 0)
				throw new Exception("queue is empty");
			T result = _array[_front];
			_array[_front--] = default(T);
			_front = _front < 0 ? _count - 1 : _front;
			return result;
		}

		public T GetElementAtIdx(int index)
		{
			if(index < 0 || index >= _count)
				throw new Exception("index is out of bound");
		
			int idx = _front + index;
			if (idx > _count - 1)
				idx -= _count; 
			return _array[idx];
		}

		public bool IsFull()
		{
			if (_front == 0 && _rear == _count - 1)
				return true;

			if (_rear == _front - 1)
				return true;

			return false;
		}

		public void Clear()
		{
			_front = 0;
			_rear = 0;
			_array = new T[_count];
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
