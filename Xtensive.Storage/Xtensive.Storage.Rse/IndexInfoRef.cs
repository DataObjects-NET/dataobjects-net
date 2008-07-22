// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="IndexInfo"/> instance.
  /// </summary>
  [Serializable]
  public sealed class IndexInfoRef
  {
    /// <summary>
    /// Name of the index.
    /// </summary>
    public string IndexName { get; private set; }

    /// <summary>
    /// Name of the reflecting type.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="IndexInfo"/> object within specified <paramref name="domainInfo"/>.
    /// </summary>
    /// <param name="domainInfo">Domain information.</param>
    public IndexInfo Resolve(DomainInfo domainInfo)
    {
      var type = domainInfo.Types[TypeName];
      if (type == null)
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));
      var index = type.Indexes[IndexName];
      if (index == null)
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "index", IndexName));

      return index;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("TypeName: {1}, Name: {0}", TypeName, IndexName);
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