// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Collections.Generic;
using Xtensive.Core.Aspects;

namespace Xtensive.Integrity.Atomicity.OperationLogs
{
  public class LoggingMemoryOperationLog: MemoryOperationLog
  {
    [Trace(TraceOptions.Indent | TraceOptions.Enter | TraceOptions.Arguments)]
    public override void Append(IRedoDescriptor redoDescriptor)
    {
      base.Append(redoDescriptor);
    }


    // Constructors

    public LoggingMemoryOperationLog(IEnumerable<IRedoDescriptor> source) 
      : base(source)
    {
    }

    public LoggingMemoryOperationLog()
    {
    }
  }
}