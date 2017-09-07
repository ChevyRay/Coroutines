using System;
using System.Collections;
using System.Diagnostics;
using Coroutines;

namespace Example
{
    // Here's a silly example animating a little roguelike-style character moving.
    class Program
    {
        public static void Main(string[] args)
        {
            //Timer variables to run the update loop at 30 fps
            var watch = Stopwatch.StartNew();
            const float updateRate = 1f / 30f;
            float prevTime = watch.ElapsedMilliseconds / 1000f;
            float accumulator = 0f;

            //The little @ character's position
            int px = 0;
            int py = 0;

            //Routine to move horizontally
            IEnumerator MoveX(int amount, float stepTime)
            {
                int dir = amount > 0 ? 1 : -1;
                while (amount != 0)
                {
                    yield return stepTime;
                    px += dir;
                    amount -= dir;
                }
            }

            //Routine to move vertically
            IEnumerator MoveY(int amount, float stepTime)
            {
                int dir = amount > 0 ? 1 : -1;
                while (amount != 0)
                {
                    yield return stepTime;
                    py += dir;
                    amount -= dir;
                }
            }

            //Walk the little @ character on a path
            IEnumerator Movement()
            {
                //Walk normally
                yield return MoveX(5, 0.25f);
                yield return MoveY(5, 0.25f);

                //Walk slowly
                yield return MoveX(2, 0.5f);
                yield return MoveY(2, 0.5f);
                yield return MoveX(-2, 0.5f);
                yield return MoveY(-2, 0.5f);

                //Run fast
                yield return MoveX(5, 0.1f);
                yield return MoveY(5, 0.1f);
            }

            //Render a little map with the @ character in the console
            void DrawMap()
            {
                Console.Clear();
                for (int y = 0; y < 16; ++y)
                {
                    for (int x = 0; x < 16; ++x)
                    {
                        if (x == px && y == py)
                            Console.Write('@');
                        else
                            Console.Write('.');
                    }
                    Console.WriteLine();
                }
            }

            //Run the coroutine
            var runner = new CoroutineRunner();
            var moving = runner.Run(Movement());

            //Run the update loop until we've finished moving
            while (moving.IsRunning)
            {
                //Track time
                float currTime = watch.ElapsedMilliseconds / 1000f;
                accumulator += currTime - prevTime;
                prevTime = currTime;

                //Update at our requested rate (30 fps)
                if (accumulator > updateRate)
                {
                    accumulator -= updateRate;
                    runner.Update(updateRate);
                    DrawMap();
                }
            }
        }
    }
}
