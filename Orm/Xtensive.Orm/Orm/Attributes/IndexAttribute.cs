// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines secondary index.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\Attributes\AttributesTest.cs" region="Model" />
  /// </example>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
  public sealed class IndexAttribute : StorageAttribute
  {
    internal double? fillFactor;
    internal bool? unique;

    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Key fields that compose the index.
    /// </summary>
    public string[] KeyFields { get; set; }

    /// <summary>
    /// Non key fields that are included into the index.
    /// </summary>
    public string[] IncludedFields { get; set; }

    /// <summary>
    /// Fill factor for this index, must be a real number between 
    /// <see langword="0"/> and <see langword="1"/>.
    /// </summary>
    public double FillFactor
    {
      get { return fillFactor ?? 0; }
      set { fillFactor = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool Unique
    {
      get { return unique ?? false; }
      set { unique = value; }
    }

    /// <summary>
    /// Gets or sets member name that provides filtering expression for partial index.
    /// This member should be static property or static method without parameters.
    /// It should return either <see cref="LambdaExpression"/> or any descendant of it.
    /// <see cref="LambdaExpression"/> should define a function that takes one argument
    /// and returns <see cref="bool"/>.
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// Gets or sets type that contains member specified by <see cref="Filter"/>.
    /// If <see cref="FilterType"/> is not set,
    /// type that <see cref="IndexAttribute"/> is applied to is used.
    /// </summary>
    public Type FilterType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this index should be clustered.
    /// If underlying RDBMS does not support clustered indexes, this value is ignored.
    /// Default value is <see langword="false" />.
    /// </summary>
    public bool Clustered { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyField">The first key field.</param>
    /// <param name="keyFields">The other (optional) key fields.</param>
    public IndexAttribute(string keyField, params string[] keyFields)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyField, "keyField");

      if (keyFields==null || keyFields.Length==0) {
        KeyFields = new[] {keyField};
      }
      else {
        KeyFields = new string[keyFields.Length + 1];
        KeyFields[0] = keyField;
        Array.Copy(keyFields, 0, KeyFields, 1, keyFields.Length);
      }
    }
  }
}
