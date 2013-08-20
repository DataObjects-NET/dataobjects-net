// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Inspections
{
  internal sealed class ReferenceImporter : Inspector
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var registry = context.References;

      registry.SerializationInfo = ImportSystemType(context, "System.Runtime.Serialization", "SerializationInfo");
      registry.StreamingContext = ImportSystemType(context, "System.Runtime.Serialization", "StreamingContext");

      registry.Session = ImportOrmType(context, WellKnown.OrmNamespace, "Session");
      registry.EntityState = ImportOrmType(context, WellKnown.OrmNamespace, "EntityState");

      return ActionResult.Success;
    }

    private TypeReference ImportOrmType(ProcessorContext context, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var reference = new TypeReference(@namespace, name, targetModule, context.References.OrmAssembly);
      return targetModule.Import(reference);
    }

    private TypeReference ImportSystemType(ProcessorContext context, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var reference = new TypeReference(@namespace, name, targetModule, targetModule.TypeSystem.Corlib);
     return targetModule.Import(reference);
    }
  }
}