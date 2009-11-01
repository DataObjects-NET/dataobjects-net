// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Database.Providers
{
  /// <summary>
  /// Defines a database model provider interface.
  /// </summary>
  public interface IModelProvider
  {
    /// <summary>
    /// Builds the database model.
    /// </summary>
    Model Build();

    /// <summary>
    /// Saves the specified model.
    /// </summary>
    /// <param name="model">The model.</param>
    void Save(Model model);
  }
}
