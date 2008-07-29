// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.29

using PostSharp.Extensibility;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Extension methods for <see cref="MessageSource"/> class.
  /// </summary>
  public static class MessageSourceExtensions
  {
    /// <summary>
    /// Writes the line to specified <see cref="MessageSource"/> instance.
    /// </summary>
    /// <param name="instance">The <see cref="MessageSource"/> instance to write message to.</param>
    /// <param name="severity">The severity type.</param>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void WriteLine(this MessageSource instance, SeverityType severity,  string format, params object[] args)
    {
      instance.Write(severity, "X", new object[] { string.Format(format, args) });
    }
  }
}