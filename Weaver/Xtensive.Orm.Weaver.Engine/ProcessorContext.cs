// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ProcessorContext
  {
    public ProcessorParameterSet Parameters { get; set; }

    public IMessageWriter MessageWriter { get; set; }

    public IList<ProcessorTask> Tasks { get; set; }
  }
}