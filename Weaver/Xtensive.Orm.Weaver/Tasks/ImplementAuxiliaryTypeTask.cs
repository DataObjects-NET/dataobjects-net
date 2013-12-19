// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.19

using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal class ImplementAuxiliaryTypeTask : WeavingTask
  {
    private readonly TypeInfo ownerTypeInfo;
    private readonly PropertyInfo property;

    public override ActionResult Execute(ProcessorContext context)
    {
      var references = context.References;
      var factorySignatures = new[] {
        new[] {references.Session, references.EntityState},
        new[] {references.Session, references.Tuple},
      };

      var auxiliaryTypeName = string.Format(WellKnown.AuxiliaryTypeNameFormat, property.Name);
      var auxiliaryType = new TypeDefinition(null, auxiliaryTypeName,
        TypeAttributes.Class | TypeAttributes.NestedPrivate);

      var ownerType = ownerTypeInfo.Definition;

      var masterType = new GenericParameter("TMaster", auxiliaryType);
      masterType.Constraints.Add(references.EntityInterface);
      var slaveType = new GenericParameter("TSlave", auxiliaryType);
      slaveType.Constraints.Add(references.EntityInterface);

      auxiliaryType.GenericParameters.Add(masterType);
      auxiliaryType.GenericParameters.Add(slaveType);

      var baseType = new GenericInstanceType(references.EntitySetItem);
      baseType.GenericArguments.Add(masterType);
      baseType.GenericArguments.Add(slaveType);
      auxiliaryType.BaseType = baseType;

      ownerType.NestedTypes.Add(auxiliaryType);

      foreach (var signature in factorySignatures)
        new ImplementFactoryTask(auxiliaryType, signature).Execute(context);

      new AddAttributeTask(auxiliaryType, references.CompilerGeneratedAttributeConstructor).Execute(context);
      new AddAttributeTask(auxiliaryType, references.AuxiliaryTypeAttributeConstructor).Execute(context);

      return ActionResult.Success;
    }

    public ImplementAuxiliaryTypeTask(TypeInfo ownerTypeInfo, PropertyInfo property)
    {
      this.ownerTypeInfo = ownerTypeInfo;
      this.property = property;
    }
  }
}