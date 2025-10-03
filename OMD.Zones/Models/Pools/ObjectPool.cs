using System.Collections.Generic;

namespace OMD.Zones.Models.Pools;

public abstract class ObjectPool<T>
{
    private readonly Stack<T> _pool = [];

    public T Rent()
    {
        lock (_pool)
        {
            if (_pool.Count > 0)
                return _pool.Pop();
        }

        return GenerateDefault();
    }

    public void Return(T value)
    {
        Reset(value);

        lock (_pool)
        {
            _pool.Push(value);
        }
    }

    protected abstract T GenerateDefault();

    protected abstract void Reset(T value);
}
