// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Console log implementation.
  /// </summary>
  public sealed class ConsoleLog : TextualLogImplementationBase
  {
    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      Console.Out.WriteLine(text);
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">Log name.</param>
    public ConsoleLog(string name)
      : base(name)
    {
    }
  }
}