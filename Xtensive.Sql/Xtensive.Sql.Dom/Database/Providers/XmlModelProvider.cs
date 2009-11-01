// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Database.Providers
{
  /// <summary>
  /// Represents a database model provider that builds database model from xml file.
  /// </summary>
  public class XmlModelProvider : IModelProvider
  {
    #region IModelProvider Members

    /// <summary>
    /// Builds the database model.
    /// </summary>
    public Model Build()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Saves the specified model.
    /// </summary>
    /// <param name="model">The model.</param>
    public void Save(Model model)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
