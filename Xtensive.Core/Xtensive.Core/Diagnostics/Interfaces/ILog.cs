// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System;
using Xtensive.IoC;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log implementation.
  /// </summary>
  public interface ILog: ILogBase, 
    IContext<LogCaptureScope>
  {
    /// <summary>
    /// Gets <see cref="IRealLog"/> object that finally processes all the calls 
    /// to this <see cref="ILog"/> implementation.
    /// </summary>
    IRealLog RealLog { get; }

    /// <summary>
    /// Writes debug message to log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    void Debug(string format, params object[] args);

    /// <summary>
    /// Writes debug message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    Exception Debug(Exception exception, string format, params object[] args);

    /// <summary>
    /// Writes debug message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    Exception Debug(Exception exception);

    /// <summary>
    /// Creates an indented and titled region in log.
    /// The region boundaries are logged as debug messages.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>An <see cref="IDisposable"/>, which disposal will
    /// "close" the region.</returns>
    IDisposable DebugRegion(string format, params object[] args);

    /// <summary>
    /// Writes info message to log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    void Info(string format, params object[] args);

    /// <summary>
    /// Writes info message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    Exception Info(Exception exception, string format, params object[] args);

    /// <summary>
    /// Writes info message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    Exception Info(Exception exception);

    /// <summary>
    /// Creates an indented and titled region in log.
    /// The region boundaries are logged as info messages.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>
    /// An <see cref="IDisposable"/>, which disposal will "close" the region.
    /// </returns>
    IDisposable InfoRegion(string format, params object[] args);

    /// <summary>
    /// Writes warning message to log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    void Warning(string format, params object[] args);

    /// <summary>
    /// Writes warning message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    Exception Warning(Exception exception, string format, params object[] args);

    /// <summary>
    /// Writes warning message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    Exception Warning(Exception exception);

    /// <summary>
    /// Writes error message to log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    void Error(string format, params object[] args);

    /// <summary>
    /// Writes error message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    Exception Error(Exception exception, string format, params object[] args);

    /// <summary>
    /// Writes error message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    Exception Error(Exception exception);

    /// <summary>
    /// Writes fatal error message to log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    void FatalError(string format, params object[] args);

    /// <summary>
    /// Writes fatal error message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments.</param>
    Exception FatalError(Exception exception, string format, params object[] args);

    /// <summary>
    /// Writes fatal error message to log.
    /// </summary>
    /// <param name="exception">The internal exception.</param>
    Exception FatalError(Exception exception);
  }
}