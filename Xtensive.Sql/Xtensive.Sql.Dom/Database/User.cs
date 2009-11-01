// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents a an authorization identifier and identifies a set of privileges.
  /// </summary>
  [Serializable]
  public class User : ServerNode
  {
    #region ServerNode Members

    /// <summary>
    /// Changes the server.
    /// </summary>
    /// <param name="value">The new value of server property.</param>
    protected override void ChangeServer(Server value)
    {
      if (Server!=null)
        Server.Users.Remove(this);
      if (value!=null)
        value.Users.Add(this);
    }

    #endregion

    #region Constructors

    internal User(Server server, string name) : base(server, name)
    {
    }

    #endregion
  }
}
