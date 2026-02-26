// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Weaving;
using Xtensive.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Name builder for <see cref="Orm.Model.DomainModel"/> nodes
  /// Provides names according to a set of naming rules contained in
  /// <see cref="NamingConvention"/>.
  /// </summary>
  public sealed class NameBuilder
  {
    private const string AssociationPattern = "{0}-{1}-{2}";
    private const string GeneratorPattern = "{0}-Generator";
    private const string GenericTypePattern = "{0}({1})";
    private const string ReferenceForeignKeyFormat = "FK_{0}_{1}_{2}";
    private const string HierarchyForeignKeyFormat = "FK_{0}_{1}";

    private static readonly Func<PropertyInfo, string> fieldNameCacheValueFactory =
      field => field.GetAttribute<OverrideFieldNameAttribute>()?.Name ?? field.Name;

    private readonly int maxIdentifierLength;
    private readonly NamingConvention namingConvention;
    private readonly bool isMultidatabase;
    private readonly string defaultDatabase;
    private readonly ConcurrentDictionary<PropertyInfo, string> fieldNameCache =
      new ConcurrentDictionary<PropertyInfo, string>();

    /// <summary>
    /// Gets the <see cref="Entity.TypeId"/> column name.
    /// </summary>
    public string TypeIdColumnName { get; private set; }

    /// <summary>
    /// Gets the name for <see cref="TypeDef"/> object.
    /// </summary>
    /// <param name="context">A <see cref="Domain"/> building context.</param>
    /// <param name="type">The <see cref="TypeDef"/> object.</param>
    /// <returns>Type name.</returns>
    public string BuildTypeName(BuildingContext context, TypeDef type)
    {
      ArgumentNullException.ThrowIfNull(context, "context");
      ArgumentNullException.ThrowIfNull(type, "type");

      if (type.UnderlyingType.IsGenericType)
        return ApplyNamingRules(BuildGenericTypeName(context, type.UnderlyingType, type.MappingName));

      if (!type.MappingName.IsNullOrEmpty())
        return ApplyNamingRules(type.MappingName);

      var underlyingTypeName = type.UnderlyingType.GetShortName();
      var @namespace = type.UnderlyingType.Namespace;
      var result = type.Name.IsNullOrEmpty() ? underlyingTypeName : type.Name;
      switch (namingConvention.NamespacePolicy) {
        case NamespacePolicy.Synonymize: {
          string synonym;
          if (!namingConvention.NamespaceSynonyms.TryGetValue(@namespace, out synonym))
            synonym = @namespace;
          if (!synonym.IsNullOrEmpty())
            result = $"{synonym}.{result}";
        }
          break;
        case NamespacePolicy.AsIs:
          if (!@namespace.IsNullOrEmpty())
            result = $"{@namespace}.{result}";
          break;
        case NamespacePolicy.Hash:
          var hash = GetHash(@namespace);
          if (!@hash.IsNullOrEmpty())
            result = $"{hash}.{result}";
          break;
      }
      return ApplyNamingRules(result);
    }

    private string BuildGenericTypeName(BuildingContext context, Type type, string mappingName)
    {
      if (!type.IsGenericType || type.IsGenericParameter) {
        return type.GetShortName();
      }

      string typeName;
      if (mappingName.IsNullOrEmpty()) {
        typeName = type.GetShortName();
        typeName = typeName.Substring(0, typeName.IndexOf("<"));
      }
      else {
        typeName = mappingName;
      }

      var arguments = type.GetGenericArguments();
      var names = new string[arguments.Length];
      if (type.IsGenericTypeDefinition) {
        for (int i = 0; i < arguments.Length; i++) {
          var argument = arguments[i];
          names[i] = BuildGenericTypeName(context, argument, null);
        }
      }
      else {
        for (var i = 0; i < arguments.Length; i++) {
          var argument = arguments[i];
          if (argument.IsSubclassOf(WellKnownOrmTypes.Persistent) &&
            context.BuilderConfiguration.ModelFilter.IsTypeAvailable(argument) && argument != WellKnownOrmTypes.EntitySetItemOfT1T2) {
            var argTypeDef = context.ModelDefBuilder.ProcessType(argument);
            names[i] = argTypeDef.Name;
          }
          else {
            names[i] = BuildGenericTypeName(context, argument, null);
          }
        }
      }
      return ApplyNamingRules(string.Format(GenericTypePattern, typeName, string.Join("-", names)));
    }

    /// <summary>
    /// Build table name by index.
    /// </summary>
    /// <param name="indexInfo">Index to build table name for.</param>
    /// <returns>Table name.</returns>
    public string BuildTableName(IndexInfo indexInfo)
    {
      return ApplyNamingRules(indexInfo.ReflectedType.Name);
    }

    /// <summary>
    /// Build table column name by <see cref="Upgrade.Model.StorageColumnInfo"/>.
    /// </summary>
    /// <param name="columnInfo"><see cref="Upgrade.Model.StorageColumnInfo"/> to build column table name for.</param>
    /// <returns>Column name.</returns>
    public string BuildTableColumnName(ColumnInfo columnInfo)
    {
      ArgumentNullException.ThrowIfNull(columnInfo, "columnInfo");
      return ApplyNamingRules(columnInfo.Name);
    }

    /// <summary>
    /// Builds foreign key name by association.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public string BuildReferenceForeignKeyName(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
    {
      ArgumentNullException.ThrowIfNull(ownerType, "ownerType");
      ArgumentNullException.ThrowIfNull(ownerField, "ownerField");
      ArgumentNullException.ThrowIfNull(targetType, "targetType");
      return ApplyNamingRules(string.Format(ReferenceForeignKeyFormat, ownerType.Name, ownerField.Name, targetType.Name));
    }

    /// <summary>
    /// Builds foreign key name for in-hierarchy primary key references.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public string BuildHierarchyForeignKeyName(TypeInfo baseType, TypeInfo descendantType)
    {
      ArgumentNullException.ThrowIfNull(baseType, "baseType");
      ArgumentNullException.ThrowIfNull(descendantType, "descendantType");
      return ApplyNamingRules(string.Format(HierarchyForeignKeyFormat, baseType.Name, descendantType.Name));
    }

    /// <summary>
    /// Gets the name for <see cref="FieldDef"/> object.
    /// </summary>
    /// <param name="field">The <see cref="FieldDef"/> object.</param>
    /// <returns>Field name.</returns>
    public string BuildFieldName(FieldDef field)
    {
      ArgumentNullException.ThrowIfNull(field, "field");
      string result = field.Name;
      if (field.UnderlyingProperty != null)
        return BuildFieldNameInternal(field.UnderlyingProperty);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string BuildFieldNameInternal(PropertyInfo propertyInfo)
      => fieldNameCache.GetOrAdd(propertyInfo, fieldNameCacheValueFactory);

    /// <summary>
    /// Builds the name of the field.
    /// </summary>
    /// <param name="propertyInfo">The property info.</param>
    public string BuildFieldName(PropertyInfo propertyInfo)
    {
      ArgumentNullException.ThrowIfNull(propertyInfo, "propertyInfo");
      return BuildFieldNameInternal(propertyInfo);
    }

    /// <summary>
    /// Builds the name of the explicitly implemented field.
    /// </summary>
    /// <param name="type">The type of interface explicit member implements.</param>
    /// <param name="name">The member name.</param>
    /// <returns>Explicitly implemented field name.</returns>
    public string BuildExplicitFieldName(TypeInfo type, string name)
    {
      return type.IsInterface ? type.UnderlyingType.Name + "." + name : name;
    }

    /// <summary>
    /// Builds the full name of the <paramref name="childField"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    /// <returns>Nested field name.</returns>
    public string BuildNestedFieldName(FieldInfo complexField, FieldInfo childField)
    {
      ArgumentNullException.ThrowIfNull(complexField, "complexField");
      ArgumentNullException.ThrowIfNull(childField, "childField");
      var nameSource = complexField;
      while (nameSource.Parent != null)
        nameSource = nameSource.Parent;
      return string.Concat(nameSource.Name, ".", childField.Name);
    }

    /// <summary>
    /// Builds the <see cref="MappedNode.MappingName"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    /// <returns>Field mapping name.</returns>
    public string BuildMappingName(FieldInfo complexField, FieldInfo childField)
    {
      Func<FieldInfo, string> getMappingName = f => f.MappingName ?? f.Name;
      var nameSource = complexField;
      while (nameSource.Parent != null)
        nameSource = nameSource.Parent;
      return string.Concat(getMappingName(nameSource), ".", getMappingName(childField));
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object.
    /// </summary>
    /// <param name="field">The field info.</param>
    /// <param name="baseColumn">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>Column name.</returns>
    public string BuildColumnName(FieldInfo field, ColumnInfo baseColumn)
    {
      ArgumentNullException.ThrowIfNull(field, "field");
      ArgumentNullException.ThrowIfNull(baseColumn, "baseColumn");

      var result = field.MappingName ?? field.Name;
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object concatenating
    /// <see cref="Node.Name"/> of its declaring type with the original column name.
    /// </summary>
    /// <param name="column">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>Column name.</returns>
    public string BuildColumnName(ColumnInfo column)
    {
      ArgumentNullException.ThrowIfNull(column, "column");
      if (column.Name.StartsWith(column.Field.DeclaringType.Name + ".", StringComparison.Ordinal))
        throw new InvalidOperationException();
      string result = string.Concat(column.Field.DeclaringType.Name, ".", column.Name);
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Gets the name for <see cref="IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexDef"/> object.</param>
    /// <returns>Index name.</returns>
    public string BuildIndexName(TypeDef type, IndexDef index)
    {
      ArgumentNullException.ThrowIfNull(index, "index");

      string result = string.Empty;
      if (!index.Name.IsNullOrEmpty())
        result = index.Name;
      else if (index.IsPrimary)
        result = $"PK_{type.Name}";
      else if (index.KeyFields.Count == 0)
        result = string.Empty;
      else if (!index.MappingName.IsNullOrEmpty())
        result = index.MappingName;
      else {
        if (index.KeyFields.Count == 1) {
          FieldDef field;
          if (type.Fields.TryGetValue(index.KeyFields[0].Key, out field) && field.IsEntity)
            result = $"FK_{field.Name}";
        }
        if (result.IsNullOrEmpty()) {
          result = $"IX_{string.Join("", index.KeyFields.Keys)}";
        }
      }
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Gets the name for <see cref="IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexInfo"/> object.</param>
    /// <returns>Index name.</returns>
    public string BuildIndexName(TypeInfo type, IndexInfo index)
    {
      ArgumentNullException.ThrowIfNull(index, "index");
      if (!index.Name.IsNullOrEmpty())
        return index.Name;

      string result = string.Empty;
      if (index.IsPrimary) {
        if (index.IsVirtual) {
          var originIndex = index;
          while (true) {
            var singleSourceIndex = (originIndex.Attributes & (IndexAttributes.Filtered | IndexAttributes.View | IndexAttributes.Typed)) != IndexAttributes.None;
            if (singleSourceIndex) {
              var sourceIndex = originIndex.UnderlyingIndexes[0];
              if (sourceIndex.ReflectedType != originIndex.ReflectedType) {
                originIndex = sourceIndex;
                break;
              }
              originIndex = sourceIndex;
            }
            else {
              originIndex = null;
              break;
            }
          }
          result = originIndex != null
            ? $"PK_{type}.{originIndex.ReflectedType}"
            : (type == index.DeclaringType
                ? $"PK_{type}"
                : $"PK_{type}.{index.DeclaringType}");
        }
        else
          result = index.DeclaringType != type
            ? $"PK_{type}.{index.DeclaringType}"
            : $"PK_{type}";
      }
      else {
        if (!index.MappingName.IsNullOrEmpty()) {
          result = index.DeclaringType != type
            ? $"{type}.{index.DeclaringType}.{index.MappingName}"
            : $"{type}.{index.MappingName}";
        }
        else if (index.IsVirtual && index.DeclaringIndex.Name!=null) {
          result = index.DeclaringIndex.Name;
        }
        else {
          var keyFields = new HashSet<FieldInfo>();
          foreach (var keyColumn in index.KeyColumns.Keys) {
            var field = keyColumn.Field;
            while (field.Parent != null && !field.Parent.IsStructure)
              field = field.Parent;
            keyFields.Add(field);
          }
          var indexNameSuffix = keyFields
            .Select(f => f.Name)
            .ToDelimitedString(String.Empty);
          if (keyFields.Count == 1 && keyFields.Single().IsEntity)
            result = index.DeclaringType != type
              ? $"{type}.{index.DeclaringType}.FK_{indexNameSuffix}"
              : $"{type}.FK_{indexNameSuffix}";
          else
            result = index.DeclaringType != type
              ? $"{type}.{index.DeclaringType}.IX_{indexNameSuffix}"
              : $"{type}.IX_{indexNameSuffix}";
        }
      }

      if (index.IsVirtual) {
        var indexAttributes = index.Attributes;

        var indexNameBeforeRules = indexAttributes.HasFlag(IndexAttributes.Filtered)
          ? $"{result}.FILTERED.{type.Name}"
          : indexAttributes.HasFlag(IndexAttributes.Join)
            ? $"{result}.JOIN.{type.Name}"
            : indexAttributes.HasFlag(IndexAttributes.Union)
              ? $"{result}.UNION.{type.Name}"
              : indexAttributes.HasFlag(IndexAttributes.View)
                ? $"{result}.VIEW.{type.Name}"
                : indexAttributes.HasFlag(IndexAttributes.Typed)
                  ? $"{result}.TYPED.{type.Name}"
                  : $"{result}{type.Name}";
        return ApplyNamingRules(indexNameBeforeRules);
      }
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Builds the name of the full-text index.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns>Index name.</returns>
    public string BuildFullTextIndexName(TypeInfo typeInfo)
    {
      var result = $"FT_{typeInfo.MappingName ?? typeInfo.Name}";
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Builds name for partial index.
    /// </summary>
    /// <param name="index">Index to build name for.</param>
    /// <param name="filterType">Type that defines filter for partial index.</param>
    /// <param name="filterMember">Member that defines filter for partial index.</param>
    /// <returns>Name for <paramref name="index"/>.</returns>
    public string BuildPartialIndexName(IndexDef index, Type filterType, string filterMember)
    {
      return $"IXP_{filterType.Name}.{filterMember}";
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns>Association name.</returns>
    public string BuildAssociationName(AssociationInfo target)
    {
      return ApplyNamingRules(string.Format(AssociationPattern,
        target.OwnerType.Name,
        target.OwnerField.Name,
        target.TargetType.Name));
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="ownerType">Type of the owner.</param>
    /// <param name="ownerField">The owner field.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>Association name.</returns>
    public string BuildAssociationName(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
    {
      return ApplyNamingRules(string.Format(AssociationPattern,
        ownerType.Name,
        ownerField.Name,
        targetType.Name));
    }

    /// <summary>
    /// Builds the mapping name for the auxiliary type
    /// associated with specified <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns>Auxiliary type mapping name.</returns>
    public string BuildAuxiliaryTypeMappingName(AssociationInfo target)
    {
      return ApplyNamingRules(string.Format(AssociationPattern,
        target.OwnerType.MappingName ?? target.OwnerType.Name,
        target.OwnerField.MappingName ?? target.OwnerField.Name,
        target.TargetType.MappingName ?? target.TargetType.Name));
    }

    /// <summary>
    /// Builds the key sequence name by <see cref="KeyInfo"/> instance.
    /// </summary>
    /// <param name="keyInfo">The <see cref="KeyInfo"/> instance to build sequence name for.</param>
    /// <returns>Sequence name.</returns>
    public string BuildSequenceName(KeyInfo keyInfo)
    {
      return keyInfo.GeneratorBaseName==null
        ? null
        : ApplyNamingRules(string.Format(GeneratorPattern, keyInfo.GeneratorBaseName));
    }

    /// <summary>
    /// Builds name for key generator.
    /// </summary>
    /// <param name="key">Key to build key generator name for.</param>
    /// <param name="hierarchyDef">Hierarchy definition.</param>
    /// <returns>Key generator name</returns>
    public string BuildKeyGeneratorName(KeyInfo key, HierarchyDef hierarchyDef)
    {
      var mappingDatabase = hierarchyDef.Root.MappingDatabase;
      var databaseSuffixRequired =
        key.GeneratorKind==KeyGeneratorKind.Default
        && KeyGeneratorFactory.IsSequenceBacked(key.SingleColumnType)
        && !string.IsNullOrEmpty(mappingDatabase);
      var baseName = key.GeneratorBaseName;
      return databaseSuffixRequired
        ? FormatKeyGeneratorName(mappingDatabase, baseName)
        : baseName;
    }

    /// <summary>
    /// Builds name for key generator.
    /// </summary>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Key generator name.</returns>
    public string BuildKeyGeneratorName(KeyGeneratorConfiguration configuration)
    {
      if (!isMultidatabase)
        return configuration.Name;
      var database = configuration.Database;
      if (string.IsNullOrEmpty(database))
        database = defaultDatabase;
      return FormatKeyGeneratorName(database, configuration.Name);
    }

    /// <summary>
    /// Builds base name for key generator.
    /// </summary>
    /// <param name="key">Key to build base key generator name for.</param>
    /// <param name="hierarchyDef">Hierarchy definition.</param>
    /// <returns>Base key generator name.</returns>
    public string BuildKeyGeneratorBaseName(KeyInfo key, HierarchyDef hierarchyDef)
    {
      if (key.GeneratorKind==KeyGeneratorKind.None)
        throw new ArgumentOutOfRangeException("key.GeneratorKind");

      if (!string.IsNullOrEmpty(hierarchyDef.KeyGeneratorName))
        return hierarchyDef.KeyGeneratorName;

      if (key.GeneratorKind==KeyGeneratorKind.Custom)
        throw new DomainBuilderException(string.Format(
          Strings.ExKeyGeneratorAttributeOnTypeXRequiresNameToBeSet,
          hierarchyDef.Root.UnderlyingType.GetShortName()));

      // KeyGeneratorKind.Default:

      return key.SingleColumnType.GetShortName();
    }

    /// <summary>
    /// Applies current naming convention to the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to apply the convention to.</param>
    /// <returns>Processed name satisfying naming convention.</returns>
    public string ApplyNamingRules(string name)
    {
      string result = name;

      result = result.Replace('+', '.');
      result = result.Replace('[', '(');
      result = result.Replace(']', ')');

      if (namingConvention.LetterCasePolicy==LetterCasePolicy.Uppercase)
        result = result.ToUpperInvariant();
      else if (namingConvention.LetterCasePolicy==LetterCasePolicy.Lowercase)
        result = result.ToLowerInvariant();

      if ((namingConvention.NamingRules & NamingRules.UnderscoreDots) > 0)
        result = result.Replace('.', '_');
      if ((namingConvention.NamingRules & NamingRules.UnderscoreHyphens) > 0)
        result = result.Replace('-', '_');
      if ((namingConvention.NamingRules & NamingRules.RemoveDots) > 0)
        result = result.Replace(".", string.Empty);
      if ((namingConvention.NamingRules & NamingRules.RemoveHyphens) > 0)
        result = result.Replace("-", string.Empty);

      if (result.Length <= maxIdentifierLength)
        return result;

      string hash = GetHash(result);
      return result.Substring(0, maxIdentifierLength - hash.Length) + hash;
    }

    /// <summary>
    /// Computes the hash for the specified <paramref name="name"/>.
    /// The length of the resulting hash is 8 characters.
    /// </summary>
    /// <returns>Computed hash.</returns>
    private static string GetHash(string name)
    {
#pragma warning disable SYSLIB0021 // Type or member is obsolete
      using (var hashAlgorithm = new MD5CryptoServiceProvider()) {
        var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name));
        return $"H{hash[0]:x2}{hash[1]:x2}{hash[2]:x2}{hash[3]:x2}";
      }
#pragma warning restore SYSLIB0021 // Type or member is obsolete
    }

    private static string FormatKeyGeneratorName(string database, string name)
    {
      return $"{name}@{database}";
    }


    // Constructors

    internal NameBuilder(DomainConfiguration configuration, ProviderInfo providerInfo)
    {
      ArgumentNullException.ThrowIfNull(configuration, "configuration");
      ArgumentNullException.ThrowIfNull(configuration.NamingConvention, "configuration.NamingConvention");
      ArgumentNullException.ThrowIfNull(providerInfo, "providerInfo");

      namingConvention = configuration.NamingConvention;
      isMultidatabase = configuration.IsMultidatabase;
      defaultDatabase = configuration.DefaultDatabase;
      maxIdentifierLength = providerInfo.MaxIdentifierLength;

      TypeIdColumnName = ApplyNamingRules(WellKnown.TypeIdFieldName);
    }
  }
}
