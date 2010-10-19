// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.13

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Threading
{
  /// <summary>
  /// Wrapping exception that contains a real exception thrown 
  /// during an execution of a task in <see cref="AsyncProcessor"/>.
  /// </summary>
  [Serializable]
  public sealed class TaskExecutionException : Exception
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="innerException">The exception thrown during a task's execution.</param>
    public TaskExecutionException(Exception innerException) :
      base(Strings.ExExceptionWasThrownDuringTaskExecution, innerException)
    {}


    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected TaskExecutionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {}
  }
}