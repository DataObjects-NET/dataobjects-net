// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ProcessorContext : IDisposable
  {
    public ProcessorConfiguration Configuration { get; set; }

    public MessageLogger Logger { get; set; }

    public IList<WeavingTask> WeavingTasks { get; set; }

    public ModuleDefinition TargetModule { get; set; }

    public ReferenceRegistry References { get; set; }

    public IAssemblyResolver AssemblyResolver { get; set; }

    public IMetadataResolver MetadataResolver { get; set; }

    public bool HasTransformations { get { return WeavingTasks.Count > 0; } }

    public IList<TypeDefinition> EntityTypes { get; set; }

    public IList<TypeDefinition> StructureTypes { get; set; }

    public void Dispose()
    {
    }

    public ProcessorContext()
    {
      WeavingTasks = new List<WeavingTask>();
      References = new ReferenceRegistry();
      EntityTypes = new List<TypeDefinition>();
      StructureTypes = new List<TypeDefinition>();
    }
  }
}