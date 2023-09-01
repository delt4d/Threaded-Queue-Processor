# Threaded Queue Processor

The Threaded Queue Processor is a C# library that provides a thread-safe mechanism for processing items from a queue using multiple concurrent threads. This library is particularly useful for scenarios where you need to efficiently process a large number of items asynchronously.

## Features
- Thread-safe queue to store items for processing.
- Semaphore-based concurrency control to limit the number of concurrent processing threads.
- Ability to define a custom processing function for each item in the queue.
- Tracking of processed items count and running threads.

## Usage

Go to the folder `Source/ThreadeQueueProcessor` and execute the command:
```bash
dotnet run build
```

Inside the folder, Copy the references inside `bin/Debug/net{version}` to a folder of your project. Then, add the reference to the library DLL `ThreadedQueueProcessor.dll`.

To use the library, add this line in your project files where you want to use it:
```csharp
using ThreadedQueueProcessor;
```

And you can now create a new threaded queue processing instance. Example:
```csharp
using System.Diagnostics;
using ThreadedQueueProcessor;

// create MyItem class
class MyItem {
    public string Value { get; set; }
    public MyItem(string value) { this.Value = value; }
}

// Create a new threaded queue processing instance
// Try to change the parameter maxNumOfThreads to another value like 1 or 100 and see the results.
var queue = new ThreadedQueueProcessor<MyItem>(maxNumOfThreads: 20);

// Enqueue
for (int i = 0; i < 100; i++)
{
    queue.Enqueue(new MyItem($"Item: Foo {i}"));
    queue.Enqueue(new MyItem($"Item: Bar {i}"));
    queue.Enqueue(new MyItem($"Item: Baz {i}"));
}

Console.WriteLine($"Started {queue.ProcessedItemCount}");

// instantiate and start Stopwatch instance
var stopwatch = new Stopwatch();
stopwatch.Start();

// Wait for the queue to complete its execution.
// It pass a callback that will be called for each item in the queue
await queue.StartAsync(async (currentItem, index, queueInstance) =>
{
    Console.Write($"\n{currentItem.Value}. Running tasks: {queueInstance.RunningThreadsCount}. Index {index}");
    var delayTime = new Random().Next(200, 1000+1);
    await Task.Delay(delayTime); // Simulate the time to proccess the queue item
});

// Show the results
stopwatch.Stop();

Console.WriteLine($"\n\nCompleted {queue.ProcessedItemCount}.");
Console.WriteLine($"Time Elapsed: {stopwatch.Elapsed.TotalSeconds} seconds");
```