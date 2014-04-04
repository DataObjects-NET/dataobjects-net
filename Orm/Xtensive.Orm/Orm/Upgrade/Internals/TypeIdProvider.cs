// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class TypeIdProvider : ITypeIdProvider
  {
    private Dictionary<string, string> movedToAnotherNamespaceTypes;
    private readonly UpgradeContext context;
    private readonly Dictionary<StoredTypeInfo, TypeInfo> typeMapping;
    private readonly Dictionary<TypeInfo, StoredTypeInfo> reverseTypeMapping;

    public int GetTypeId(Type type)
    {
      var typeId = TypeInfo.NoTypeId;
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null && context.ExtractedTypeMap==null)
        return typeId;

      // type has been renamed?
      var fullName = type.GetFullName();
      var renamer = context.Hints
        .OfType<RenameTypeHint>()
        .SingleOrDefault(hint => hint.NewType.GetFullName()==fullName);
      if (renamer!=null) {
        if (context.ExtractedTypeMap.TryGetValue(renamer.OldType, out typeId))
          return typeId;
        if (oldModel!=null) {
          var oldType = oldModel.Types.SingleOrDefault(t => t.UnderlyingType==renamer.OldType);
          if (oldType!=null)
            return oldType.TypeId;
        }
        // If RenameTypeHint is specified
        // but we can't find old type in any available source fail immediately
        throw new InvalidOperationException(string.Format(Strings.ExTypeXIsNotFound, renamer.OldType));
      }

      // type has been preserved
      if (context.ExtractedTypeMap.TryGetValue(fullName, out typeId))
        return typeId;
      if (oldModel!=null) {
        var oldType = oldModel.Types.SingleOrDefault(t => t.UnderlyingType==fullName);
        if (oldType!=null)
          return oldType.TypeId;
      }
      
      if (TryGetTypeIdFromMovedToAnotherNamespaceTypes(fullName, out typeId))
        return typeId;
  
      return TypeInfo.NoTypeId;
    }
    
    private bool TryGetTypeIdFromMovedToAnotherNamespaceTypes(string typeName, out int typeId)
    {
      typeId = TypeInfo.NoTypeId;
      if (context.Session==null || context.ExtractedDomainModel==null)
        return false;

      var hints = new NativeTypeClassifier<UpgradeHint>(true);
      hints.AddRange(context.Hints);

      var renameTypeHints = hints.GetItems<RenameTypeHint>().ToList();
      var removeTypeHints = hints.GetItems<RemoveTypeHint>().ToList();

      BuildTypeMapping(renameTypeHints, removeTypeHints);

      var oldName = TryFindOutOldTypeName(typeName);
      
      if (!string.IsNullOrEmpty(oldName))
        return context.ExtractedTypeMap.TryGetValue(oldName, out typeId);
      return false;
    }

    private string TryFindOutOldTypeName(string newName)
    {
      string oldName;
      if (movedToAnotherNamespaceTypes.TryGetValue(newName, out oldName))
        return oldName;
      return string.Empty;
    }

    private void BuildTypeMapping(IEnumerable<RenameTypeHint> renames, IEnumerable<RemoveTypeHint> removes)
    {
      ClearMappings();

      // Excluding EntitySetItem<TL,TR> descendants.
      // They're not interesting at all for us, since
      // these types aren't ever referenced.

      var oldModelTypes = GetNonConnectorTypes(context.ExtractedDomainModel);
      var currentModel = context.Session.Domain.Model;

      var newConnectorTypes = currentModel.Associations
        .Select(association => association.AuxiliaryType)
        .Where(type => type != null)
        .ToHashSet();

      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType.FullName);

      var renameLookup = renames.ToDictionary(hint => hint.OldType);
      var removeLookup = removes.ToDictionary(hint => hint.Type);

      // Types that are neither mapped nor removed.
      var suspiciousTypes = new List<StoredTypeInfo>();

      // Mapping types
      foreach (var oldType in oldModelTypes) {
        var removeTypeHint = removeLookup.GetValueOrDefault(oldType.UnderlyingType);
        if (removeTypeHint!=null)
          continue;
        var renameTypeHint = renameLookup.GetValueOrDefault(oldType.UnderlyingType);
        var newTypeName = renameTypeHint!=null
          ? renameTypeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        var newType = newModelTypes.GetValueOrDefault(newTypeName);
        if (newType==null)
          suspiciousTypes.Add(oldType);
      }

      if (suspiciousTypes.Count==0)
        return;

      // Now we'll lookup by using DO type name instead of CLR type name
      // By default DO type name is a CLR type name without namespace
      // however this could be adjusted by domain configuration.
      // If CLR name is changed but DO name is preserved we should
      // automatically process this type without extra hints.

      newModelTypes = newModelTypes.Values.ToDictionary(t => t.Name);

      foreach (var oldType in suspiciousTypes) {
        var newType = newModelTypes.GetValueOrDefault(oldType.Name);
        if (newType!=null && !reverseTypeMapping.ContainsKey(newType))
          MapType(oldType, newType);
      }

      movedToAnotherNamespaceTypes = reverseTypeMapping
        .Where(el => el.Key.UnderlyingType.FullName!=el.Value.UnderlyingType)
        .ToDictionary(pair => pair.Key.UnderlyingType.FullName, pair => pair.Value.UnderlyingType);
    }

    private IEnumerable<StoredTypeInfo> GetNonConnectorTypes(StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type!=null
        select type
        ).ToHashSet();
      return model.Types.Where(type => !connectorTypes.Contains(type));
    }

    private void MapType(StoredTypeInfo oldType, TypeInfo newType)
    {
      TypeInfo existingNewType;
      if (typeMapping.TryGetValue(oldType, out existingNewType)){
        throw new InvalidOperationException(string.Format(
          Strings.ExUnableToAssociateTypeXWithTypeYTypeXIsAlreadyMappedToTypeZ,
          oldType, newType, existingNewType));
      }
      typeMapping[oldType] = newType;
      reverseTypeMapping[newType] = oldType;
    }

    public void ClearMappings()
    {
      typeMapping.Clear();
      reverseTypeMapping.Clear();
      movedToAnotherNamespaceTypes.Clear();
    }

    // Constructors

    public TypeIdProvider(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      this.context = context;
      typeMapping = new Dictionary<StoredTypeInfo, TypeInfo>();
      reverseTypeMapping = new Dictionary<TypeInfo, StoredTypeInfo>();
      movedToAnotherNamespaceTypes = new Dictionary<string, string>();
    }
  }
}