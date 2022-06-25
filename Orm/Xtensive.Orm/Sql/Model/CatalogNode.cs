// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a <see cref="Catalog"/> bound object.
  /// </summary>
  [Serializable]
  public abstract class CatalogNode : Node, IPairedNode<Catalog>
  {
    private Catalog catalog;
    private bool isNamesReadingDenied = false;

    /// <inheritdoc />
    public override string Name
    {
      get {
        if (!isNamesReadingDenied)
          return base.Name;
        throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!isNamesReadingDenied)
          base.Name = value;
        else
          throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
    }

    /// <inheritdoc />
    public override string DbName
    {
      get {
        if (!isNamesReadingDenied)
          return base.DbName;
        throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!isNamesReadingDenied)
          base.DbName = value;
        else
          throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Catalog"/> this instance belongs to.
    /// </summary>
    /// <value>The catalog this instance belongs to.</value>
    public Catalog Catalog
    {
      get { return catalog; }
      set {
        EnsureNotLocked();
        if (catalog != value)
          ChangeCatalog(value);
      }
    }

    /// <summary>
    /// Changes the catalog.
    /// </summary>
    /// <param name="value">The new value of catalog property.</param>
    protected abstract void ChangeCatalog(Catalog value);

    #region IPairedNode<Catalog> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<Catalog>.UpdatePairedProperty(string property, Catalog value)
    {
      EnsureNotLocked();
      catalog = value;
    }

    #endregion

    internal void MakeNamesUnreadable()
    {
      isNamesReadingDenied = true;
    }

    internal string GetActualName(IReadOnlyDictionary<string, string> nodeNameMap)
    {
      if (!isNamesReadingDenied)
        return Name;
      if (nodeNameMap==null)
        throw new ArgumentNullException("nodeNameMap");

      var name = GetNameInternal();
      string actualName;
      if (nodeNameMap.TryGetValue(name, out actualName))
        return actualName;
      return name;
    }

    internal string GetActualDbName(IReadOnlyDictionary<string, string> nodeNameMap)
    {
      if (!isNamesReadingDenied)
        return DbName;
      if (nodeNameMap==null)
        throw new ArgumentNullException("nodeNameMap");

      var name = GetDbNameInternal();
      string actualName;
      if (nodeNameMap.TryGetValue(name, out actualName))
        return actualName;
      return name;
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogNode"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="name">The name.</param>
    protected CatalogNode(Catalog catalog, string name)
      : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog, "catalog");
      Catalog = catalog;
    }

    #endregion
  }
}