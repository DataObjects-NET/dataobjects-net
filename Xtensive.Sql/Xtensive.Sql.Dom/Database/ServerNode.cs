// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents a <see cref="Server"/> bound object.
  /// </summary>
  [Serializable]
  public abstract class ServerNode : Node, IPairedNode<Server>
  {
    private Server server;

    /// <summary>
    /// Gets or sets the <see cref="Server"/> this instance belongs to.
    /// </summary>
    /// <value>The server this instance belongs to.</value>
    public Server Server
    {
      get { return server; }
      set {
        this.EnsureNotLocked();
        if (server != value)
          ChangeServer(value);
      }
    }

    /// <summary>
    /// Changes the server.
    /// </summary>
    /// <param name="value">The new value of server property.</param>
    protected abstract void ChangeServer(Server value);

    #region IPairedNode<Server> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<Server>.UpdatePairedProperty(string property, Server value)
    {
      this.EnsureNotLocked();
      server = value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogNode"/> class.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="name">The name.</param>
    protected ServerNode(Server server, string name) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(server, "server");
      Server = server;
    }

    #endregion
  }
}