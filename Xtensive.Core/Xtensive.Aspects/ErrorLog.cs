// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

using System;
using PostSharp.Extensibility;
using Xtensive.Aspects.Helpers;
using Xtensive.Aspects.Resources;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Error log used by aspects.
  /// </summary>
  public static class ErrorLog
  {
    private readonly static string usingStringsX = Strings.XW0001;
    private readonly static string usingStringsDebugX = Strings.XD0001;

    /// <summary>
    /// Gets the message source this log is bound to.
    /// </summary>
    public static MessageSource MessageSource;

    /// <summary>
    /// Writes the standard message to the underlying PostSharp <see cref="MessageSource"/>.
    /// </summary>
    /// <param name="severity">The severity type.</param>
    /// <param name="messageType">The standard message type.</param>
    /// <param name="args">The message arguments.</param>
    public static void Write(SeverityType severity, AspectMessageType messageType, params object[] args)
    {
      MessageSource.Write(severity, "XW0001",
        new object[] { string.Format(AspectHelper.GetStandardMessage(messageType), args) });
    }

    /// <summary>
    /// Writes the message to the underlying PostSharp <see cref="MessageSource"/>.
    /// </summary>
    /// <param name="severity">The severity type.</param>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void Write(SeverityType severity, string format, params object[] args)
    {
      MessageSource.Write(severity, "XW0001", 
        new object[] { string.Format(format, args) });
    }

    /// <summary>
    /// Writes the debug message to the underlying PostSharp <see cref="MessageSource"/>.
    /// </summary>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void Debug(string format, params object[] args)
    {
      MessageSource.Write(SeverityType.Warning, "XD0001",
        new object[] { string.Format(format, args) });
    }


    // Type initializer

    static ErrorLog()
    {
      MessageSource = new MessageSource("Xtensive.Aspects", Strings.ResourceManager);
    }
  }
}