/*
 
MIT License

Copyright (c) 2017 Chevy Ray Johnston

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System.Collections;
using System.Collections.Generic;

namespace Coroutines
{
    /// <summary>
    /// A container for running multiple routines in parallel. Coroutines can be nested.
    /// </summary>
    public class CoroutineRunner
    {
        List<IEnumerator> running = new List<IEnumerator>();
        List<float> delays = new List<float>();

        /// <summary>
        /// Run a coroutine.
        /// </summary>
        /// <returns>A handle to the new coroutine.</returns>
        /// <param name="delay">How many seconds to delay before starting.</param>
        /// <param name="routine">The routine to run.</param>
        public CoroutineHandle Run(float delay, IEnumerator routine)
        {
            running.Add(routine);
            delays.Add(delay);
            return new CoroutineHandle(this, routine);
        }

        /// <summary>
        /// Run a coroutine.
        /// </summary>
        /// <returns>A handle to the new coroutine.</returns>
        /// <param name="routine">The routine to run.</param>
        public CoroutineHandle Run(IEnumerator routine)
        {
            return Run(0f, routine);
        }

        /// <summary>
        /// Stop the specified routine.
        /// </summary>
        /// <returns>True if the routine was actually stopped.</returns>
        /// <param name="routine">The routine to stop.</param>
        public bool Stop(IEnumerator routine)
        {
            int i = running.IndexOf(routine);
            if (i < 0)
                return false;
            running[i] = null;
            delays[i] = 0f;
            return true;
        }

        /// <summary>
        /// Stop the specified routine.
        /// </summary>
        /// <returns>True if the routine was actually stopped.</returns>
        /// <param name="routine">The routine to stop.</param>
        public bool Stop(CoroutineHandle routine)
        {
            return routine.Stop();
        }

        /// <summary>
        /// Stop all running routines.
        /// </summary>
        public void StopAll()
        {
            running.Clear();
            delays.Clear();
        }

        /// <summary>
        /// Check if the routine is currently running.
        /// </summary>
        /// <returns>True if the routine is running.</returns>
        /// <param name="routine">The routine to check.</param>
        public bool IsRunning(IEnumerator routine)
        {
            return running.Contains(routine);
        }

        /// <summary>
        /// Check if the routine is currently running.
        /// </summary>
        /// <returns>True if the routine is running.</returns>
        /// <param name="routine">The routine to check.</param>
        public bool IsRunning(CoroutineHandle routine)
        {
            return routine.IsRunning;
        }

        /// <summary>
        /// Update all running coroutines.
        /// </summary>
        /// <returns>True if any routines were updated.</returns>
        /// <param name="deltaTime">How many seconds have passed sinced the last update.</param>
        public bool Update(float deltaTime)
        {
            if (running.Count > 0)
            {
                for (int i = 0; i < running.Count; i++)
                {
                    if (delays[i] > 0f)
                        delays[i] -= deltaTime;
                    else if (running[i] == null || !MoveNext(running[i], i))
                    {
                        running.RemoveAt(i);
                        delays.RemoveAt(i--);
                    }
                }
                return true;
            }
            return false;
        }

        bool MoveNext(IEnumerator routine, int index)
        {
            if (routine.Current is IEnumerator)
            {
                if (MoveNext((IEnumerator)routine.Current, index))
                    return true;
                
                delays[index] = 0f;
            }

            bool result = routine.MoveNext();

            if (routine.Current is float)
                delays[index] = (float)routine.Current;
            
            return result;
        }

        /// <summary>
        /// How many coroutines are currently running.
        /// </summary>
        public int Count
        {
            get { return running.Count; }
        }
    }

    /// <summary>
    /// A handle to a (potentially running) coroutine.
    /// </summary>
    public struct CoroutineHandle
    {
        /// <summary>
        /// Reference to the routine's runner.
        /// </summary>
        public CoroutineRunner Runner;

        /// <summary>
        /// Reference to the routine's enumerator.
        /// </summary>
        public IEnumerator Enumerator;

        /// <summary>
        /// Construct a coroutine. Never call this manually, only use return values from Coroutines.Run().
        /// </summary>
        /// <param name="runner">The routine's runner.</param>
        /// <param name="enumerator">The routine's enumerator.</param>
        public CoroutineHandle(CoroutineRunner runner, IEnumerator enumerator)
        {
            Runner = runner;
            Enumerator = enumerator;
        }

        /// <summary>
        /// Stop this coroutine if it is running.
        /// </summary>
        /// <returns>True if the coroutine was stopped.</returns>
        public bool Stop()
        {
            return IsRunning && Runner.Stop(Enumerator);
        }

        /// <summary>
        /// A routine to wait until this coroutine has finished running.
        /// </summary>
        /// <returns>The wait enumerator.</returns>
        public IEnumerator Wait()
        {
            if (Enumerator != null)
                while (Runner.IsRunning(Enumerator))
                    yield return null;
        }

        /// <summary>
        /// True if the enumerator is currently running.
        /// </summary>
        public bool IsRunning
        {
            get { return Enumerator != null && Runner.IsRunning(Enumerator); }
        }
    }
}