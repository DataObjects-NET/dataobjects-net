// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="IndexInfo"/> instance.
  /// </summary>
  [Serializable]
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
      if (!type.Indexes.TryGetValue(IndexName, out index))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "index", IndexName));

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

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator !=(IndexInfoRef x, IndexInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator ==(IndexInfoRef x, IndexInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(IndexInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        TypeName==TypeName;
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
      return unchecked( TypeName.GetHashCode() );
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, IndexName, TypeName);
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="indexInfo"><see cref="IndexInfo"/> object to make reference for.</param>
    public IndexInfoRef(IndexInfo indexInfo)
    {
      IndexName = indexInfo.Name;      
      TypeName = indexInfo.ReflectedType.Name;
    }
  }
}