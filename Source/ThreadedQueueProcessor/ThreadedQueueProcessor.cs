using System.Collections.Concurrent;
using System.Diagnostics;

namespace ThreadedQueueProcessor
{
    public class ThreadedQueueProcessor<T>
    {
        // A thread-safe queue to store items for processing
        private readonly ConcurrentQueue<T> queue;

        // A semaphore to limit the number of concurrent threads
        private SemaphoreSlim? semaphore;

        // Function to process each item in the queue
        private Func<T, int, ThreadedQueueProcessor<T>, Task>? process;

        // The maximum number of concurrent processing threads
        private readonly int maxNumOfThreads;

        // A list to store references to running threads
        private int runningThreadsCount = 0;

        // A counter to keep track of the number of processed items
        private int count = 0;

        // Property to get the count of processed items
        public int ProcessedItemCount => count;

        // Property to get the running threads
        public int RunningThreadsCount => runningThreadsCount;

        /// <summary>
        /// Initializes a new instance of the ThreadedQueueProcessor class.
        /// </summary>
        /// <param name="maxNumOfThreads">The maximum number of concurrent processing threads.</param>
        public ThreadedQueueProcessor(int maxNumOfThreads)
        {
            this.maxNumOfThreads = maxNumOfThreads;
            queue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// Enqueues an item for processing.
        /// </summary>
        /// <param name="item">The item to be processed.</param>
        public void Enqueue(T item)
        {
            queue.Enqueue(item);
        }

        /// <summary>
        /// Starts processing items from the queue using multiple threads.
        /// </summary>
        /// <param name="processFunction">The function to process items.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task StartAsync(Func<T, int, ThreadedQueueProcessor<T>, Task> processFunction)
        {
            process = processFunction;

            var numOfThreads = Math.Min(queue.Count, maxNumOfThreads);
            var tasks = new List<Task>();

            semaphore = new SemaphoreSlim(numOfThreads, numOfThreads);

            for (int i = 0; i < semaphore.CurrentCount; i++)
            {
                tasks.Add(Task.Run(ProcessQueueAsync));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Processes items from the queue asynchronously.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task ProcessQueueAsync()
        {
            while (queue.TryDequeue(out var item))
            {
                try
                {
                    // Increment the count of running threads
                    Interlocked.Increment(ref runningThreadsCount);

                    // Increment the count of processed items (thread-safe)
                    var currentCount = Interlocked.Increment(ref count);

                    // Wait for a slot in the semaphore to become available then process the item
                    await semaphore!.WaitAsync();
                    await process!(item, currentCount, this);
                }
                finally
                {
                    Interlocked.Decrement(ref runningThreadsCount);
                    semaphore!.Release();
                }
            }
        }
    }
}