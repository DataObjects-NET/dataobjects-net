// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a character set object.
  /// </summary>
  [Serializable]
  public class CharacterSet : SchemaNode
  {
    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.CharacterSets.Remove(this);
      if (value!=null)
        value.CharacterSets.Add(this);
    }

    #endregion

    #region Constructors

    internal CharacterSet(Schema schema, string name) : base(schema, name)
    {
    }

    #endregion
  }
}