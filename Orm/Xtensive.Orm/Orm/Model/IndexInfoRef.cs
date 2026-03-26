// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using System.Diagnostics;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="IndexInfo"/> instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("IndexName = {IndexName}, TypeName = {TypeName}")]
  public sealed class IndexInfoRef
  {
    private const string ToStringFormat = "Index '{0}' @ {1}";

    /// <summary>
    /// Name of the index.
    /// </summary>
    public string IndexName { get; private set; }

    /// <summary>
    /// Name of the reflecting type.
    /// </summary>
    public string TypeName { get; private set; }

    public TupleDescriptor KeyTupleDescriptor { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="IndexInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public IndexInfo Resolve(DomainModel model)
    {
      TypeInfo type;
      if (!model.Types.TryGetValue(TypeName, out type))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));
      IndexInfo index;
      if (!type.Indexes.TryGetValue(IndexName, out index)) {
        var hierarchy = type.Hierarchy;
        if (hierarchy != null && hierarchy.InheritanceSchema == InheritanceSchema.SingleTable && hierarchy.Root.Indexes.TryGetValue(IndexName, out index)) 
          return index;
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "index", IndexName));
      }

      return index;
    }

    /// <summary>
    /// Creates reference for <see cref="IndexInfo"/>.
    /// </summary>
    public static implicit operator IndexInfoRef (IndexInfo indexInfo)
    {
      return new IndexInfoRef(indexInfo);
    }

    #region Equality members, ==, !=

    /// <inheritdoc/>
    public bool Equals(IndexInfoRef other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return 
        other.IndexName==IndexName && 
        other.TypeName==TypeName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as IndexInfoRef);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return 
          ((IndexName!=null ? IndexName.GetHashCode() : 0) * 397) ^ 
          (TypeName!=null ? TypeName.GetHashCode() : 0);
      }
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(IndexInfoRef x, IndexInfoRef y)
    {
      return Equals(x, y);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(IndexInfoRef x, IndexInfoRef y)
    {
      return !Equals(x, y);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, IndexName, TypeName);
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="indexInfo"><see cref="IndexInfo"/> object to make reference for.</param>
    public IndexInfoRef(IndexInfo indexInfo)
    {
      IndexName = indexInfo.Name;      
      TypeName = indexInfo.ReflectedType.Name;
      KeyTupleDescriptor = indexInfo.KeyTupleDescriptor;
    }
  }
}