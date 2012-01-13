// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a collation object thst specifies the rules for how strings 
  /// of character data are sorted and compared, based on the norms of 
  /// particular languages and locales.
  /// </summary>
  [Serializable]
  public class Collation : SchemaNode
  {
    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Collations.Remove(this);
      if (value!=null)
        value.Collations.Add(this);
    }

    #endregion

    #region Constructors

    internal Collation(Schema schema, string name) : base(schema, name)
    {
    }

    #endregion
  }
}