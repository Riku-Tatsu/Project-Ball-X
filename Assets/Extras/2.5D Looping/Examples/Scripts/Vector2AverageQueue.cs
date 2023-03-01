using System.Collections.Generic;
using UnityEngine;


namespace Kamgam.Looping25D.Helpers
{
    public class Vector2AverageQueue
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; } = 0;
        public float AverageX { get; private set; } = 0;
        public float AverageY { get; private set; } = 0;
        protected float[] xValues;
        protected float[] yValues;
        protected int lastIndex = -1;
        protected int nextIndex = 0;

        public Vector2AverageQueue(int capacity)
        {
            Capacity = capacity;
            xValues = new float[capacity];
            yValues = new float[capacity];
            Count = 0;
        }

        /// <summary>
        /// Enqueues and calcs the average.
        /// </summary>
        /// <param name="vector"></param>
        public void Enqueue(Vector2? vector)
        {
            if (vector.HasValue)
            {
                Enqueue(vector.Value.x, vector.Value.y);
            }
        }

        /// <summary>
        /// Enqueues
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="updateAverage"></param>
        public void Enqueue(Vector2 vector, bool updateAverage = true)
        {
            Enqueue(vector.x, vector.y, updateAverage);
        }

        /// <summary>
        /// Enqueues
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="updateAverage"></param>
        public void Enqueue(float x, float y, bool updateAverage = true)
        {
            xValues[nextIndex] = x;
            yValues[nextIndex] = y;
            lastIndex = nextIndex;
            nextIndex = (nextIndex + 1) % Capacity;

            if (Count < Capacity)
            {
                Count++;
            }

            if(updateAverage)
            {
                UpdateAverage();
            }
        }

        public void Dequeue()
        {
            if (Count > 0)
            {
                Count--;
            }
        }

        public void UpdateAverage()
        {
            UpdateAverage(Count);
        }

        public void UpdateAverage(int numOfEntriesToAverage)
        {
            if(numOfEntriesToAverage == 0)
            {
                AverageX = 0;
                AverageY = 0;
                return;
            }

            numOfEntriesToAverage = Mathf.Min(Count, numOfEntriesToAverage);

            float xSum = 0;
            float ySum = 0;
            int idx;
            for (int i = 0; i < numOfEntriesToAverage; i++)
            {
                idx = ((nextIndex - numOfEntriesToAverage + i) % Capacity + Capacity) % Capacity; // mod(..) inlined
                xSum += xValues[idx];
                ySum += yValues[idx];
            }
            AverageX = xSum / numOfEntriesToAverage;
            AverageY = ySum / numOfEntriesToAverage;
        }

        /// <summary>
        /// Transforms negative k to a positive index.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns>Results are positive, even for negative k.</returns>
        protected int mod(int k, int n)
        {
            return (k % n + n) % n;
        }

        public Vector2 Average()
        {
            return new Vector2(AverageX, AverageY);
        }

        public Vector2 Last()
        {
            return new Vector2(xValues[lastIndex], yValues[lastIndex]);
        }

        public float LastX()
        {
            return xValues[lastIndex];
        }

        public float LastY()
        {
            return yValues[lastIndex];
        }

        public void TakeLastNonAlloc( int N, ref List<Vector2> result)
        {
            result.Clear();
            N = Mathf.Min(N, Count);
            int idx;
            for (int i = 0; i < N; i++)
            {
                idx = mod(nextIndex - N + i, Capacity);
                result.Add(new Vector2(xValues[idx], yValues[idx]));
            }
        }

        public void Clear()
        {
            Count = 0;
            lastIndex = -1;
            nextIndex = 0;
            AverageX = 0;
            AverageY = 0;
        }
    }
}
