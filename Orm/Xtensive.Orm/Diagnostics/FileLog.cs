// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Console log implementation.
  /// </summary>
  public sealed class FileLog : TextualLogImplementationBase
  {
    private static readonly Dictionary<string, FileLog> logs = new Dictionary<string, FileLog>();
    private readonly FileLog primaryLog;
    private readonly Stream stream;
    private readonly TextWriter writer;

    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      if (primaryLog!=null) {
        primaryLog.LogEventText(text);
        return;
      }
      if (stream.CanWrite) {
        writer.WriteLine(text);
        writer.Flush();
        stream.Flush();
      }
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">Log name.</param>
    /// <param name="fileName">Name of the file.</param>
    public FileLog(string name, string fileName)
      : base(name)
    {
      fileName = Path.IsPathRooted(fileName)
          ? fileName
          : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
      lock (logs) {
        if (logs.TryGetValue(fileName, out primaryLog))
          return;
        stream = new FileStream(fileName, FileMode.Append, FileAccess.Write);
        writer = TextWriter.Synchronized(new StreamWriter(stream, Encoding.UTF8));
        logs.Add(fileName, this);
      }
    }
  }
}