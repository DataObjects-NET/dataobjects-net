// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents any <see cref="Schema"/> bound <see cref="Node"/>.
  /// </summary>
  [Serializable]
  public abstract class SchemaNode : Node, IPairedNode<Schema>
  {
    private Schema schema;

    /// <summary>
    /// <see cref="Schema"/> instance this instance belongs to.
    /// </summary>
    public Schema Schema
    {
      get { return schema; }
      set {
        this.EnsureNotLocked();
        if (schema != value)
          ChangeSchema(value);
      }
    }

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected abstract void ChangeSchema(Schema value);

    #region IPairedNode<Schema> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<Schema>.UpdatePairedProperty(string property, Schema value)
    {
      this.EnsureNotLocked();
      schema = value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaNode"/> class.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="name">The name.</param>
    protected SchemaNode(Schema schema, string name) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      Schema = schema;
    }

    #endregion
  }
}