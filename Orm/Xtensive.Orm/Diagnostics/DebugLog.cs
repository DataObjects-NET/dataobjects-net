// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Debug log implementation.
  /// </summary>
  public sealed class DebugLog : TextualLogImplementationBase
  {
    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      Debug.WriteLine(text);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name">Log name.</param>
    public DebugLog(string name)
      : base(name)
    {
    }
  }
}