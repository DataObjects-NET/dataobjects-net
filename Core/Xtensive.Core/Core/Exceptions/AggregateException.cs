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
using System.Security.Permissions;
using System.Text;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Aggregates a set of caught exceptions.
  /// </summary>
  [Serializable]
  public class AggregateException : Exception
  {
    private ReadOnlyList<Exception> exceptions;

    /// <summary>
    /// Gets the list of caught exceptions.
    /// </summary>
    public ReadOnlyList<Exception> Exceptions
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

    private void SetExceptions(IEnumerable<Exception> exceptions)
    {
      var list = exceptions as IList<Exception> ?? exceptions.ToList();
      this.exceptions = new ReadOnlyList<Exception>(list);
    }

    private void SetExceptions(Exception exception)
    {
      var list = new List<Exception>();
      list.Add(exception);
      exceptions = new ReadOnlyList<Exception>(list);
    }

    #endregion


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public AggregateException()
      : base(Strings.ExASetOfExceptionsIsCaught)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="text">Text of message.</param>
    public AggregateException(string text)
      : base(text)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="innerException">Inner exception.</param>
    public AggregateException(string message, Exception innerException) 
      : base(message, innerException)
    {
      SetExceptions(innerException);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(IEnumerable<Exception> exceptions) 
      : base(Strings.ExASetOfExceptionsIsCaught, exceptions.First())
    {
      SetExceptions(exceptions);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(string message, IEnumerable<Exception> exceptions) 
      : base(message, exceptions.First())
    {
      SetExceptions(exceptions);
    }


    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected AggregateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      exceptions = (ReadOnlyList<Exception>)info.GetValue("Exceptions", typeof (ReadOnlyList<Exception>));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Exceptions", exceptions);
    }
  }
}