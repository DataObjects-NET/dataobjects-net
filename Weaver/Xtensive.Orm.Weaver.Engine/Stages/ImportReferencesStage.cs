// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ImportReferencesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      var registry = context.References;
      var mscorlibAssembly = context.TargetModule.TypeSystem.Corlib;

      var coreAssembly = FindReference(context, WellKnown.CoreAssemblyFullName);
      if (coreAssembly==null)
        return ActionResult.Failure;
      registry.CoreAssembly = coreAssembly;

      var ormAssembly = FindReference(context, WellKnown.OrmAssemblyFullName);
      if (ormAssembly==null)
        return ActionResult.Failure;
      registry.OrmAssembly = ormAssembly;

      // mscorlib
      registry.SerializationInfo = ImportType(context, mscorlibAssembly, "System.Runtime.Serialization", "SerializationInfo");
      registry.StreamingContext = ImportType(context, mscorlibAssembly, "System.Runtime.Serialization", "StreamingContext");

      // Xtensive.Core
      registry.Tuple = ImportType(context, coreAssembly, "Xtensive.Tuples", "Tuple");

      // Xtensive.Orm
      registry.Session = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "Session");
      registry.EntityState = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "EntityState");
      registry.Persistent = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "Persistent");
      registry.FieldInfo = ImportType(context, ormAssembly, WellKnown.OrmNamespace, "FieldInfo");

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

    private AssemblyNameReference FindReference(ProcessorContext context, string assemblyName)
    {
      var comparer = AssemblyResolver.AssemblyNameComparer;
      var reference = context.TargetModule.AssemblyReferences
        .FirstOrDefault(r => comparer.Equals(r.FullName, assemblyName));

      if (reference==null) {
        context.Logger.Write(MessageCode.ErrorTargetAssemblyHasNoExpectedReference, assemblyName);
        return null;
      }

      return reference;
    }
  }
}