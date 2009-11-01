// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Aggregates a set of caught exceptions.
  /// </summary>
  [Serializable]
  public class AggregateException : Exception,
    IHasExceptions<Exception>
  {
    private List<Exception> exceptions = new List<Exception>();

    public IEnumerable<Exception> Exceptions
    {
      get { return exceptions; }
    }

    public void Add(Exception exception)
    {
      exceptions.Add(exception);
    }

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

    private static Exception ExtractFirstException(IEnumerable<Exception> exceptions)
    {
      if (exceptions==null)
        return null;
      foreach (Exception e in exceptions)
        return e;
      return null;
    }


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
      Add(innerException);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(IEnumerable<Exception> exceptions) 
      : this(Strings.ExASetOfExceptionsIsCaught, ExtractFirstException(exceptions))
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="message">Text of message.</param>
    /// <param name="exceptions">Inner exceptions.</param>
    public AggregateException(string message, IEnumerable<Exception> exceptions) 
      : base(message, ExtractFirstException(exceptions))
    {
      foreach (Exception e in exceptions)
        Add(e);
    }


    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected AggregateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      exceptions = (List<Exception>)info.GetValue("Exceptions", typeof (List<Exception>));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("Exceptions", exceptions);
    }
  }
}