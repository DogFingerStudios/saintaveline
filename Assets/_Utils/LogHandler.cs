using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

// AI: A simple data container for a single Unity log entry.
public struct UnityLogEntry
{
    public DateTime Timestamp;
    public string Message;
    public string StackTrace;
    public LogType Type;
}

// AI: Contract for anything that wants to receive logs (file, UI, network, etc.)
public interface IUnityLogSink
{
    void Handle(ref UnityLogEntry entry);
}

// AI: The interceptor that subscribes to Unity's global log events and fans out to sinks.
public sealed class UnityLogInterceptor : MonoBehaviour
{
    [SerializeField] private bool _useThreadedCallback = true;
    [SerializeField] private bool _logToFile = true;
    [SerializeField] private string _fileName = "game.log";

    // AI: Optional: expose a buffer sink for your UI to read from.
    public InMemoryBufferSink BufferSink => _bufferSink;

    private readonly ConcurrentQueue<UnityLogEntry> _pending = new ConcurrentQueue<UnityLogEntry>();
    private readonly System.Collections.Generic.List<IUnityLogSink> _sinks = new System.Collections.Generic.List<IUnityLogSink>();

    private FileLogSink _fileSink;
    private InMemoryBufferSink _bufferSink;

    // AI: Ensure this exists very early so startup logs aren't missed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void AutoCreate()
    {
        if (UnityEngine.Object.FindAnyObjectByType<UnityLogInterceptor>(FindObjectsInactive.Include) == null)
        {
            GameObject go = new GameObject("UnityLogInterceptor_Auto");
            go.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<UnityLogInterceptor>();
        }
    }

    private void OnEnable()
    {
        if (_useThreadedCallback)
        {
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }
        else
        {
            Application.logMessageReceived += OnLogMessageReceivedMainThread;
        }

        if (_logToFile)
        {
            //string path = Path.Combine(Application.persistentDataPath, _fileName);
            string path = @"C:\Users\aclau\Desktop\game.log";
            _fileSink = new FileLogSink(path);
            _sinks.Add(_fileSink);
        }

        //_bufferSink = new InMemoryBufferSink(300);

        //_sinks.Add(_bufferSink);
    }

    private void OnDisable()
    {
        if (_useThreadedCallback)
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
        }
        else
        {
            Application.logMessageReceived -= OnLogMessageReceivedMainThread;
        }

        if (_fileSink != null)
        {
            _fileSink.Dispose();
            _fileSink = null;
        }

        _sinks.Clear();
    }

    // AI: Threaded callback (may come from worker threads).
    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        var entry = new UnityLogEntry
        {
            Timestamp = DateTime.Now,
            Message = condition,
            StackTrace = stackTrace,
            Type = type
        };

        _pending.Enqueue(entry);
    }

    // AI: Main-thread callback alternative.
    private void OnLogMessageReceivedMainThread(string condition, string stackTrace, LogType type)
    {
        var entry = new UnityLogEntry
        {
            Timestamp = DateTime.Now,
            Message = condition,
            StackTrace = stackTrace,
            Type = type
        };

        FanOut(ref entry);
    }

    private void Update()
    {
        while (_pending.TryDequeue(out var entry))
        {
            FanOut(ref entry);
        }
    }

    private void FanOut(ref UnityLogEntry entry)
    {
        for (int i = 0; i < _sinks.Count; i++)
        {
            _sinks[i].Handle(ref entry);
        }
    }

    // AI: Public API to let you register your own sinks at runtime.
    public void AddSink(IUnityLogSink sink)
    {
        if (sink != null)
        {
            _sinks.Add(sink);
        }
    }

    public void RemoveSink(IUnityLogSink sink)
    {
        if (sink != null)
        {
            _sinks.Remove(sink);
        }
    }
}



// AI: File sink implementation.
public sealed class FileLogSink : IUnityLogSink, IDisposable
{
    private readonly string _path;
    private StreamWriter _writer;

    public FileLogSink(string path)
    {
        _path = path;

        Directory.CreateDirectory(Path.GetDirectoryName(_path));
        _writer = new StreamWriter(new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
        _writer.AutoFlush = true;
    }

    public void Handle(ref UnityLogEntry entry)
    {
        if (_writer != null)
        {
            string line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Type}] {entry.Message}";
            _writer.WriteLine(line);

            if (!string.IsNullOrEmpty(entry.StackTrace))
            {
                _writer.WriteLine(entry.StackTrace);
            }
        }
    }

    public void Dispose()
    {
        if (_writer != null)
        {
            _writer.Dispose();
            _writer = null;
        }
    }
}

// AI: Example UI sink stub; wire this to your dialog/console as you like.
public sealed class InMemoryBufferSink : IUnityLogSink
{
    private readonly int _capacity;
    private readonly ConcurrentQueue<UnityLogEntry> _buffer;

    public InMemoryBufferSink(int capacity = 200)
    {
        _capacity = capacity;
        _buffer = new ConcurrentQueue<UnityLogEntry>();
    }

    public void Handle(ref UnityLogEntry entry)
    {
        _buffer.Enqueue(entry);

        while (_buffer.Count > _capacity)
        {
            if (_buffer.TryDequeue(out _))
            {
                // AI: Trim oldest
            }
        }
    }

    public UnityLogEntry[] Snapshot()
    {
        return _buffer.ToArray();
    }
}
