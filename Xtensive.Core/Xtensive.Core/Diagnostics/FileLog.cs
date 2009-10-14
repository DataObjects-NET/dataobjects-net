// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Diagnostics
{
  /// <summary>
  /// Console log implementation.
  /// </summary>
  public sealed class FileLog : TextualLogImplementationBase
  {
    private static readonly Dictionary<string, TextWriter> writers;
    private readonly TextWriter writer;

    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      writer.WriteLine(text);
      writer.Flush();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Log name.</param>
    /// <param name="fileName">Name of the file.</param>
    public FileLog(string name, string fileName)
      : base(name)
    {
      lock (writers) {
        if (writers.TryGetValue(fileName, out writer))
          return;
        var stream = new FileStream(fileName, FileMode.Append, FileAccess.Write);
        writer = TextWriter.Synchronized(new StreamWriter(stream, Encoding.UTF8));
        writers.Add(fileName, writer);
      }
    }
  }
}