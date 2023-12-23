// -----------------------------------------------------------------------
// <copyright file="SimpleObjPool.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;

    public class SimpleObjPool<T>
    {
        private readonly Stack<T> stack;
        private readonly Func<T> ctor;
        private readonly Action<T> onRecycle;
        private readonly Action<T> dtor;
        private int size;
        private int usedCount;

        public SimpleObjPool(int max = 5, Action<T> onRecycle = null, Func<T> ctor = null, Action<T> dtor = null)
        {
            this.stack = new Stack<T>(max);
            this.size = max;
            this.onRecycle = onRecycle;
            this.ctor = ctor;
            this.dtor = dtor;
        }

        public T Get()
        {
            T item;
            if (this.stack.Count == 0)
            {
                if (this.ctor != null)
                {
                    item = this.ctor();
                }
                else
                {
                    item = Activator.CreateInstance<T>();
                }
            }
            else
            {
                item = this.stack.Pop();
            }

            this.usedCount++;
            return item;
        }

        public void Recycle(T item)
        {
            if (this.onRecycle != null)
            {
                this.onRecycle.Invoke(item);
            }

            if (this.stack.Count < this.size)
            {
                this.stack.Push(item);
            }
            else
            {
                if (this.dtor != null)
                {
                    this.dtor.Invoke(item);
                }
            }

            this.usedCount--;
        }

        public void Purge()
        {
            while (this.stack.Count > 0)
            {
                var item = this.stack.Pop();
                if (this.dtor != null)
                {
                    this.dtor.Invoke(item);
                }
            }
        }

        public override string ToString()
        {
            return $"SimpleObjPool: item=[{typeof(T)}], inUse=[{this.usedCount}], restInPool=[{this.stack.Count}/{this.size}] ";
        }
    }
}
