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
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Name provider for <see cref="DomainModelDef"/>. 
  /// Provides names according to a set of naming rules contained in
  /// <see cref="NamingConvention"/> object.
  /// </summary>
  public class NameBuilder : HandlerBase
  {
    private static readonly Regex explicitFieldNameRegex = new Regex(@"(?<name>\w+\.\w+)$", 
      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
    private static readonly string typeIdFieldName = "TypeId";
    private NamingConvention namingConvention;
    private HashAlgorithm hashAlgorithm;

    /// <summary>
    /// Gets the naming convention object.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get { return namingConvention; }
    }

    /// <summary>
    /// Gets the <see cref="Entity.TypeId"/> field name.
    /// </summary>
    public string TypeIdFieldName
    {
      get { return typeIdFieldName; }
    }

    /// <summary>
    /// Gets the name for <see cref="TypeDef"/> object.
    /// </summary>
    /// <param name="type">The <see cref="TypeDef"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(TypeDef type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      string result;
      if (!type.MappingName.IsNullOrEmpty())
        result = type.MappingName;
      else {
        string underlyingName = type.UnderlyingType.FullName.Substring(type.UnderlyingType.Namespace.Length + 1, type.UnderlyingType.FullName.Length - type.UnderlyingType.Namespace.Length - 1);
        result = type.Name.IsNullOrEmpty() ? underlyingName : type.Name;
        if (NamingConvention.NamespacePolicy == NamespacePolicy.UseNamespaceSynonym) {
          string namespacePrefix;
          try {
            namespacePrefix = NamingConvention.NamespaceSynonyms[type.UnderlyingType.Namespace];
            if (namespacePrefix.IsNullOrEmpty())
              throw new ApplicationException("Incorrect namespace synonyms.");
          }
          catch (KeyNotFoundException) {
            namespacePrefix = type.UnderlyingType.Namespace;
          }
          result = string.Format("{0}.{1}", namespacePrefix, result);
        }
        else if (NamingConvention.NamespacePolicy == NamespacePolicy.UseNamespace) {
          result = string.Format("{0}.{1}", type.UnderlyingType.Namespace, result);
        }
        else if (NamingConvention.NamespacePolicy == NamespacePolicy.UseHash) {
          result = string.Format("{0}.{1}", BuildHash(type.UnderlyingType.Namespace), result);
        }
      }
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="FieldDef"/> object.
    /// </summary>
    /// <param name="field">The <see cref="FieldDef"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(FieldDef field)
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
    public virtual string BuildExplicit(TypeInfo type, string name)
    {
      return type.IsInterface ? type.UnderlyingType.Name + "_" + name : name;
    }

    /// <summary>
    /// Builds the name of the explicitly implemented field.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>The built name.</returns>
    protected virtual string BuildExplicit(FieldInfo field)
    {
      if (field.UnderlyingProperty == null || !field.UnderlyingProperty.Name.Contains("."))
        return field.Name;
      return field.UnderlyingProperty.DeclaringType.Name + "_" + field.Name;
    }

    /// <summary>
    /// Builds the full name of the <paramref name="childField"/>.
    /// </summary>
    /// <param name="complexField">The complex field.</param>
    /// <param name="childField">The child field.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(FieldInfo complexField, FieldInfo childField)
    {
      ArgumentValidator.EnsureArgumentNotNull(complexField, "complexField");
      ArgumentValidator.EnsureArgumentNotNull(childField, "childField");
      return string.Concat(complexField.Name, ".", childField.Name);
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object.
    /// </summary>
    /// <param name="field">The field info.</param>
    /// <param name="baseColumn">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(FieldInfo field, ColumnInfo baseColumn)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      ArgumentValidator.EnsureArgumentNotNull(baseColumn, "baseColumn");

      string prefix = field.MappingName ?? field.Name;
      string result = field.IsStructure ? prefix + "." + baseColumn.Name : prefix;
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="ColumnInfo"/> object concatenating <see cref="TypeInfo.Name"/> with original column name.
    /// </summary>
    /// <param name="column">The <see cref="ColumnInfo"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(ColumnInfo column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "baseColumn");
      if (column.Name.StartsWith(column.Field.DeclaringType.Name))
        throw new InvalidOperationException();
      string result = string.Concat(column.Field.DeclaringType.Name, ".", column.Name);
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexDef"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(TypeDef type, IndexDef index)
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
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexInfo"/> object.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(TypeInfo type, IndexInfo index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      string result = string.Empty;
      if (!index.Name.IsNullOrEmpty())
        result = index.Name;
      else if (index.IsPrimary)
        result = string.Concat("PK_", type.Name);
      else if (!index.MappingName.IsNullOrEmpty())
        result = string.Format("{0}.{1}", type.Name,index.MappingName);
      else if (!index.ReflectedType.IsInterface && index.KeyColumns.Count == 0)
        return string.Empty;
      else {
        if (index.IsSecondary)
          result = string.Format("{0}.{1}", type.Name, index.ShortName);
        if (result.IsNullOrEmpty()) {
          Func<FieldInfo, FieldInfo> fieldSeeker = null;
          fieldSeeker = (field => field.Parent==null ? field : fieldSeeker(field.Parent));
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
      }
      return NamingConvention.Apply(string.Concat(result, suffix));
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns>The built name.</returns>
    public virtual string Build(AssociationInfo target)
    {
      return target.ReferencingType.Name + "." + target.ReferencingField.Name + "_" + target.ReferencedType.Name;
    }

    /// <summary>
    /// Builds the name for the <see cref="HierarchyInfo"/>.
    /// </summary>
    /// <param name="hierarchy">The <see cref="HierarchyInfo"/> instance to build name for.</param>
    public string Build(HierarchyInfo hierarchy)
    {
      return NamingConvention.Apply("Gen_" + hierarchy.Name);
    }

    #region Protected methods

    /// <summary>
    /// Computes the hash for the specified <paramref name="name"/>.
    /// The length of the resulting hash is 8 characters.
    /// </summary>
    /// <returns>The hash.</returns>
    protected virtual string BuildHash(string name)
    {
      byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name)); 
      return String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", hash[0], hash[1], hash[2], hash[3]);
    }

    #endregion


    // Initializers

    /// <summary>
    /// <see cref="ClassDocTemplate.Initialize" copy="true" />
    /// </summary>
    /// <param name="namingConvention">The naming convention.</param>
    protected internal virtual void Initialize(NamingConvention namingConvention)
    {
      ArgumentValidator.EnsureArgumentNotNull(namingConvention, "namingConvention");
      this.namingConvention = namingConvention;
      hashAlgorithm = new MD5CryptoServiceProvider();
    }
  }
}