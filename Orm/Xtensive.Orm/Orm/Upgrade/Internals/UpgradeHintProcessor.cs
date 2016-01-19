using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal static class DomainModelExtensions
  {
    public static ClassifiedCollection<Type, Pair<Type, Type[]>> GetGenericTypes(this DomainModel model)
    {
      var genericTypes = new ClassifiedCollection<Type, Pair<Type, Type[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.UnderlyingType.IsGenericType)) {
        var typeDefinition = typeInfo.UnderlyingType.GetGenericTypeDefinition();
        genericTypes.Add(new Pair<Type, Type[]>(typeDefinition, typeInfo.UnderlyingType.GetGenericArguments()));
      }
      return genericTypes;
    }
  }

  internal static class StoredDomainModelExtensions
  {
    public static ClassifiedCollection<string, Pair<string, string[]>> GetGenericTypes(this StoredDomainModel model)
    {
      var genericTypes = new ClassifiedCollection<string, Pair<string, string[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        var typeDefinitionName = typeInfo.GenericTypeDefinition;
        genericTypes.Add(new Pair<string, string[]>(typeDefinitionName, typeInfo.GenericArguments));
      }
      return genericTypes;
    }

    public static IEnumerable<StoredTypeInfo> GetNonConnectorTypes(this StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type!=null
        select type
        ).ToHashSet();
      return model.Types.Where(type => !connectorTypes.Contains(type));
    }
  }

  internal sealed class UpgradeHintsProcessorResult
  {

  }


  internal sealed class UpgradeHintsProcessor
  {
    private readonly bool autoDetectTypesMovements = true;

    private readonly NativeTypeClassifier<UpgradeHint> hints;
    private readonly StoredDomainModel currentModel;
    private readonly StoredDomainModel extractedModel;
    private readonly DomainModel domainModel;

    //private readonly IStoredTypeMapper typeMapper;

    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> reverseFieldMapping;

    public UpgradeHintsProcessorResult Process(IEnumerable<UpgradeHint> inputHints)
    {
      ProcessGenericTypeHints(inputHints);
      ProcessTypeChanges();
      ProcessFieldChanges();
      ProcessConnectorTypes();


      return new UpgradeHintsProcessorResult();
    }

    private void ProcessGenericTypeHints(IEnumerable<UpgradeHint> inputHints)
    {
      hints.AddRange(RewriteGenericTypeHints(inputHints));
    }

    private void ProcessTypeChanges()
    {
      var renameTypeHints = hints.GetItems<RenameTypeHint>().ToChainedBuffer();
      var removeTypeHints = hints.GetItems<RemoveTypeHint>().ToChainedBuffer();
      BuildTypeMapping(renameTypeHints, removeTypeHints);
    }

    private void ProcessFieldChanges()
    {
      var renameFieldHints = hints.GetItems<RenameFieldHint>().ToChainedBuffer();
      var changeFieldTypeHints = hints.GetItems<ChangeFieldTypeHint>().ToChainedBuffer();
      BuildFieldMapping(renameFieldHints, changeFieldTypeHints);
    }

    private void ProcessConnectorTypes()
    {
      BuildConnectorTypeMapping();
    }

    private IEnumerable<UpgradeHint> RewriteGenericTypeHints(IEnumerable<UpgradeHint> hints)
    {
      // Prepare data
      Dictionary<string, RenameTypeHint> renamedTypesLookup;
      ChainedBuffer<RenameTypeHint> renameGenericTypeHints;
      ChainedBuffer<RenameFieldHint> renameFieldHints;
      GetGenericRenameHints(hints, out renamedTypesLookup, out renameGenericTypeHints, out renameFieldHints);
      var oldGenericTypes = extractedModel.GetGenericTypes();
      var newGenericTypes = domainModel.GetGenericTypes();

      // Build generic types mapping
      var genericTypeMapping = new List<Triplet<string, Type, List<Pair<string, Type>>>>();
      var newTypesLookup = newGenericTypes.GetClasses().ToDictionary(t => t.GetFullName());
      foreach (var oldGenericDefName in oldGenericTypes.GetClasses()) {
        var newGenericDefType = GetNewType(oldGenericDefName, newTypesLookup, renamedTypesLookup);
        if (newGenericDefType==null)
          continue;
        foreach (var pair in oldGenericTypes.GetItems(oldGenericDefName)) {
          var genericArgumentsMapping = new List<Pair<string, Type>>();
          foreach (var oldGenericArgumentType in pair.Second) {
            var newGenericArgumentType = GetNewType(oldGenericArgumentType, newTypesLookup, renamedTypesLookup);
            if (newGenericArgumentType==null)
              break;
            genericArgumentsMapping.Add(new Pair<string, Type>(oldGenericArgumentType, newGenericArgumentType));
          }
          if (genericArgumentsMapping.Count==pair.Second.Length)
            genericTypeMapping.Add(new Triplet<string, Type, List<Pair<string, Type>>>(
              oldGenericDefName, newGenericDefType, genericArgumentsMapping));
        }
      }

      // Build rename generic type hints
      var rewrittenHints = new ChainedBuffer<UpgradeHint>();
      foreach (var triplet in genericTypeMapping) {
        var oldGenericArguments = triplet.Third.Select(pair => pair.First).ToArray();
        var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
        var oldTypeFullName = GetGenericTypeFullName(triplet.First, oldGenericArguments);
        var newType = triplet.Second.MakeGenericType(newGenericArguments);
        if (oldTypeFullName!=newType.GetFullName())
          rewrittenHints.Add(new RenameTypeHint(oldTypeFullName, newType));
      }

      var genericTypeDefLookup = (
        from triplet in genericTypeMapping
        group triplet by triplet.Second.GetGenericTypeDefinition()
          into g
          select new { Definition = g.Key, Instances = g.ToArray() }
        ).ToDictionary(g => g.Definition);

      // Build rename generic type field hints
      foreach (var hint in renameFieldHints) {
        var newGenericDefType = hint.TargetType;
        var instanceGroup = genericTypeDefLookup.GetValueOrDefault(newGenericDefType);
        if (instanceGroup==null)
          continue;
        foreach (var triplet in instanceGroup.Instances) {
          var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
          rewrittenHints.Add(new RenameFieldHint(newGenericDefType.MakeGenericType(newGenericArguments),
            hint.OldFieldName, hint.NewFieldName));
        }
      }

      // Return new hint set
      return hints
        .Except(renameGenericTypeHints.Cast<UpgradeHint>())
        .Except(renameFieldHints.Cast<UpgradeHint>())
        .Concat(rewrittenHints);
    }

    private void BuildTypeMapping(ICollection<RenameTypeHint> renameTypeHints, ICollection<RemoveTypeHint> removeTypeHints)
    {
      // Excluding EntitySetItem<TL,TR> descendants.
      // They're not interesting at all for us, since
      // these types aren't ever referenced.
      var oldModelTypes = extractedModel.GetNonConnectorTypes();

      var newConnectorTypes = currentModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type != null)
        .ToHashSet();

      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType);

      var renameLookup = renameTypeHints.ToDictionary(hint => hint.OldType);
      var removeLookup = removeTypeHints.ToDictionary(hint => hint.Type);

      // Types that are neither mapped nor removed.
      var suspiciousTypes = new List<StoredTypeInfo>();

      // Mapping types
      foreach (var oldType in oldModelTypes) {
        var removeTypeHint = removeLookup.GetValueOrDefault(oldType.UnderlyingType);
        if (removeTypeHint != null)
          continue;
        var renameTypeHint = renameLookup.GetValueOrDefault(oldType.UnderlyingType);
        var newTypeName = renameTypeHint!=null
          ? renameTypeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        var newType = newModelTypes.GetValueOrDefault(newTypeName);
        if (newType!=null)
          MapType(oldType, newType);
        else
          //todo:// movements detection should be implemented by other processor
          if (autoDetectTypesMovements)
            suspiciousTypes.Add(oldType);
      }

      if (suspiciousTypes.Count == 0)
        return;

      // Now we'll lookup by using DO type name instead of CLR type name
      // By default DO type name is a CLR type name without namespace
      // however this could be adjusted by domain configuration.
      // If CLR name is changed but DO name is preserved we should
      // automatically process this type without extra hints.

      newModelTypes = newModelTypes.Values.ToDictionary(t => t.Name);

      foreach (var oldType in suspiciousTypes) {
        var newType = newModelTypes.GetValueOrDefault(oldType.Name);
        if (newType != null && !reverseTypeMapping.ContainsKey(newType))
          MapType(oldType, newType);
      }
    }

    private void BuildFieldMapping(ICollection<RenameFieldHint> renameFieldHints, ICollection<ChangeFieldTypeHint> changeFieldTypeHints)
    {
      
    }

    private void BuildConnectorTypeMapping()
    {
      var oldAssociations = extractedModel.Associations
        .Where(association => association.ConnectorType != null);
      foreach (var oldAssociation in oldAssociations)
      {
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType))
          continue;

        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;

        var newReferencingType = typeMapping.GetValueOrDefault(oldReferencingType);
        if (newReferencingType == null)
          continue;

        var newReferencingField = fieldMapping.GetValueOrDefault(oldReferencingField);
        if (newReferencingField == null)
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name == oldReferencingField.Name);
        if (newReferencingField == null)
          continue;

        var newAssociation = currentModel.Associations
          .SingleOrDefault(association => association.ReferencingField == newReferencingField);
        if (newAssociation == null || newAssociation.ConnectorType == null)
          continue;

        MapType(oldAssociation.ConnectorType, newAssociation.ConnectorType);

        var oldMaster = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.MasterFieldName);
        var newMaster = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.MasterFieldName);
        var oldSlave = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.SlaveFieldName);
        var newSlave = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name == WellKnown.SlaveFieldName);

        MapFieldRecursively(oldMaster, newMaster);
        MapFieldRecursively(oldSlave, newSlave);
      }
    }

    private static string GetGenericTypeFullName(string genericDefinitionTypeName, string[] genericArgumentNames)
    {
      return string.Format("{0}<{1}>", genericDefinitionTypeName.Replace("<>", string.Empty),
        genericArgumentNames.ToCommaDelimitedString());
    }

    private IEnumerable<UpgradeHint> RewriteMoveFieldHints(IEnumerable<MoveFieldHint> moveFieldHints)
    {
      foreach (var hint in moveFieldHints)
      {
        yield return new CopyFieldHint(hint.SourceType, hint.SourceField, hint.TargetType, hint.TargetField);
        yield return new RemoveFieldHint(hint.SourceType, hint.SourceField);
      }
    }

    private void GetGenericRenameHints(IEnumerable<UpgradeHint> hints, out Dictionary<string, RenameTypeHint> renameTypeHints, out ChainedBuffer<RenameTypeHint> renameGenericTypeHints, out ChainedBuffer<RenameFieldHint> renameFieldHints)
    {
      renameTypeHints = new Dictionary<string, RenameTypeHint>();
      renameGenericTypeHints = new ChainedBuffer<RenameTypeHint>();
      renameFieldHints = new ChainedBuffer<RenameFieldHint>();

      foreach (var upgradeHint in hints)
      {
        var renameTypeHint = upgradeHint as RenameTypeHint;
        if (renameTypeHint != null)
        {
          renameTypeHints.Add(renameTypeHint.OldType, renameTypeHint);
          if (renameTypeHint.NewType.IsGenericTypeDefinition)
            renameGenericTypeHints.Add(renameTypeHint);
        }
        var renameFieldHint = upgradeHint as RenameFieldHint;
        if (renameFieldHint != null && renameFieldHint.TargetType.IsGenericTypeDefinition)
          renameFieldHints.Add(renameFieldHint);
      }
    }

    private void MapType(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      StoredTypeInfo existingNewType;
      if (typeMapping.TryGetValue(oldType, out existingNewType)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateTypeXWithTypeYTypeXIsAlreadyMappedToTypeZ,
          oldType, newType, existingNewType));
      }
      typeMapping[oldType] = newType;
      reverseTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      StoredFieldInfo existingNewField;
      if (fieldMapping.TryGetValue(oldField, out existingNewField)) {
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateFieldXWithFieldYFieldXIsAlreadyMappedToFieldZ,
          oldField, newField, existingNewField));
      }
      fieldMapping[oldField] = newField;
      reverseFieldMapping[newField] = oldField;
    }

    private void MapNestedFields(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      var oldNestedFields = ((IEnumerable<StoredFieldInfo>)oldField.Fields).ToArray();
      if (oldNestedFields.Length==0)
        return;
      var oldValueType = extractedModel.Types
        .Single(type => type.UnderlyingType == oldField.ValueType);
      foreach (var oldNestedField in oldNestedFields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = oldValueType.AllFields
          .Single(field => field.Name==oldNestedFieldOriginalName);
        if (!fieldMapping.ContainsKey(oldNestedFieldOrigin))
          continue;
        var newNestedFieldOrigin = fieldMapping[oldNestedFieldOrigin];
        var newNestedField = newField.Fields
          .Single(field => field.OriginalName==newNestedFieldOrigin.Name);
        MapFieldRecursively(oldNestedField, newNestedField);
      }
    }

    private void MapFieldRecursively(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      MapField(oldField, newField);
      MapNestedFields(oldField, newField);
    }

    private static Type GetNewType(string oldTypeName, Dictionary<string, Type> newTypes, Dictionary<string, RenameTypeHint> hints)
    {
      RenameTypeHint hint;
      Type newType;
      return hints.TryGetValue(oldTypeName, out hint)
        ? hint.NewType
        : (newTypes.TryGetValue(oldTypeName, out newType) ? newType : null);
    }
  }
}
