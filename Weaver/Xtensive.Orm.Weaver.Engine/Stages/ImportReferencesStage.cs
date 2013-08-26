// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
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
      registry.SerializationInfo = ImportType(context, mscorlibAssembly, "System.Runtime.Serialization.SerializationInfo");
      registry.StreamingContext = ImportType(context, mscorlibAssembly, "System.Runtime.Serialization.StreamingContext");

      registry.CompilerGeneratedAttributeConstructor = ImportDefaultConstructor(context, mscorlibAssembly, WellKnown.CompilerGeneratedAttribute);

      // Xtensive.Core
      registry.Tuple = ImportType(context, coreAssembly, "Xtensive.Tuples.Tuple");

      // Xtensive.Orm
      registry.Session = ImportType(context, ormAssembly, "Xtensive.Orm.Session");
      registry.EntityState = ImportType(context, ormAssembly, "Xtensive.Orm.EntityState");
      registry.Persistent = ImportType(context, ormAssembly, "Xtensive.Orm.Persistent");
      registry.FieldInfo = ImportType(context, ormAssembly, "Xtensive.Orm.FieldInfo");

      var getValueParameter = new GenericParameter(0, GenericParameterType.Method, context.TargetModule);
      registry.PersistentGetterDefinition = new MethodReference("GetFieldValue", getValueParameter, registry.Persistent);
      registry.PersistentGetterDefinition.GenericParameters.Add(getValueParameter);

      var setValueParameter = new GenericParameter(0, GenericParameterType.Method, context.TargetModule);
      registry.PersistentSetterDefinition = new MethodReference("SetFieldValue", context.TargetModule.TypeSystem.Void, registry.Persistent);
      registry.PersistentSetterDefinition.GenericParameters.Add(setValueParameter);

      registry.ProcessedByWeaverAttributeConstructor = ImportDefaultConstructor(context, ormAssembly, WellKnown.ProcessedByWeaverAttribute);
      registry.EntityTypeAttributeConstructor = ImportDefaultConstructor(context, ormAssembly, WellKnown.EntityTypeAttribute);
      registry.EntitySetTypeAttributeConstructor = ImportDefaultConstructor(context, ormAssembly, WellKnown.EntitySetTypeAttribute);
      registry.EntityInterfaceAttributeConstructor = ImportDefaultConstructor(context, ormAssembly, WellKnown.EntityInterfaceAttribute);
      registry.StructureTypeAttributeConstructor = ImportDefaultConstructor(context, ormAssembly, WellKnown.StructureTypeAttribute);

      return ActionResult.Success;
    }

    private TypeReference ImportType(ProcessorContext context, IMetadataScope assembly, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var reference = new TypeReference(@namespace, name, targetModule, assembly);
      return targetModule.Import(reference);
    }

    private TypeReference ImportType(ProcessorContext context, IMetadataScope assembly, string fullName)
    {
      var splitName = SplitTypeName(fullName);
      return ImportType(context, assembly, splitName.Item1, splitName.Item2);
    }

    private MethodReference ImportDefaultConstructor(ProcessorContext context, IMetadataScope assembly, string @namespace, string name)
    {
      var targetModule = context.TargetModule;
      var typeReference = new TypeReference(@namespace, name, targetModule, assembly);
      var constructorReference = new MethodReference(WellKnown.Constructor, targetModule.TypeSystem.Void, typeReference);
      return targetModule.Import(constructorReference);
    }

    private MethodReference ImportDefaultConstructor(ProcessorContext context, IMetadataScope assembly, string fullName)
    {
      var splitName = SplitTypeName(fullName);
      return ImportDefaultConstructor(context, assembly, splitName.Item1, splitName.Item2);
    }

    private AssemblyNameReference FindReference(ProcessorContext context, string assemblyName)
    {
      var comparer = WeavingHelper.AssemblyNameComparer;
      var reference = context.TargetModule.AssemblyReferences
        .FirstOrDefault(r => comparer.Equals(r.FullName, assemblyName));

      if (reference==null) {
        context.Logger.Write(MessageCode.ErrorTargetAssemblyHasNoExpectedReference, assemblyName);
        return null;
      }

      return reference;
    }

    private static Tuple<string, string> SplitTypeName(string fullName)
    {
      var index = fullName.IndexOf(".", StringComparison.InvariantCulture);
      if (index < 0)
        return Tuple.Create(String.Empty, fullName);
      return Tuple.Create(
        fullName.Substring(0, index),
        fullName.Substring(index + 1));
    }
  }
}