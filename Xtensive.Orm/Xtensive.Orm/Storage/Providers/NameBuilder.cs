// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Aspects;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using System.Linq;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Name builder for <see cref="Orm.Model.DomainModel"/> nodes 
  /// Provides names according to a set of naming rules contained in
  /// <see cref="NamingConvention"/>.
  /// </summary>
  public class NameBuilder : HandlerBase
  {
    private Dictionary<Pair<Type, string>, string> fieldNameCache = new Dictionary<Pair<Type, string>, string>();
    private readonly object _lock = new object();
    private HashAlgorithm hashAlgorithm;
    private const string AssociationPattern = "{0}-{1}-{2}";
    private const string GeneratorPattern = "{0}-Generator";
    private const string GenericTypePattern = "{0}({1})";
    private const string ReferenceForeignKeyFormat = "FK_{0}_{1}_{2}";
    private const string HierarchyForeignKeyFormat = "FK_{0}_{1}";
    private int maxIdentifierLength;

    /// <summary>
    /// Gets the maximum length of storage entity identifier.
    /// </summary>
    protected virtual int MaxIdentifierLength
    {
      get { return maxIdentifierLength; }
    }

    /// <summary>
    /// Gets the <see cref="Entity.TypeId"/> column name.
    /// </summary>
    public string TypeIdColumnName { get; private set; }

    /// <summary>
    /// Gets the naming convention object.
    /// </summary>
    public NamingConvention NamingConvention { get; private set; }

    /// <summary>
    /// Gets the name for <see cref="TypeDef"/> object.
    /// </summary>
    /// <param name="type">The <see cref="TypeDef"/> object.</param>
    /// <returns>Type name.</returns>
    public virtual string BuildTypeName(TypeDef type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      if (type.UnderlyingType.IsGenericType)
        return ApplyNamingRules(BuildGenericTypeName(type.UnderlyingType, type.MappingName));

      if (!type.MappingName.IsNullOrEmpty())
        return ApplyNamingRules(type.MappingName);

      var underlyingTypeName = type.UnderlyingType.GetShortName();
      var @namespace = type.UnderlyingType.Namespace;
      var result = type.Name.IsNullOrEmpty() ? underlyingTypeName : type.Name;
      switch (NamingConvention.NamespacePolicy) {
        case NamespacePolicy.Synonymize: {
          string synonym;
          if (!NamingConvention.NamespaceSynonyms.TryGetValue(@namespace, out synonym))
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

    private string BuildGenericTypeName(Type type, string mappingName)
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
          names[i] = BuildGenericTypeName(argument, null);
        }
      else {
        var context = BuildingContext.Demand();
        for (int i = 0; i < arguments.Length; i++) {
          var argument = arguments[i];
          if (argument.IsSubclassOf(typeof (Persistent))) {
            var argTypeDef = ModelDefBuilder.ProcessType(argument);
            names[i] = argTypeDef.Name;
          }
          else
            names[i] = BuildGenericTypeName(argument, null);
        }
      }
      return string.Format(GenericTypePattern, typeName, string.Join("-", names));
    }

    /// <summary>
    /// Build table name by index.
    /// </summary>
    /// <param name="indexInfo">Index to build table name for.</param>
    /// <returns>Table name.</returns>
    public virtual string BuildTableName(IndexInfo indexInfo)
    {
      return ApplyNamingRules(indexInfo.ReflectedType.Name);
    }

    /// <summary>
    /// Build table column name by <see cref="Model.ColumnInfo"/>.
    /// </summary>
    /// <param name="columnInfo"><see cref="Model.ColumnInfo"/> to build column table name for.</param>
    /// <returns>Column name.</returns>
    public virtual string BuildTableColumnName(ColumnInfo columnInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnInfo, "columnInfo");
      return ApplyNamingRules(columnInfo.Name);
    }

    /// <summary>
    /// Builds foreign key name by association.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public virtual string BuildReferenceForeignKeyName(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
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
    public virtual string BuildHierarchyForeignKeyName(TypeInfo baseType, TypeInfo descendantType)
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
    public virtual string BuildFieldName(FieldDef field)
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
      string result;
      if (fieldNameCache.TryGetValue(key, out result))
        return result;
      var attribute = propertyInfo.GetAttribute<OverrideFieldNameAttribute>();
      if (attribute!=null) {
        result = attribute.Name;
        fieldNameCache.Add(key, result);
        return result;
      }
      return propertyInfo.Name;
    }

    /// <summary>
    /// Builds the name of the field.
    /// </summary>
    /// <param name="propertyInfo">The property info.</param>
    public string BuildFieldName(PropertyInfo propertyInfo)
    {
      var key = new Pair<Type, string>(propertyInfo.ReflectedType, propertyInfo.Name);
      string result;
      return fieldNameCache.TryGetValue(key, out result) 
        ? result 
        : propertyInfo.Name;
    }

    /// <summary>
    /// Builds the name of the explicitly implemented field.
    /// </summary>
    /// <param name="type">The type of interface explicit member implements.</param>
    /// <param name="name">The member name.</param>
    /// <returns>Explicitly implemented field name.</returns>
    public virtual string BuildExplicitFieldName(TypeInfo type, string name)
    {
      return type.IsInterface ? type.UnderlyingType.Name + "." + name : name;
    }

    /// <summary>
    /// Builds the full name of the <paramref name="childField"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    /// <returns>Nested field name.</returns>
    public virtual string BuildNestedFieldName(FieldInfo complexField, FieldInfo childField)
    {
      ArgumentValidator.EnsureArgumentNotNull(complexField, "complexField");
      ArgumentValidator.EnsureArgumentNotNull(childField, "childField");
      var nameSource = complexField;
      while (nameSource.Parent != null)
        nameSource = nameSource.Parent;
      return string.Concat(nameSource.Name, ".", childField.Name);
    }


    /// <summary>
    /// Builds the <see cref="MappingNode.MappingName"/>.
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
    public virtual string BuildColumnName(FieldInfo field, ColumnInfo baseColumn)
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
    public virtual string BuildColumnName(ColumnInfo column)
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
    public virtual string BuildIndexName(TypeDef type, IndexDef index)
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
    public virtual string BuildIndexName(TypeInfo type, IndexInfo index)
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
          suffix = ".FILTERED";
        else if ((index.Attributes & IndexAttributes.Join)!=IndexAttributes.None)
          suffix = ".JOIN";
        else if ((index.Attributes & IndexAttributes.Union)!=IndexAttributes.None)
          suffix = ".UNION";
        else if ((index.Attributes & IndexAttributes.View)!=IndexAttributes.None)
          suffix = ".VIEW";
        else if ((index.Attributes & IndexAttributes.Typed) != IndexAttributes.None)
          suffix = ".TYPED";
      }
      return ApplyNamingRules(string.Concat(result, suffix));
    }

    /// <summary>
    /// Builds the name of the full-text index.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns>Index name.</returns>
    public virtual string BuildFullTextIndexName(TypeInfo typeInfo)
    {
      var result = string.Format("FT_{0}", typeInfo.MappingName ?? typeInfo.Name);
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns>Association name.</returns>
    public virtual string BuildAssociationName(AssociationInfo target)
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
    public virtual string BuildAssociationName(TypeInfo ownerType, FieldInfo ownerField, TypeInfo targetType)
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
    public virtual string BuildAuxiliaryTypeMappingName(AssociationInfo target)
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
      return keyInfo.GeneratorName == null
        ? null
        : ApplyNamingRules(string.Format(GeneratorPattern, keyInfo.GeneratorName));
    }

    /// <summary>
    /// Applies current naming convention to the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to apply the convention to.</param>
    /// <returns>Processed name satisfying naming convention.</returns>
    public virtual string ApplyNamingRules(string name)
    {
      string result = name;
      result = result.Replace('+', '.');

      if (NamingConvention.LetterCasePolicy==LetterCasePolicy.Uppercase)
        result = result.ToUpperInvariant();
      else if (NamingConvention.LetterCasePolicy==LetterCasePolicy.Lowercase)
        result = result.ToLowerInvariant();

      if ((NamingConvention.NamingRules & NamingRules.UnderscoreDots) > 0)
        result = result.Replace('.', '_');
      if ((NamingConvention.NamingRules & NamingRules.UnderscoreHyphens) > 0)
        result = result.Replace('-', '_');

      if (result.Length <= MaxIdentifierLength)
        return result;
      string hash = GetHash(result);
      return result.Substring(0, MaxIdentifierLength - hash.Length) + hash;
    }

    /// <summary>
    /// Computes the hash for the specified <paramref name="name"/>.
    /// The length of the resulting hash is 8 characters.
    /// </summary>
    /// <returns>Computed hash.</returns>
    protected string GetHash(string name)
    {
      byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name)); 
      return string.Format("H{0:x2}{1:x2}{2:x2}{3:x2}", hash[0], hash[1], hash[2], hash[3]);
    }


    // Initializers

    /// <summary>
    /// <see cref="ClassDocTemplate.Initialize" copy="true"/>
    /// </summary>
    /// <param name="namingConvention">The naming convention.</param>
    protected internal virtual void Initialize(NamingConvention namingConvention)
    {
      ArgumentValidator.EnsureArgumentNotNull(namingConvention, "namingConvention");
      NamingConvention = namingConvention;
      hashAlgorithm = new MD5CryptoServiceProvider();
      maxIdentifierLength = Handlers.DomainHandler.ProviderInfo.MaxIdentifierLength;
      TypeIdColumnName = ApplyNamingRules(Orm.WellKnown.TypeIdFieldName);
    }
  }
}