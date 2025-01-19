using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class ProfilingResult
{
	public string? MethodName { get; set; }
	public DateTime Timestamp { get; set; }
	public long TotalTicks { get; set; }
	public double ExecutionMs { get; set; }
	public int GcCollections { get; set; }
	public double MemoryUsedMB { get; set; }
	public long MemoryUsedBytes { get; set; }
}

public class PerformanceProfilerWrapper : IDisposable
{
	private static readonly ConcurrentDictionary<string, List<ProfilingResult>> _profilingResults
		= new ConcurrentDictionary<string, List<ProfilingResult>>();
	private static readonly HashSet<string> _firstCallsMade = [];
	private static readonly object _syncLock = new();
	private readonly string _methodName;
	private readonly Stopwatch _stopwatch;
	private readonly int _gcBefore;
	private readonly long _memoryBefore;
	private readonly bool _isFirstCall;

	public PerformanceProfilerWrapper([CallerMemberName] string methodName = "")
	{
		_methodName = methodName;
		_stopwatch = new Stopwatch();

		// Check if this is the first call
		lock (_syncLock)
		{
			_isFirstCall = !_firstCallsMade.Contains(methodName);
			if (_isFirstCall)
			{
				_firstCallsMade.Add(methodName);
			}
		}

		_gcBefore = GC.CollectionCount(0);
		_memoryBefore = GC.GetTotalMemory(false);

		_stopwatch.Start();
	}

	public void Dispose()
	{
		_stopwatch.Stop();
		var elapsedTicks = _stopwatch.ElapsedTicks;
		var currentMethodName = _methodName;

		// Skip storing results if it's the first call due to "cold start" effects
		if (_isFirstCall)
			return;

		var memoryUsedBytes = GC.GetTotalMemory(false) - _memoryBefore;

		var result = new ProfilingResult
		{
			MethodName = currentMethodName,
			Timestamp = DateTime.Now,
			TotalTicks = elapsedTicks,
			ExecutionMs = elapsedTicks / (double)Stopwatch.Frequency * 1000,
			GcCollections = GC.CollectionCount(0) - _gcBefore,
			MemoryUsedMB = (double)memoryUsedBytes / (1024 * 1024),
			MemoryUsedBytes = memoryUsedBytes
		};

		var list = _profilingResults.GetOrAdd(currentMethodName, _ => new List<ProfilingResult>());
		lock (list)
			list.Add(result);
	}

	public static bool WriteResultsToFile(string directoryPath)
	{
		lock (_syncLock)
		{
			try
			{
				Directory.CreateDirectory(directoryPath);
				string filePath = Path.Combine(directoryPath, $"profiling_results_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");

				using (var writer = new StreamWriter(filePath, false))
				{
					foreach (var methodResults in _profilingResults)
					{
						var results = methodResults.Value;
						if (results.Count == 0) continue;

						writer.WriteLine($"\n ==================== [ {methodResults.Key} Performance Summary ] ====================");

						double avgExecutionMs = results.Average(r => r.ExecutionMs);

						writer.WriteLine($"\nTotal Measurements: {results.Count}");
						writer.WriteLine($"Time Period: {results.Min(r => r.Timestamp)} to {results.Max(r => r.Timestamp)}");
						writer.WriteLine("\nPerformance Statistics:");
						writer.WriteLine($"{"Average execution time:",-30} {avgExecutionMs:F6} ms");
						writer.WriteLine($"{"Minimum execution time:",-30} {results.Min(r => r.ExecutionMs):F6} ms");
						writer.WriteLine($"{"Maximum execution time:",-30} {results.Max(r => r.ExecutionMs):F6} ms");
						writer.WriteLine($"{"Median execution time:",-30} {results.OrderBy(r => r.ExecutionMs).ElementAt(results.Count / 2).ExecutionMs:F6} ms");
						writer.WriteLine($"{"Standard deviation:",-30} {Math.Sqrt(results.Average(r => Math.Pow(r.ExecutionMs - avgExecutionMs, 2))):F6} ms");
						writer.WriteLine($"\nMemory and GC Statistics:");
						writer.WriteLine($"{"Average memory usage:",-30} {results.Average(r => Math.Abs(r.MemoryUsedMB)):F6} MB ({(long)results.Average(r => Math.Abs(r.MemoryUsedBytes)):N0} bytes)");
						writer.WriteLine($"{"Total GC collections:",-30} {results.Sum(r => r.GcCollections)}");
					}
				}

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"[PerformanceProfiler] Results saved to: {filePath}");
				Console.ResetColor();
				return true;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[PerformanceProfiler] Error saving results: {ex.Message}");
				Console.ResetColor();
				return false;
			}
		}
	}

	public static void ClearResults()
	{
		lock (_syncLock)
		{
			_profilingResults.Clear();
			_firstCallsMade.Clear();
		}
	}

	public static IReadOnlyDictionary<string, List<ProfilingResult>> GetAllResults()
	{
		return _profilingResults;
	}
}