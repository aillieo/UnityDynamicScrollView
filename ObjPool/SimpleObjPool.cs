//#define SIMPLE_OBJ_POOL_SAFE_MODE
using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public class SimpleObjPool<T> where T : new()
    {

        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly Func<T> m_ctor;
        private readonly Action<T> m_OnRecycle;
        private int m_Size;
        private int m_UsedCount;


        public SimpleObjPool(int max = 5, Action<T> actionOnReset = null, Func <T> ctor = null)
        {
            m_Size = max;
            m_OnRecycle = actionOnReset;
            m_ctor = ctor;
        }


        public T Get()
        {
            T item;
            if (m_Stack.Count == 0)
            {
                if(null != m_ctor)
                {
                    item = m_ctor();
                }
                else
                {
                    item = new T();
                }
            }
            else
            {
                item = m_Stack.Pop();
            }
            m_UsedCount++;
            return item;
        }

        public void Recycle(T item)
        {
#if SIMPLE_OBJ_POOL_SAFE_MODE
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), item))
            {
                UnityEngine.Debug.LogError("Recycle failed ...  already recycled");
                return;
            }
#endif
            if (null != m_OnRecycle)
            {
                m_OnRecycle(item);
            }
            if(m_Stack.Count < m_Size)
            {
                m_Stack.Push(item);
            }
            m_UsedCount -- ;
        }


        public void Purge()
        {
            // TODO
        }


        public override string ToString()
        {
            return string.Format("SimpleObjPool: item=[{0}], inUse=[{1}], restInPool=[{2}/{3}] ", typeof(T), m_UsedCount, m_Stack.Count, m_Size);
        }

    }
}
