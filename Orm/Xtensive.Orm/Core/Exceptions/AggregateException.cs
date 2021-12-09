// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Linq;

namespace Xtensive.Core
{
  /// <summary>
  /// Aggregates a set of caught exceptions.
  /// </summary>
  [Serializable]
  public class AggregateException : Exception
  {
    private Exception[] exceptions;

    /// <summary>
    /// Gets the list of caught exceptions.
    /// </summary>
    public IReadOnlyList<Exception> Exceptions
    {
      [DebuggerStepThrough]
      get { return exceptions; }
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

      foreach (var exception in exceptions) {
        var ae = exception as AggregateException;
        if (ae!=null)
          result.AddRange(ae.GetFlatExceptions());
        else
          result.Add(exception);
      }

      return result;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(64);
      sb.Append(base.ToString());
      sb.AppendFormat("\r\n{0}:", Strings.OriginalExceptions);
      int i = 1;
      foreach (Exception exception in exceptions)
        sb.AppendFormat("\r\n{0}: {1}", i++, exception);
      return sb.ToString();
    }

    #region Private \ internal methods

    private void SetExceptions(Exception[] exceptions)
    {
      this.exceptions = exceptions;
    }

    private void SetExceptions(Exception exception)
    {
      exceptions =  new Exception[] { exception };
    }

    #endregion


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
      SetExceptions(innerException);
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(Exception[] exceptions) 
      : base(Strings.ExASetOfExceptionsIsCaught, exceptions.First())
    {
      SetExceptions(exceptions);
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(string message, Exception[] exceptions) 
      : base(message, exceptions.First())
    {
      SetExceptions(exceptions);
    }


    // Serialization

    /// <summary>
    /// Deserializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected AggregateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      exceptions = (Exception[]) info.GetValue("Exceptions", typeof (Exception[]));
    }

    /// <summary>
    /// Serializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Exceptions", exceptions);
    }
  }
}