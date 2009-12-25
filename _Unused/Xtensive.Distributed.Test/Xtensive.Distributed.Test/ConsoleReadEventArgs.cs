// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.03

using System;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Event data for console output.
  /// </summary>
  public class ConsoleReadEventArgs: EventArgs
  {
    #region Private fields

    private readonly string message;
    private readonly bool isError;

    #endregion

    #region Properties

    /// <summary>
    /// Gets <see langword="true"/> if <see cref="Message"/> was written to the error output, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool IsError
    {
      get { return isError; }
    }

    /// <summary>
    /// Gets console output string.
    /// </summary>
    public string Message
    {
      get { return message; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="ConsoleReadEventArgs"/>.
    /// </summary>
    /// <param name="message">Message string written to the console output.</param>
    /// <param name="isError"><see langword="True"/> if <paramref name="message"/> was written to the error output, otherwise gets <see langword="false"/>.</param>
    public ConsoleReadEventArgs(string message, bool isError)
    {
      this.message = message;
      this.isError = isError;
    }

    #endregion
  }
}