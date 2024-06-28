// See https://aka.ms/new-console-template for more information

using Lab.ParalleQuery;
using System.Collections.Concurrent;

var cancellationTokenSource = new CancellationTokenSource();
var token = cancellationTokenSource.Token;
var degreeOfParallelism = Math.Min(Environment.ProcessorCount, 512);

var threadsMap = new ConcurrentDictionary<int, List<int>>();

int[] Ids = Enumerable.Range(1, 100).ToArray();

var batches = Ids.Chunk2(50).ToArray();

foreach (var batch in batches)
{
    batch.AsParallel()
    .WithDegreeOfParallelism(degreeOfParallelism)
    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
    .Where(_ => !token.IsCancellationRequested)
    .Select(HighComputeProcess).ToArray();

    foreach (var threadMap in threadsMap)
    {
        var values = string.Join(",", threadMap.Value.Select(x => x.ToString()));
        Console.WriteLine($"ThreadId: {threadMap.Key}, RecordCount:{threadMap.Value.Count}, IDsList:{values}");
    }
}




int HighComputeProcess(int n)
{
    Thread.Sleep(n);

    threadsMap.AddOrUpdate(key: Environment.CurrentManagedThreadId,
        addValue: new List<int> { n},
        updateValueFactory: (key,values) => { values.Add(n);return values; });
    var m = 0;
    for (int i = 0; i < 1000000; i++)
    {
        m++;
    }
    return m;
}

Console.WriteLine("Lab, ParallelQuert!");

Console.ReadLine();
