// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using System.Diagnostics;
using System.Text;
using System;

using Xtensive.Internals.DocTemplates;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log writing all events to <see cref="StringBuilder"/> <see cref="Output"/>.
  /// </summary>
  public class StringLog: TextualLogImplementationBase
  {
    private readonly StringBuilder output  = new StringBuilder();

    /// <summary>
    /// Gets output of this log (as <see cref="StringBuilder"/>).
    /// </summary>
    public StringBuilder Output
    {
      [DebuggerStepThrough]
      get { return output; }
    }

    /// <summary>
    /// Gets output of this log (as <see cref="String"/>).
    /// </summary>
    public override string Text {
      get {
        lock (this)
          return output.ToString();
      }
    }

    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      lock (this)
        output.AppendLine(text);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    public StringLog(string name) 
      : base(name)
    {
    }
  }
}