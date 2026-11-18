// Copyright (C) 2013-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ModifyPersistentTypesStage : ProcessorStage
  {
    private IPersistentPropertyChecker propertyChecker;

    private TypeReference[][] entityFactorySignatures;
    private TypeReference[][] structureFactorySignatures;
    private TypeReference[][] entitySetFactorySignatures;

    public override ActionResult Execute(ProcessorContext context)
    {
      var references = context.References;

      entityFactorySignatures = [
        [references.Session, references.EntityState],
      ];

      structureFactorySignatures = [
        [references.Session, references.Tuple],
        [references.Persistent, references.FieldInfo],
      ];

      entitySetFactorySignatures = [
        [references.Entity, references.FieldInfo],
      ];

      propertyChecker = (context.Language is SourceLanguage.CSharp) 
        ? context.ReadOnlyProperties
          ? new CsPropertyCheckerRelaxed()
          : new CsPropertyCheckerStrict() 
        : new VbPropertyChecker();

      foreach (var type in context.PersistentTypes) {
        switch (type.Kind) {
          case PersistentTypeKind.Entity:
            ProcessEntity(context, type);
            break;
          case PersistentTypeKind.EntitySet:
            ProcessEntitySet(context, type);
            break;
          case PersistentTypeKind.EntityInterface:
            ProcessEntityInterface(context, type);
            break;
          case PersistentTypeKind.Structure:
            ProcessStructure(context, type);
            break;
        }
      }

      if (propertyChecker.HasSkippedProperties) {
        context.Logger.Write(MessageCode.ErrorPersistentPropertiesWereNotProcessed);
        return ActionResult.Failure;
      }

      return ActionResult.Success;
    }

    private void ProcessEntityInterface(ProcessorContext context, TypeInfo type)
    {
      context.WeavingTasks.Add(new AddAttributeTask(type.Definition, context.References.EntityInterfaceAttributeConstructor));
    }

    private void ProcessEntitySet(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in entitySetFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.EntitySetTypeAttributeConstructor));
    }

    private void ProcessEntity(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in entityFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      var inCtorInitializedFields = ProcessFields(context, type, out var hasReadOnlyProps);
      ProcessConstructors(context, type, inCtorInitializedFields, hasReadOnlyProps);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.EntityTypeAttributeConstructor));
    }

    private void ProcessStructure(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in structureFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      var inCtorInitializedFields = ProcessFields(context, type, out var hasReadOnlyProps);
      ProcessConstructors(context, type, inCtorInitializedFields, hasReadOnlyProps);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.StructureTypeAttributeConstructor));
    }

    private Dictionary<FieldReference, PropertyInfo> ProcessFields(ProcessorContext context, TypeInfo type, out bool hasReadOnlyProperties)
    {
      var backingFieldMap = new Dictionary<FieldReference, PropertyInfo>();
      hasReadOnlyProperties = false;

      foreach (var property in type.Properties.Values.Where(static p => p.IsPersistent)) {
        // checks whether there is both getter and setter
        if (!propertyChecker.ShouldProcess(property, context))
          continue;

        var typeDefinition = type.Definition;
        var propertyDefinition = property.Definition;
        var isReadOnly = property.IsReadOnly;
        var persistentName = property.PersistentName ?? property.Name;

        if (isReadOnly)
          hasReadOnlyProperties = true;

        var backingField = propertyDefinition.GetBackingField();
        if (backingField is not null)
          backingFieldMap.Add(backingField, property);

        // Backing field
        context.WeavingTasks.Add(new RemoveBackingFieldTask(typeDefinition, propertyDefinition));
        // Getter
        context.WeavingTasks.Add(new ImplementFieldAccessorTask(AccessorKind.Getter,
          typeDefinition, propertyDefinition, persistentName));
        // Setter
        if (!isReadOnly) {
          context.WeavingTasks.Add((property.IsKey)
            ? new ImplementKeySetterTask(typeDefinition, propertyDefinition)
            : new ImplementFieldAccessorTask(AccessorKind.Setter, typeDefinition, propertyDefinition, persistentName));
        }
        if (property.PersistentName is not null)
          context.WeavingTasks.Add(
            new AddAttributeTask(propertyDefinition, context.References.OverrideFieldNameAttributeConstructor, property.PersistentName));
      }
      return backingFieldMap;
    }

    private static void ProcessConstructors(ProcessorContext context, TypeInfo type,
      Dictionary<FieldReference, PropertyInfo> fieldsInitializations, bool hasReadOnlyProps)
    {
      var userConstructors = type.Definition.Methods
        .Where(static m => m.IsConstructor && !m.IsStatic && !m.HasAttribute(WellKnown.CompilerGeneratedAttribute));
      // the order matters
      // 1st - expects unchanged ctor, it handles (base ctor call, last backing field call] window
      // 2nd - hadles [initial instuction, base ctor call) window, moves base constructor call to the top
      // 3rd - wraps all body instructions
      foreach (var constructor in userConstructors) {
        if (context.ReadOnlyProperties && hasReadOnlyProps)
          context.WeavingTasks.Add(new RewriteReadOnlyPropertyInitializersTask(type.Definition, constructor, fieldsInitializations));
        if (context.AutoPropertyInitializers)
          context.WeavingTasks.Add(new RewriteAutoPropertyInitializersTask(type.Definition, constructor, fieldsInitializations));
        context.WeavingTasks.Add(new ImplementInitializablePatternTask(type.Definition, constructor));
      }
    }
  }
}