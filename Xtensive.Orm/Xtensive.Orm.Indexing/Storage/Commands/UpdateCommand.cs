// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Commands.Resources;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Update command.
  /// </summary>
  [Serializable]
  public class UpdateCommand : Command<NoResult>
  {
    private string tableName;
    private Tuple key;
    private Tuple value;
    private bool keyMustExist;
    
    /// <summary>
    /// Gets or sets the name of affected table.
    /// </summary>
    public string TableName {
      get { return tableName; }
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        tableName = value;
      }
    }

    /// <summary>
    /// Gets or sets the key.
    /// <see langword="null" /> indicates that
    /// key must be generated automatically.
    /// </summary>
    public Tuple Key {
      get { return key; }
      set { key = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether key must exist in the table.
    /// </summary>
    public bool KeyMustExist {
      get { return keyMustExist; }
      set { keyMustExist = value; }
    }

    /// <summary>
    /// Gets or sets the new value.
    /// <see langword="null" /> indicates that row must be removed.
    /// </summary>
    public Tuple Value {
      get { return value; }
      set { this.value = value; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string suffix = KeyMustExist ? Strings.KeyMustExistsSuffix : string.Empty;
      return string.Format(Strings.UpdateCommandFormat, base.ToString(), Key, Value, suffix);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected UpdateCommand()
      : base(CommandType.Update)
    {
    }
  }
}