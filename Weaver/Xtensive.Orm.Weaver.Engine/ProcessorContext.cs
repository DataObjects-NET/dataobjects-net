// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ProcessorContext
  {
    public ProcessorParameterSet Parameters { get; set; }

    public MessageLogger Logger { get; set; }

    public IList<WeavingTask> WeavingTasks { get; set; }

    public ModuleDefinition TargetModule { get; set; }
  }
}