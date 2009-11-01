// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Name provider for <see cref="DomainDef"/>. Provides names according to a set of naming rules contained in
  /// <see cref="NamingConvention"/> object.
  /// </summary>
  public class NameProvider
  {
    private static readonly Regex fieldNameRe = new Regex(@"(?<name>\w+\.\w+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
    private readonly NamingConvention namingConvention;
    private HashAlgorithm hashAlgorithm;
    private const string typeId = "TypeId";

    /// <summary>
    /// Gets the naming convention object.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get { return namingConvention; }
    }

    /// <summary>
    /// Gets the <c>TypeId</c> field name.
    /// </summary>
    public string TypeId
    {
      get { return typeId; }
    }

    /// <summary>
    /// Gets the name for <see cref="Definitions.TypeDef"/> object.
    /// </summary>
    /// <param name="type">The <see cref="Definitions.TypeDef"/> object.</param>
    /// <returns>
    ///   <see cref="string"/> containing name for <paramref name="type"/>.
    /// </returns>
    public string BuildName(TypeDef type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      string result;
      if (!string.IsNullOrEmpty(type.MappingName))
        result = type.MappingName;
      else {
        string underlyingName = type.UnderlyingType.FullName.Substring(type.UnderlyingType.Namespace.Length + 1, type.UnderlyingType.FullName.Length - type.UnderlyingType.Namespace.Length - 1);
        result = string.IsNullOrEmpty(type.Name) ? underlyingName : type.Name;
        if (NamingConvention.NamespacePolicy == NamespacePolicy.UseNamespaceSynonym) {
          string namespacePrefix;
          try {
            namespacePrefix = NamingConvention.NamespaceSynonyms[type.UnderlyingType.Namespace];
            if (string.IsNullOrEmpty(namespacePrefix))
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
          result = string.Format("{0}.{1}", ComputeHash(type.UnderlyingType.Namespace), result);
        }
      }
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="Definitions.FieldDef"/> object.
    /// </summary>
    /// <param name="field">The <see cref="Definitions.FieldDef"/> object.</param>
    /// <returns>
    ///   <see cref="string"/> containing name for <paramref name="field"/>.
    /// </returns>
    public string BuildName(FieldDef field)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      string result = field.Name;
      if (field.UnderlyingProperty != null)
        result = field.UnderlyingProperty.Name;
      if (fieldNameRe.IsMatch(result))
        result = fieldNameRe.Match(result).Groups["name"].Value.Replace('.','_');
      return result;
    }

    internal string BuildExplicitName(TypeInfo type, string name)
    {
      return type.IsInterface ? type.UnderlyingType.Name + "_" + name : name;
    }

    internal string BuildExplicitName(FieldInfo field)
    {
      if (field.UnderlyingProperty == null || !field.UnderlyingProperty.Name.Contains("."))
        return field.Name;
      return field.UnderlyingProperty.DeclaringType.Name + "_" + field.Name;
    }

    public string BuildName(FieldInfo complexField, FieldInfo childField)
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
    /// <returns>
    ///   <see cref="string"/> containing name for <paramref name="baseColumn"/>.
    /// </returns>
    public string BuildName(FieldInfo field, ColumnInfo baseColumn)
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
    public string BuildName(ColumnInfo column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "baseColumn");
      if (column.Name.StartsWith(column.Field.DeclaringType.Name))
        throw new InvalidOperationException();
      string result = string.Concat(column.Field.DeclaringType.Name, ".", column.Name);
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="Definitions.IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="Definitions.IndexDef"/> object.</param>
    /// <returns>
    ///   <see cref="string"/> containing name for <paramref name="index"/>.
    /// </returns>
    public string BuildName(TypeDef type, IndexDef index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      string result = string.Empty;
      if (!string.IsNullOrEmpty(index.Name))
        result = index.Name;
      else if (index.IsPrimary)
        result = string.Format("PK_{0}", type.Name);
      else if (index.KeyFields.Count == 0)
        result = string.Empty;
      else if (!string.IsNullOrEmpty(index.MappingName))
        result = index.MappingName;
      else {
        if (index.KeyFields.Count == 1) {
          FieldDef field = type.Fields[index.KeyFields[0].Key];
          if (field.IsEntity)
            result = string.Format("FK_{0}", field.Name);
        }
        if (string.IsNullOrEmpty(result)) {
          string[] names = new string[index.KeyFields.Keys.Count];
          index.KeyFields.Keys.CopyTo(names, 0);
          result = string.Format("IX_{0}", string.Join("", names));
        }
      }
      return NamingConvention.Apply(result);
    }

    /// <summary>
    /// Gets the name for <see cref="Definitions.IndexDef"/> object.
    /// </summary>
    /// <param name="type">The type def.</param>
    /// <param name="index">The <see cref="IndexInfo"/> object.</param>
    /// <returns>
    ///   <see cref="string"/> containing name for <paramref name="index"/>.
    /// </returns>
    public string BuildName(TypeInfo type, IndexInfo index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      string result = string.Empty;
      if (!string.IsNullOrEmpty(index.Name))
        result = index.Name;
      else if (index.IsPrimary)
        result = string.Concat("PK_", type.Name);
      else if (!string.IsNullOrEmpty(index.MappingName))
        result = string.Format("{0}.{1}", type.Name,index.MappingName);
      else if (!index.ReflectedType.IsInterface && index.KeyColumns.Count == 0)
        return string.Empty;
      else {
        if (index.IsForeignKey)
          result = string.Format("{0}.{1}", type.Name, index.ShortName);
        if (string.IsNullOrEmpty(result)) {
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
      return NamingConvention.Apply(string.Concat(result, index.IsVirtual ? ".VIRTUAL" : string.Empty));
    }

    /// <summary>
    /// Builds the name for the <see cref="AssociationInfo"/>.
    /// </summary>
    /// <param name="target">The <see cref="AssociationInfo"/> instance to build name for.</param>
    /// <returns></returns>
    public string BuildName(AssociationInfo target)
    {
      return target.ReferencingType.Name + "." + target.ReferencingField.Name + "_" + target.ReferencedType.Name;
    }

    /// <summary>
    /// Computes a hash for the specified string.
    /// The lengs of the resulting hash is 8 bytes.
    /// </summary>
    private string ComputeHash(string source)
    {
      byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(source)); 
      return String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", hash[0], hash[1], hash[2], hash[3]);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NameProvider"/> class.
    /// </summary>
    /// <param name="namingConvention">The naming convention.</param>
    public NameProvider(NamingConvention namingConvention)
    {
      ArgumentValidator.EnsureArgumentNotNull(namingConvention, "namingConvention");
      this.namingConvention = namingConvention;
      hashAlgorithm = new MD5CryptoServiceProvider();
    }
  }
}