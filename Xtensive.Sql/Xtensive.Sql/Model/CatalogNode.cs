// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a <see cref="Catalog"/> bound object.
  /// </summary>
  [Serializable]
  public abstract class CatalogNode : Node, IPairedNode<Catalog>
  {
    private Catalog catalog;

    /// <summary>
    /// Gets or sets the <see cref="Catalog"/> this instance belongs to.
    /// </summary>
    /// <value>The catalog this instance belongs to.</value>
    public Catalog Catalog
    {
      get { return catalog; }
      set {
        this.EnsureNotLocked();
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
      this.EnsureNotLocked();
      catalog = value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogNode"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="name">The name.</param>
    protected CatalogNode(Catalog catalog, string name) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog, "catalog");
      Catalog = catalog;
    }

    #endregion
  }
}