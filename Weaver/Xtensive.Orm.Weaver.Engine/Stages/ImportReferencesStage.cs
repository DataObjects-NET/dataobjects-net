// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Inspections
{
  internal sealed class ImportReferencesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      if (!FindOrmReference(context))
        return ActionResult.Failure;

      var ormAssembly = context.References.OrmAssembly;
      var coreLibAssembly = context.TargetModule.TypeSystem.Corlib;

      var registry = context.References;

      registry.SerializationInfo = ImportType(context, coreLibAssembly, "System.Runtime.Serialization", "SerializationInfo");
      registry.StreamingContext = ImportType(context, coreLibAssembly, "System.Runtime.Serialization", "StreamingContext");

      registry.Session = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "Session");
      registry.EntityState = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "EntityState");

      registry.ProcessedByWeaverAttributeConstructor = ImportDefaultConstructor(
        context, ormAssembly, WellKnown.OrmNamespace, WellKnown.ProcessedByWeaverAttribute);

      registry.EntityTypeAttributeConstructor = ImportDefaultConstructor(
        context, ormAssembly, WellKnown.OrmNamespace, WellKnown.EntityTypeAttribute);
      registry.StructureTypeAttributeConstructor = ImportDefaultConstructor(
        context, ormAssembly, WellKnown.OrmNamespace, WellKnown.StructureTypeAttribute);

      return ActionResult.Success;
    }

    private TypeReference ImportType(ProcessorContext context, IMetadataScope assembly, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var reference = new TypeReference(@namespace, name, targetModule, assembly);
      return targetModule.Import(reference);
    }

    private MethodReference ImportDefaultConstructor(ProcessorContext context, IMetadataScope assembly, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var typeReference = new TypeReference(@namespace, name, targetModule, assembly);
      var constructorReference = new MethodReference(WellKnown.Constructor, targetModule.TypeSystem.Void, typeReference);
      return targetModule.Import(constructorReference);
    }

    private bool FindOrmReference(ProcessorContext context)
    {
      var comparer = AssemblyResolver.AssemblyNameComparer;
      var ormReference = context.TargetModule.AssemblyReferences
        .FirstOrDefault(r => comparer.Equals(r.FullName, WellKnown.OrmAssemblyFullName));

      if (ormReference==null) {
        context.Logger.Write(MessageCode.ErrorTargetAssemblyHasNoReferenceToOrm);
        return false;
      }

      context.References.OrmAssembly = ormReference;

      return true;
    }
  }
}