// Copyright (C) 2013 Xtensive LLC
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.18

using System.Xml.Serialization;
using Xtensive.Core;

namespace Xtensive.Orm.Model.Stored
{
  /// <summary>
  /// Serializable collection of partial index filter expressions.
  /// </summary>
  [XmlRoot("PartialIndexes", Namespace = "")]
  public sealed class StoredPartialIndexFilterInfoCollection
  {
    private static readonly SimpleXmlSerializer<StoredPartialIndexFilterInfoCollection> Serializer
      = new SimpleXmlSerializer<StoredPartialIndexFilterInfoCollection>();

    [XmlElement("Index")]
    public StoredPartialIndexFilterInfo[] Items;

    /// <summary>
    /// Deserializes <see cref="StoredPartialIndexFilterInfoCollection"/> from string.
    /// </summary>
    /// <param name="serialized">Serialized instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static StoredPartialIndexFilterInfoCollection Deserialize(string serialized)
    {
      return Serializer.Deserialize(serialized);
    }

    /// <summary>
    /// Serializes this instance to string.
    /// </summary>
    /// <returns>Serialized instance.</returns>
    public string Serialize()
    {
      return Serializer.Serialize(this);
    }
  }
}