// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Notifications;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents a single database server within database model.
  /// </summary>
  [Serializable]
  public class Server : Node, IPairedNode<Model>
  {
    private Catalog defaultCatalog;
    private PairedNodeCollection<Server, Catalog> catalogs;
    private PairedNodeCollection<Server, User> users;
    private Model model;

    /// <summary>
    /// Creates a catalog.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public Catalog CreateCatalog(string name)
    {
      return new Catalog(this, name);
    }

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public User CreateUser(string name)
    {
      return new User(this, name);
    }

    /// <summary>
    /// Gets or sets the database model this node belongs to.
    /// </summary>
    /// <value>The database model.</value>
    public Model Model
    {
      get { return model; }
      set {
        this.EnsureNotLocked();
        if (model == value)
          return;
        if (model!=null)
          model.Servers.Remove(this);
        if (value!=null)
          value.Servers.Add(this);
      }
    }

    /// <summary>
    /// Default <see cref="Catalog"/> for this instance.
    /// </summary>
    public Catalog DefaultCatalog
    {
      get { return defaultCatalog; }
      set {
        this.EnsureNotLocked();
        if (defaultCatalog == value)
          return;
        if (value!=null && !catalogs.Contains(value))
          catalogs.Add(value);
        defaultCatalog = value;
      }
    }

    /// <summary>
    /// Gets the list of all available catalogs.
    /// </summary>
    /// <value>The list of all catalogs.</value>
    public PairedNodeCollection<Server, Catalog> Catalogs
    {
      get { return catalogs; }
    }

    /// <summary>
    /// Gets the list of all users.
    /// </summary>
    /// <value>The list of all users.</value>
    public PairedNodeCollection<Server, User> Users
    {
      get { return users; }
    }

    #region IPairedNode<Model> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<Model>.UpdatePairedProperty(string property, Model value)
    {
      this.EnsureNotLocked();
      model = value;
    }

    #endregion

    #region Event Handlers

    private void OnCatalogRemoved(object sender, CollectionChangeNotifierEventArgs<Catalog> args)
    {
      if (args.Item==defaultCatalog)
        defaultCatalog = catalogs[0];
    }

    private void OnCatalogInserted(object sender, CollectionChangeNotifierEventArgs<Catalog> args)
    {
      if (catalogs.Count==1)
        defaultCatalog = args.Item;
    }

    private void OnCatalogsCleared(object sender, ChangeNotifierEventArgs args)
    {
      defaultCatalog = null;
    }

    #endregion

    #region ILockable Members 

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      catalogs.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal Server(Model model, string name) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      Model = model;
      catalogs =
        new PairedNodeCollection<Server, Catalog>(this, "Catalogs", 1);
      catalogs.Inserted += OnCatalogInserted;
      catalogs.Removed += OnCatalogRemoved;
      catalogs.Cleared += OnCatalogsCleared;
      users =
        new PairedNodeCollection<Server, User>(this, "Users", 16);
    }

    #endregion
  }
}