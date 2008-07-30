// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

using System;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Resources;

namespace Xtensive.Core.Aspects
{
  public static class ErrorLog
  {
    private readonly static string usingStringsX = Strings.X;
    private readonly static string usingStringsDebugX = Strings.DebugX;

    /// <summary>
    /// Gets the message source this log is bound to.
    /// </summary>
    public static MessageSource MessageSource;

    /// <summary>
    /// Writes the message to the underlying PostSharp <see cref="MessageSource"/>.
    /// </summary>
    /// <param name="severity">The severity type.</param>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void Write(SeverityType severity, string format, params object[] args)
    {
      MessageSource.Write(severity, "X", 
        new object[] { string.Format(format, args) });
    }

    /// <summary>
    /// Writes the debug message to the underlying PostSharp <see cref="MessageSource"/>.
    /// </summary>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void Debug(string format, params object[] args)
    {
      MessageSource.Write(SeverityType.Warning, "DebugX",
         new object[] {String.Format(format, args) });
    }


    // Type initializer

    static ErrorLog()
    {
      MessageSource = new MessageSource("Xtensive.Core.Aspects", Strings.ResourceManager);
    }
  }
}