// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Error log implementation.
  /// </summary>
  public sealed class ErrorLog : TextualLogImplementationBase
  {
    /// <inheritdoc/>
    protected override void LogEventText(string text)
    {
      Console.Error.WriteLine(text);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Log name.</param>
    public ErrorLog(string name)
      : base(name)
    {
      LoggedEventTypes = LogEventTypes.Error | LogEventTypes.FatalError;
    }
  }
}