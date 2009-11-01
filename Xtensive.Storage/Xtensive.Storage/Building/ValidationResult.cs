// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.17

using System;

namespace Xtensive.Storage.Building
{
  [Serializable]
  internal class ValidationResult
  {
    private readonly string message;
    private readonly bool success;

    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message
    {
      get { return message; }
    }

    /// <summary>
    /// Gets a value indicating whether validation was successfull.
    /// </summary>
    public bool Success
    {
      get { return success; }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="success">if set to <see langword="true"/> then validation result was successfull.</param>
    /// <param name="message">The message.</param>
    public ValidationResult(bool success, string message)
    {
      this.success = success;
      this.message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="success">if set to <see langword="true"/> then validation result was successfull.</param>
    public ValidationResult(bool success)
      : this(success, string.Empty)
    {
    }
  }
}