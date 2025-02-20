﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Reloaded.Mod.Loader.Logging;

/// <summary>
/// Basic logger writer used for storing crash logs.
/// </summary>
public class LogWriter : IDisposable
{
    private const int MaxBufferLength = 1000;

    /// <summary>
    /// Gets the path to which the log will be flushed to.
    /// </summary>
    public string FlushPath { get; }

    private Timer _autoFlushThread;
    private StreamWriter  _textStream;
    private Logger        _logger;
    private List<string>  _logItems;

    /// <summary>
    /// Intercepts logs from the console and provides the ability to flush them to a file in the event of a crash.
    /// </summary>
    /// <param name="logger">The logger to intercept logs from.</param>
    /// <param name="outputDir">The directory to which the log is output to.</param>
    public LogWriter(Logger logger, string outputDir)
    {
        var executableName    = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
        var universalDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss");
        Directory.CreateDirectory(outputDir);

        _logItems = new List<string>(MaxBufferLength + 1);
        _logger  = logger;
        _logger.OnPrintMessage += OnPrintMessage;

        // Add integer to file path in case there is another instance of same process launched at the same time.
        string GetLogFilePath()
        {
            for (int x = 0; x < short.MaxValue; x++)
            {
                var filePath = x == 0 ? 
                    Path.Combine(outputDir, $"{universalDateTime} ~ {executableName}.txt") :
                    Path.Combine(outputDir, $"{universalDateTime} ~ {executableName} [{x}].txt");
                
                if (!File.Exists(filePath))
                    return filePath;
            }

            return default;
        }

        FlushPath = GetLogFilePath();
        if (FlushPath == null) 
            return;

        _textStream = File.CreateText(FlushPath);
        _textStream.AutoFlush = false;
        _autoFlushThread = new Timer(AutoFlush, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(250));
    }

    /// <summary>
    /// Note: The default value of 250ms per potential flush should fit within the time limit provided for applications to close up, e.g. Console Control Handlers (SetConsoleCtrlHandler).
    ///       Flushing periodically is also prone to power loss or forced kills via TerminateProcess() etc.
    /// </summary>
    private void AutoFlush(object state) => Flush();

    /// <summary>
    /// Flushes the current contents of the log.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Flush()
    {
        if (_logItems.Count <= 0) 
            return;

        foreach (var item in _logItems)
            _textStream?.WriteLine(item);

        _textStream?.Flush();
        _logItems.Clear();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnPrintMessage(object sender, string message)
    {
        if (_logItems.Count == MaxBufferLength)
            Flush();

        _logItems.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    /// <inheritdoc />
    public void Dispose() => _logger.OnPrintMessage -= OnPrintMessage;
}