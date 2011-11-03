// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// A parameter for accessing current tuple of left (outer) <see cref="Provider"/>
  /// within right (inner) <see cref="Provider"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public sealed class ApplyParameter
  {
    [NonSerialized]
    private Parameter<Tuple> parameter;

    /// <summary>
    /// Gets the name of this parameter.
    /// </summary>
    /// <value>The name.</value>
    public string Name {get; private set; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public Tuple Value {
      [DebuggerStepThrough]
      get { return parameter.Value; }
      [DebuggerStepThrough]
      set { parameter.Value = value; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Value for <see cref="Name"/>.</param>
    public ApplyParameter(string name)
    {
      Name = name;
      parameter = new Parameter<Tuple>(name);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ApplyParameter()
      : this(string.Empty)
    {
    }

    // Deserialization

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      parameter = new Parameter<Tuple>(Name);
    }
  }
}