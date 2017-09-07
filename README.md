# Coroutines
A simple system for running nested coroutines in C#. Just drop `Coroutines.cs` into your project and you're ready to go.

## What is a coroutine?
C# has a feature called "enumerators" which are functions that can be *suspended* during their execution. They do this by using `yield return`, rather than regular `return`. When you use `yield`, you effectively *pause* the function, and at any time you can *resume* it and it will continue after the most recent `yield`.

In this context, a "coroutine" is basically one of these functions, but wrapped up in a nice little package and a few extra features:

- A container that will automatically update several coroutines for you
- A simple syntax for nesting coroutines so they can yield to others
- The ability to yield a floating point number, which will pause the routine for a desired amount of time
- Handles to routines are provided to make tracking them easier

## Example usage
To use the system, you need to create a `CoroutineRunner` and update it at regular intervals (eg. in a game loop). You also need to tell it a time interval.

```csharp
CoroutineRunner runner = new CoroutineRunner();

void UpdateGame(float deltaTime)
{
    runner.Update(deltaTime);
}
```

Now you can run coroutines by calling `Run()`. Here's a simple coroutine that counts:

```csharp
IEnumerator CountTo(int num, float delay)
{
    for (int i = 1; i <= num; ++i)
    {
        yield return delay;
        Console.WriteLine(i);
    }
}
void StartGame()
{
    //Count to 10, pausing 1 second between each number
    runner.Run(CountTo(10, 1.0f));
}
```

When you yield a floating-point number, it will pause the coroutine for that many seconds.

You can also nest coroutines by yielding to them. Here we will have a parent routine that will run several sub-routines:

```csharp
IEnumerator DoSomeCounting()
{
    Console.WriteLine("Counting to 3 slowly...");
    yield return CountTo(3, 2.0f);
    Console.WriteLine("Done!");

    Console.WriteLine("Counting to 5 normally...");
    yield return CountTo(5, 1.0f);
    Console.WriteLine("Done!");

    Console.WriteLine("Counting to 99 quickly...");
    yield return CountTo(99, 0.1f);
    Console.WriteLine("Done!");
}

void StartGame()
{
    runner.Run(DoSomeCounting());
}
```

You can also stop any running routines:

```csharp
//Stop all running routines
runner.StopAll();

var
```

## Other tips and tricks
A coroutine can run infinitely as well by using a loop. You can also tell the routine to "wait for the next frame" by yielding `null`:

```csharp
IEnumerator RunThisForever()
{
    while (true)
    {
        yield return null;
    }
}
```

Coroutines are very handy for games, especially for sequenced behavior and animations, acting sort of like *behavior trees*. For example, a simple enemy's AI routine might look like this:

```csharp
IEnumerator EnemyBehavior()
{
    while (enemyIsAlive)
    {
        yield return PatrolForPlayer();
        yield return Speak("I found you!");
        yield return ChaseAfterPlayer();
        yield return Speak("Wait... where did you go!?");
        yield return ReturnToPatrol();
    }
}
```

Sometimes you might want to run multiple routines in parallel, and have a parent routine wait for them both to finish. For this you can use the return handle from `Run()`:

```csharp
IEnumerator GatherNPCs(Vector gatheringPoint)
{
    //Make three NPCs walk to the gathering point at the same time
    var move1 = runner.Run(npc1.WalkTo(gatheringPoint));
    var move2 = runner.Run(npc2.WalkTo(gatheringPoint));
    var move3 = runner.Run(npc3.WalkTo(gatheringPoint));

    //We don't know how long they'll take, so just wait until all three have finished
    while (move1.IsPlaying || move2.IsPlaying || move3.IsPlaying)
        yield return null;

    //Now they've all gathered!
}
```

Here is a more complicated example where I show how you can use coroutines in conjunction with asynchronous functions (in this case, to download a batch of files and wait until they've finished):

```csharp
IEnumerator DownloadFile(string url, string toFile)
{
    //I actually don't know how to download files in C# so I just guessed this, but you get the point
    bool done = false;
    var client = new WebClient();
    client.DownloadFileCompleted += (e, b, o) => done = true;
    client.DownloadFileAsync(new Uri(url), toFile);
    while (!done)
        yield return null;
}

//Download the files one-by-one in sync
IEnumerator DownloadOneAtATime()
{
    yield return DownloadFile("http://site.com/file1.png", "file1.png");
    yield return DownloadFile("http://site.com/file2.png", "file2.png");
    yield return DownloadFile("http://site.com/file3.png", "file3.png");
    yield return DownloadFile("http://site.com/file4.png", "file4.png");
    yield return DownloadFile("http://site.com/file5.png", "file5.png");
}

//Download the files all at once asynchronously
IEnumerator DownloadAllAtOnce()
{
    //Start multiple async downloads and store their handles
    var downloads = new List<CoroutineHandle>();
    downloads.Add(runner.Run(DownloadFile("http://site.com/file1.png", "file1.png")));
    downloads.Add(runner.Run(DownloadFile("http://site.com/file2.png", "file2.png")));
    downloads.Add(runner.Run(DownloadFile("http://site.com/file3.png", "file3.png")));
    downloads.Add(runner.Run(DownloadFile("http://site.com/file4.png", "file4.png")));
    downloads.Add(runner.Run(DownloadFile("http://site.com/file5.png", "file5.png")));

    //Wait until all downloads are done
    while (downloads.Count > 0)
    {
        yield return null;
        for (int i = 0; i < downloads.Count; ++i)
            if (!downloads[i].IsRunning)
                downloads.RemoveAt(i--);
    }
}
```

## Why coroutines?

I use coroutines a lot in my games, as I find them great for organizing actor behavior and animations. As opposed to an async callback-based system, coroutines allow you to write your behaviors line-by-line, like how you would naturally write code, and result in very clean and easy to understand sequences.

There are good and bad times to use them, and you will get better at distinguishing this as you use them more. For many of my games, coroutines have been completely priceless, and have helped me organize and maintain very large and complicated systems that behave exactly in the order I wish them to.

**NOTE:** Not all languages have built-in support for coroutine systems like this. If you plan on porting your code to other languages, it may not be worth the pain of porting if your target language does not have a reliable means of implementing coroutines.
