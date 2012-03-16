// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

using PostSharp;
using PostSharp.Extensibility;
using Xtensive.Aspects.Helpers;

namespace Xtensive.Aspects
{
  /// <summary>
  /// Error log used by aspects.
  /// </summary>
  public static class ErrorLog
  {
    private static readonly MessageSource messageSource;

    /// <summary>
    /// Writes the standard message to the underlying PostSharp <see cref="messageSource"/>.
    /// </summary>
    /// <param name="severity">The severity type.</param>
    /// <param name="messageType">The standard message type.</param>
    /// <param name="args">The message arguments.</param>
    public static void Write(MessageLocation location, SeverityType severity,
      AspectMessageType messageType, params object[] args)
    {
      messageSource.Write(location, severity, "XW0001",
        new object[] {string.Format(AspectHelper.GetStandardMessage(messageType), args)});
    }

    /// <summary>
    /// Writes the message to the underlying PostSharp <see cref="messageSource"/>.
    /// </summary>
    /// <param name="severity">The severity type.</param>
    /// <param name="format">The message format string.</param>
    /// <param name="args">The message arguments.</param>
    public static void Write(MessageLocation location, SeverityType severity, string format, params object[] args)
    {
      messageSource.Write(location, severity, "XW0001", new object[] {string.Format(format, args)});
    }


    // Type initializer

    static ErrorLog()
    {
      messageSource = new MessageSource("Xtensive.Aspects", new AspectsMessageDispenser());
    }
  }
}