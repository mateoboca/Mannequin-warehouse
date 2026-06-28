using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> _list = new List<(T, float)>();

    public bool IsEmpty => _list.Count == 0;

    public void Enqueue(T item, float priority)
    {
        _list.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < _list.Count; i++)
        {
            if (_list[i].priority < _list[bestIndex].priority)
                bestIndex = i;
        }
        T best = _list[bestIndex].item;
        _list.RemoveAt(bestIndex);
        return best;
    }
}