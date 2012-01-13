// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a characher translation object.
  /// </summary>
  [Serializable]
  public class Translation : SchemaNode
  {
    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Translations.Remove(this);
      if (value!=null)
        value.Translations.Add(this);
    }

    #endregion

    #region Constructors

    internal Translation(Schema schema, string name) : base(schema, name)
    {
    }

    #endregion
  }
}
