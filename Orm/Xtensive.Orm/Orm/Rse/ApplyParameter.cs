// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.15

using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// A parameter for accessing current tuple of left (outer) <see cref="Provider"/>
  /// within right (inner) <see cref="Provider"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public sealed class ApplyParameter : ISerializable
  {
    [NonSerialized]
    private Parameter<Tuple> parameter;


    /// <summary>
    /// Gets the name of this parameter.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public Tuple Value {
      [DebuggerStepThrough]
      get { return parameter.Value; }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Name", Name);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">Value for <see cref="Name"/>.</param>
    public ApplyParameter(string name)
    {
      Name = name;
      parameter = new Parameter<Tuple>(name);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ApplyParameter()
      : this(string.Empty)
    {
    }

    public ApplyParameter(SerializationInfo info, StreamingContext context)
    {
      Name = info.GetString("Name");
      parameter = new Parameter<Tuple>(Name);
    }

    // Deserialization

    //[OnDeserialized]
    //private void OnDeserialized(StreamingContext context)
    //{
    //  parameter = new Parameter<Tuple>(Name);
    //}
  }
}