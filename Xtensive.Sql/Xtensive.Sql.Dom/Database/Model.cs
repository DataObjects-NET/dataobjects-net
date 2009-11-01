// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Notifications;
using Xtensive.Sql.Dom.Database.Providers;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents the root <see cref="Node"/> of the whole database model.
  /// </summary>
  [Serializable]
  public class Model : Node
  {
    private Server defaultServer;
    private PairedNodeCollection<Model, Server> servers;

    /// <summary>
    /// Builds the database model.
    /// </summary>
    /// <param name="provider">The database model provider.</param>
    /// <returns>Builded database model.</returns>
    public static Model Build(IModelProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      return provider.Build();
    }

    /// <summary>
    /// Creates a server.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public Server CreateServer(string name)
    {
      return new Server(this, name);
    }

    /// <summary>
    /// Default <see cref="Server"/> for this instance.
    /// </summary>
    public Server DefaultServer
    {
      get { return defaultServer; }
      set {
        this.EnsureNotLocked();
        if (defaultServer == value)
          return;
        if (value!=null && !servers.Contains(value))
          servers.Add(value);
        defaultServer = value;
      }
    }

    /// <summary>
    /// Gets the list of servers.
    /// </summary>
    /// <value>The list of servers.</value>
    public PairedNodeCollection<Model, Server> Servers
    {
      get { return servers; }
    }

   
    #region Event Handlers

    private void OnServerRemoved(object sender, CollectionChangeNotifierEventArgs<Server> args)
    {
      if (args.Item==defaultServer)
        defaultServer = servers.Count>0 ? servers[0] : null;
    }

    private void OnServerInserted(object sender, CollectionChangeNotifierEventArgs<Server> args)
    {
      if (servers.Count==1)
        defaultServer = args.Item;
    }

    private void OnServersCleared(object sender, ChangeNotifierEventArgs args)
    {
      defaultServer = null;
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
      servers.Lock(recursive);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public Model(string name) : base(name)
    {
      servers =
        new PairedNodeCollection<Model, Server>(this, "Servers", 1);
      servers.Inserted += OnServerInserted;
      servers.Removed += OnServerRemoved;
      servers.Cleared += OnServersCleared;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> class.
    /// </summary>
    public Model() : this(string.Empty)
    {
    }

    #endregion
  }
}