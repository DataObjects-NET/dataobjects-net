// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Name builder for <see cref="DomainModel"/> nodes 
  /// Provides names according to a set of naming rules contained in
  /// <see cref="NamingConvention"/>.
  /// </summary>
  public class NameBuilder : HandlerBase
  {
    private static readonly Regex explicitFieldNameRegex = new Regex(@"(?<name>\w+\.\w+)$", 
      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);

    private HashAlgorithm hashAlgorithm;
    private const string AssociationPattern = "{0}-{1}-{2}";
    private const string GeneratorPattern = "{0}-Generator";
    private const string GenericTypePattern = "{0}({1})";
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
    /// <returns>The built name.</returns>
    public virtual string BuildTypeName(TypeDef type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      string result;

      if (type.UnderlyingType.IsGenericType) {
        Type[] arguments = type.UnderlyingType.GetGenericArguments();
        var names = new string[arguments.Length];
        if (!type.UnderlyingType.IsGenericTypeDefinition) {
          var context = BuildingContext.Current;
          for (int i = 0; i < arguments.Length; i++) {
            var argument = arguments[i];
            if (argument.IsSubclassOf(typeof (Persistent))) {
              TypeDef argTypeDef = context.ModelDef.Types[argument];
              names[i] = argTypeDef.Name;
            }
            else
              names[i] = argument.GetShortName();
          }
        }
        else
          for (int i = 0; i < arguments.Length; i++) {
            var argument = arguments[i];
            names[i] = argument.GetShortName();
          }
        if (type.MappingName.IsNullOrEmpty()) {
          result = type.UnderlyingType.GetShortName();
          result = result.Substring(0, result.IndexOf("<"));
        }
        else
          result = type.MappingName;
        return ApplyNamingRules(string.Format(GenericTypePattern, result, string.Join(",", names)));
      }

      if (!type.MappingName.IsNullOrEmpty())
        return ApplyNamingRules(type.MappingName);

      string underlyingTypeName = type.UnderlyingType.GetShortName();
      string @namespace = type.UnderlyingType.Namespace;
      result = type.Name.IsNullOrEmpty() ? underlyingTypeName : type.Name;
      switch (NamingConvention.NamespacePolicy) {
        case NamespacePolicy.Synonymize: {
          string namespacePrefix;
          try {
            namespacePrefix = NamingConvention.NamespaceSynonyms[@namespace];
            if (namespacePrefix.IsNullOrEmpty())
              throw new InvalidOperationException(Resources.Strings.ExIncorrectNamespaceSynonyms);
          }
          catch (KeyNotFoundException) {
            namespacePrefix = @namespace;
          }
          result = string.Format("{0}.{1}", namespacePrefix, result);
        }
          break;
        case NamespacePolicy.AsIs:
          result = string.Format("{0}.{1}", @namespace, result);
          break;
        case NamespacePolicy.Hash:
          result = string.Format("{0}.{1}", GetHash(@namespace), result);
          break;
      }
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Build table name by index.
    /// </summary>
    /// <param name="indexInfo">Index to build table name for.</param>
    /// <returns>Table name</returns>
    public virtual string BuildTableName(IndexInfo indexInfo)
    {
      return ApplyNamingRules(indexInfo.ReflectedType.Name);
    }

    /// <summary>
    /// Build table column name by <see cref="ColumnInfo"/>.
    /// </summary>
    /// <param name="columnInfo"><see cref="ColumnInfo"/> to build column table name for.</param>
    /// <returns>Column name</returns>
    public virtual string BuildTableColumnName(ColumnInfo columnInfo)
    {
      return ApplyNamingRules(columnInfo.Name);
    }

    /// <summary>
    /// Builds foreign key name by association.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public virtual string BuildForeignKeyName(AssociationInfo association, FieldInfo referencingField)
    {
      return ApplyNamingRules(string.Format("FK_{0}_{1}", association.Name, referencingField.Name));
    }

    /// <summary>
    /// Builds foreign key name for in-hierarchy primary key references.
    /// </summary>
    /// <returns>Foreign key name.</returns>
    public virtual string BuildForeignKeyName(TypeInfo baseType, TypeInfo descendantType)
    {
      return ApplyNamingRules(string.Format("FK_{0}_{1}", baseType.Name, descendantType.Name));
    }

    /// <summary>
    /// Gets the name for <see cref="FieldDef"/> object.
    /// </summary>
    /// <param name="field">The <see cref="FieldDef"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string BuildFieldName(FieldDef field)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      string result = field.Name;
      if (field.UnderlyingProperty != null)
        result = field.UnderlyingProperty.Name;
      if (explicitFieldNameRegex.IsMatch(result))
        result = explicitFieldNameRegex.Match(result).Groups["name"].Value.Replace('.','_');
      return result;
    }

    /// <summary>
    /// Builds the name of the explicitly implemented member.
    /// </summary>
    /// <param name="type">The type of interface explicit member implements.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The built name.</returns>
    public virtual string BuildExplicitFieldName(TypeInfo type, string name)
    {
      return type.IsInterface ? type.UnderlyingType.Name + "_" + name : name;
    }

    /// <summary>
    /// Builds the full name of the <paramref name="childField"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    public virtual string BuildNestedFieldName(FieldInfo complexField, FieldInfo childField)
    {
      ArgumentValidator.EnsureArgumentNotNull(complexField, "complexField");
      ArgumentValidator.EnsureArgumentNotNull(childField, "childField");
      var nameSource = complexField;
      while (nameSource.Parent != null)
        nameSource = nameSource.Parent;
//      if (complexField.Parent!=null)
//        return string.Concat(complexField.Parent.Name, ".", childField.Name);
      return string.Concat(nameSource.Name, ".", childField.Name);
    }


    /// <summary>
    /// Builds the <see cref="MappingNode.MappingName"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    public string BuildMappingName(FieldInfo complexField, FieldInfo childField)
    {
      Func<FieldInfo, string> getMappingName = f => f.MappingName ?? f.Name;
      var nameSource = complexField;
      while (nameSource.Parent != null)
        nameSource = nameSource.Parent;
//      if (complexField.Parent != null)
//        return string.Concat(getMappingName(complexField.Parent), ".", getMappingName(childField));
      return string.Concat(getMappingName(nameSource), ".", getMappingName(childField));
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object.
    /// </summary>
    /// <param name="field">The field info.</param>
    /// <param name="baseColumn">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string BuildColumnName(FieldInfo field, ColumnInfo baseColumn)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      ArgumentValidator.EnsureArgumentNotNull(baseColumn, "baseColumn");

      var result = field.MappingName ?? field.Name;

//      string name = field.MappingName ?? field.Name;
//      var currentField = field.Parent;
//      while (currentField!=null) {
//        name = currentField.MappingName ?? currentField.Name + "." + name;
//        currentField = currentField.Parent;
//      }
//
//      string result = field.IsStructure ? name + "." + baseColumn.Name : name;
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object concatenating <see cref="TypeInfo.Name"/> with original column name.
    /// </summary>
    /// <param name="column">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string BuildColumnName(ColumnInfo column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      if (column.Name.StartsWith(column.Field.DeclaringType.Name))
        throw new InvalidOperationException();
      string result = string.Concat(column.Field.DeclaringType.Name, ".", column.Name);
      return ApplyNamingRules(result);
    }

    /// <summary>
    /// Gets the name for <see cref="IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexDef"/> object.</param>
    /// <returns>The built name.</returns>
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
          FieldDef field = type.Fields[index.KeyFields[0].Key];
          if (field.IsEntity)
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
    /// <returns>The built name.</returns>
    public virtual string BuildIndexName(TypeInfo type, IndexInfo index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      string result = string.Empty;
      if (!index.Name.IsNullOrEmpty())
        result = index.Name;
      else if (index.IsPrimary)
        result = index.DeclaringType != type
          ? string.Format("PK_{0}.{1}", type.Name, index.DeclaringType)
          : string.Format("PK_{0}", type.Name);
      else if (!index.MappingName.IsNullOrEmpty())
        result = index.DeclaringType != type
          ? string.Format("{0}.{1}.{2}", type.Name, index.DeclaringType, index.MappingName)
          : string.Format("{0}.{1}", type.Name, index.MappingName);
      else if (!index.ReflectedType.IsInterface && index.KeyColumns.Count == 0)
        return string.Empty;
      else {
        if (index.IsSecondary)
          result = index.DeclaringType != type
            ? string.Format("{0}.{1}.{2}", type.Name, index.DeclaringType, index.ShortName)
            : string.Format("{0}.{1}", type.Name, index.ShortName);
        if (result.IsNullOrEmpty()) {
          Func<FieldInfo, FieldInfo> fieldSeeker = null;
          fieldSeeker = (field => field.Parent == null ? field : fieldSeeker(field.Parent));
          var fieldList = new List<FieldInfo>();
          foreach (ColumnInfo keyColumn in index.KeyColumns.Keys) {
            FieldInfo foundField = fieldSeeker(keyColumn.Field);
            fieldList.Add(foundField.IsEntity ? foundField : keyColumn.Field);
          }
          result = string.Format("{1}.IX_{0}", string.Join("", fieldList.ConvertAll(f => f.Name).ToArray()), type.Name);
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
      }
      return ApplyNamingRules(string.Concat(result, suffix));
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns>The built name.</returns>
    public virtual string BuildAssociationName(AssociationInfo target)
    {
      return ApplyNamingRules(string.Format(AssociationPattern, target.OwnerType.Name, target.OwnerField.Name, target.TargetType.Name));
    }

    /// <summary>
    /// Builds the name for the <see cref="generatorInfo"/> instance.
    /// </summary>
    /// <param name="generatorInfo">The <see cref="generatorInfo"/> instance to build name for.</param>
    public string BuildGeneratorName(GeneratorInfo generatorInfo)
    {
      return ApplyNamingRules(string.Format(GeneratorPattern, 
        generatorInfo.KeyInfo.Fields.Select(f => f.Key.ValueType.GetShortName()).ToDelimitedString("-")));
    }

    /// <summary>
    /// Applies current naming convention to the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name to apply the convention to.</param>
    /// <returns>Processed name satisfying naming convention.</returns>
    public virtual string ApplyNamingRules(string name)
    {
      string result = name;
      result = result.Replace('+', '_');

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
    /// <returns>The hash.</returns>
    protected string GetHash(string name)
    {
      byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name)); 
      return string.Format("H{0:x2}{1:x2}{2:x2}{3:x2}", hash[0], hash[1], hash[2], hash[3]);
    }

    // Initializers

    /// <summary>
    /// <see cref="ClassDocTemplate.Initialize" copy="true" />
    /// </summary>
    /// <param name="namingConvention">The naming convention.</param>
    protected internal virtual void Initialize(NamingConvention namingConvention)
    {
      ArgumentValidator.EnsureArgumentNotNull(namingConvention, "namingConvention");
      NamingConvention = namingConvention;
      hashAlgorithm = new MD5CryptoServiceProvider();
      maxIdentifierLength = Handlers.DomainHandler.ProviderInfo.MaxIdentifierLength;
      TypeIdColumnName = ApplyNamingRules(WellKnown.TypeIdFieldName);
    }
  }
}