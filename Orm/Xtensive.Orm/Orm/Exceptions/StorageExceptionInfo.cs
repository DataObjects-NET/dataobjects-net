// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.30

using System;
using System.Text;

using Xtensive.Orm.Model;


namespace Xtensive.Orm
{
  /// <summary>
  /// Extended information about <see cref="StorageException"/>.
  /// All fields are optional.
  /// </summary>
  [Serializable]
  public class StorageExceptionInfo
  {
    /// <summary>
    /// Type in which error occurred (if any).
    /// </summary>
    public TypeInfo Type { get; private set; }
    /// <summary>
    /// Field in which error occurred (if any).
    /// </summary>
    public FieldInfo Field { get; private set; }
    /// <summary>
    /// Value that caused error (if any).
    /// </summary>
    public string Value { get; private set; }
    /// <summary>
    /// Constraint that was violated (if any).
    /// </summary>
    public string Constraint { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      var builder = new StringBuilder();
      if (Type!=null)
        builder.AppendFormat(Strings.EntityX, Type);
      if (Field!=null)
        builder.AppendFormat(Strings.FieldX, Field);
      // Do not write Value & Constraint values, those are directly taken from SqlExceptionInfo.
      // They will be provided by its ToString() method.
      return builder.ToString();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    /// <param name="field">Value for <see cref="Field"/>.</param>
    /// <param name="value">Value for <see cref="Value"/>.</param>
    /// <param name="constraint">Value for <see cref="Constraint"/>.</param>
    public StorageExceptionInfo(TypeInfo type, FieldInfo field, string value, string constraint)
    {
      Type = type;
      Field = field;
      Value = value;
      Constraint = constraint;
    }
  }
}