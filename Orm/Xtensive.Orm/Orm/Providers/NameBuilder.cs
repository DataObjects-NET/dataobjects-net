// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Weaving;
using Xtensive.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

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

    private readonly Dictionary<Pair<Type, string>, string> fieldNameCache = new Dictionary<Pair<Type, string>, string>();
    private readonly object _lock = new object();
    private readonly int maxIdentifierLength;
    private readonly NamingConvention namingConvention;
    private readonly bool isMultidatabase;
    private readonly string defaultDatabase;
    private readonly ProviderInfo providerInfo;

    /// <summary>
    /// Gets the <see cref="Entity.TypeId"/> column name.
    /// </summary>
    public string TypeIdColumnName { get; private set; }

    /// <summary>
    /// Gets the name for <see cref="TypeDef"/> object.
    /// </summary>
    /// <param name="type">The <see cref="TypeDef"/> object.</param>
    /// <returns>Type name.</returns>
    public string BuildTypeName(BuildingContext context, TypeDef type)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

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
            result = string.Format("{0}.{1}", synonym, result);
        }
          break;
        case NamespacePolicy.AsIs:
          if (!@namespace.IsNullOrEmpty())
            result = string.Format("{0}.{1}", @namespace, result);
          break;
        case NamespacePolicy.Hash:
          var hash = GetHash(@namespace);
          if (!@hash.IsNullOrEmpty())
            result = string.Format("{0}.{1}", hash, result);
          break;
      }
      return ApplyNamingRules(result);
    }

    private string BuildGenericTypeName(BuildingContext context, Type type, string mappingName)
    {
      if (!type.IsGenericType || type.IsGenericParameter)
        return type.GetShortName();

      string typeName;
      if (mappingName.IsNullOrEmpty()) {
        typeName = type.GetShortName();
        typeName = typeName.Substring(0, typeName.IndexOf("<"));
      }
      else
        typeName = mappingName;

      var arguments = type.GetGenericArguments();
      var names = new string[arguments.Length];
      if (type.IsGenericTypeDefinition)
        for (int i = 0; i < arguments.Length; i++) {
          var argument = arguments[i];
          names[i] = BuildGenericTypeName(context, argument, null);
        }
      else {
        for (int i = 0; i < arguments.Length; i++) {
          var argument = arguments[i];
          if (argument.IsSubclassOf(typeof (Persistent))) {
            var argTypeDef = context.ModelDefBuilder.ProcessType(argument);
            names[i] = argTypeDef.Name;
          }
          else
            names[i] = BuildGenericTypeName(context, argument, null);
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
      ArgumentValidator.EnsureArgumentNotNull(columnInfo, "columnInfo");
      return ApplyNamingRules(columnInfo.Name);
    }

    /// <summary>
    /// Builds foreign key name by association.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public string BuildReferenceForeignKeyName(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
    {
      ArgumentValidator.EnsureArgumentNotNull(ownerType, "ownerType");
      ArgumentValidator.EnsureArgumentNotNull(ownerField, "ownerField");
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      return ApplyNamingRules(string.Format(ReferenceForeignKeyFormat, ownerType.Name, ownerField.Name, targetType.Name));
    }

    /// <summary>
    /// Builds foreign key name for in-hierarchy primary key references.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public string BuildHierarchyForeignKeyName(TypeInfo baseType, TypeInfo descendantType)
    {
      ArgumentValidator.EnsureArgumentNotNull(baseType, "baseType");
      ArgumentValidator.EnsureArgumentNotNull(descendantType, "descendantType");
      return ApplyNamingRules(string.Format(HierarchyForeignKeyFormat, baseType.Name, descendantType.Name));
    }

    /// <summary>
    /// Gets the name for <see cref="FieldDef"/> object.
    /// </summary>
    /// <param name="field">The <see cref="FieldDef"/> object.</param>
    /// <returns>Field name.</returns>
    public string BuildFieldName(FieldDef field)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      string result = field.Name;
      if (field.UnderlyingProperty != null)
        return BuildFieldNameInternal(field.UnderlyingProperty);
      return result;
    }

    private string BuildFieldNameInternal(PropertyInfo propertyInfo)
    {
      var key = new Pair<Type, string>(propertyInfo.ReflectedType, propertyInfo.Name);

      lock (fieldNameCache) {
        string result;
        if (fieldNameCache.TryGetValue(key, out result))
          return result;
        var attribute = propertyInfo.GetAttribute<OverrideFieldNameAttribute>();
        if (attribute!=null) {
          result = attribute.Name;
          fieldNameCache.Add(key, result);
          return result;
        }
      }

      return propertyInfo.Name;
    }

    /// <summary>
    /// Builds the name of the field.
    /// </summary>
    /// <param name="propertyInfo">The property info.</param>
    public string BuildFieldName(PropertyInfo propertyInfo)
    {
      lock (fieldNameCache) {
        var key = new Pair<Type, string>(propertyInfo.ReflectedType, propertyInfo.Name);
        string result;
        return fieldNameCache.TryGetValue(key, out result)
          ? result
          : propertyInfo.Name;
      }
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
      ArgumentValidator.EnsureArgumentNotNull(complexField, "complexField");
      ArgumentValidator.EnsureArgumentNotNull(childField, "childField");
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
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      ArgumentValidator.EnsureArgumentNotNull(baseColumn, "baseColumn");

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
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      if (column.Name.StartsWith(column.Field.DeclaringType.Name + "."))
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
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      string result = string.Empty;
      if (!index.Name.IsNullOrEmpty())
        result = index.Name;
      else if (index.IsPrimary)
        result = string.Format("PK_{0}", type.Name);
      else if (index.KeyFields.Count == 0)
        result = string.Empty;
      else if (!index.MappingName.IsNullOrEmpty())
        result = index.MappingName;
      else {
        if (index.KeyFields.Count == 1) {
          FieldDef field;
          if (type.Fields.TryGetValue(index.KeyFields[0].Key, out field) && field.IsEntity)
            result = string.Format("FK_{0}", field.Name);
        }
        if (result.IsNullOrEmpty()) {
          string[] names = new string[index.KeyFields.Keys.Count];
          index.KeyFields.Keys.CopyTo(names, 0);
          result = string.Format("IX_{0}", string.Join("", names));
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
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
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
            ? string.Format("PK_{0}.{1}", type, originIndex.ReflectedType)
            : (type == index.DeclaringType
                ? string.Format("PK_{0}", type)
                : string.Format("PK_{0}.{1}", type, index.DeclaringType));
        }
        else
          result = index.DeclaringType != type
            ? string.Format("PK_{0}.{1}", type, index.DeclaringType)
            : string.Format("PK_{0}", type);
      }
      else {
        if (!index.MappingName.IsNullOrEmpty()) {
          result = index.DeclaringType != type
            ? string.Format("{0}.{1}.{2}", type, index.DeclaringType, index.MappingName)
            : string.Format("{0}.{1}", type, index.MappingName);
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
              ? string.Format("{0}.{1}.FK_{2}", type, index.DeclaringType, indexNameSuffix)
              : string.Format("{0}.FK_{1}", type, indexNameSuffix);
          else
            result = index.DeclaringType != type
              ? string.Format("{0}.{1}.IX_{2}", type, index.DeclaringType, indexNameSuffix)
              : string.Format("{0}.IX_{1}", type, indexNameSuffix);
        }
      }

      string suffix = string.Empty;
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Filtered)!=IndexAttributes.None)
          suffix = ".FILTERED.";
        else if ((index.Attributes & IndexAttributes.Join)!=IndexAttributes.None)
          suffix = ".JOIN.";
        else if ((index.Attributes & IndexAttributes.Union)!=IndexAttributes.None)
          suffix = ".UNION.";
        else if ((index.Attributes & IndexAttributes.View)!=IndexAttributes.None)
          suffix = ".VIEW.";
        else if ((index.Attributes & IndexAttributes.Typed) != IndexAttributes.None)
          suffix = ".TYPED.";
        suffix += type.Name;
      }
      return ApplyNamingRules(string.Concat(result, suffix));
    }

    /// <summary>
    /// Builds the name of the full-text index.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns>Index name.</returns>
    public string BuildFullTextIndexName(TypeInfo typeInfo)
    {
      var result = string.Format("FT_{0}", typeInfo.MappingName ?? typeInfo.Name);
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
      return string.Format("IXP_{0}.{1}", filterType.Name, filterMember);
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
        && KeyGeneratorFactory.IsSequenceBacked(key.SingleColumnType, providerInfo.CollectionsSupportedTypes.SupportedNumericTypes)
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
    protected string GetHash(string name)
    {
      using (var hashAlgorithm = new MD5CryptoServiceProvider()) {
        byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name));
        return string.Format("H{0:x2}{1:x2}{2:x2}{3:x2}", hash[0], hash[1], hash[2], hash[3]);
      }
    }

    private static string FormatKeyGeneratorName(string database, string name)
    {
      return string.Format("{0}@{1}", name, database);
    }


    // Constructors

    internal NameBuilder(DomainConfiguration configuration, ProviderInfo providerInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(configuration.NamingConvention, "configuration.NamingConvention");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");

      namingConvention = configuration.NamingConvention;
      isMultidatabase = configuration.IsMultidatabase;
      defaultDatabase = configuration.DefaultDatabase;
      maxIdentifierLength = providerInfo.MaxIdentifierLength;

      TypeIdColumnName = ApplyNamingRules(WellKnown.TypeIdFieldName);

      this.providerInfo = providerInfo;
    }
  }
}