// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace Xtensive.Core
{
  /// <summary>
  /// Aggregates a set of caught exceptions.
  /// </summary>
  public class AggregateException : Exception
  {
    /// <summary>
    /// Gets the list of caught exceptions.
    /// </summary>
    public IReadOnlyList<Exception> Exceptions
    {
      [DebuggerStepThrough]
      get;
    }

    /// <summary>
    /// Gets the "flat" list with all aggregated exceptions. 
    /// If other <see cref=" AggregateException"/>s were aggregated, 
    /// their inner exceptions are included instead of them.
    /// </summary>
    /// <returns>Flat list of aggregated exceptions.</returns>
    public List<Exception> GetFlatExceptions()
    {
      var result = new List<Exception>();

      foreach (var exception in Exceptions) {
        if (exception is AggregateException ae)
          result.AddRange(ae.GetFlatExceptions());
        else
          result.Add(exception);
      }

      return result;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder(64)
        .Append(base.ToString())
        .AppendLine()
        .AppendFormat($"{Strings.OriginalExceptions}:");
      int i = 1;
      foreach (var exception in Exceptions)
        _ = sb.AppendLine().AppendFormat($"{i++}: {exception}");
      return sb.ToString();
    }


    // Constructors
    
    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public AggregateException()
      : base(Strings.ExASetOfExceptionsIsCaught)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="text">Text of message.</param>
    public AggregateException(string text)
      : base(text)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="innerException">Inner exception.</param>
    public AggregateException(string message, Exception innerException) 
      : base(message, innerException)
    {
      Exceptions = new[] { innerException };
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(Exception[] exceptions) 
      : base(Strings.ExASetOfExceptionsIsCaught, exceptions.First())
    {
      Exceptions = exceptions;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(string message, Exception[] exceptions) 
      : base(message, exceptions.First())
    {
      Exceptions = exceptions;
    }
  }
}