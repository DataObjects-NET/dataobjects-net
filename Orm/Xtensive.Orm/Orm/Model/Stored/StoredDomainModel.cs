// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.IO;
using System.Xml.Serialization;
using Xtensive.Core;

namespace Xtensive.Orm.Model.Stored
{
  /// <summary>
  /// An xml serializable representation of <see cref="DomainModel"/>.
  /// </summary>
  [XmlRoot("DomainModel", Namespace = "")]
  public sealed class StoredDomainModel
  {
    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof (StoredDomainModel));

    /// <summary>
    /// <see cref="DomainModel.Types"/>.
    /// </summary>
    [XmlArray("Types"), XmlArrayItem("Type")]
    public StoredTypeInfo[] Types;

    /// <summary>
    /// <see cref="DomainModel.Associations"/>
    /// </summary>
    [XmlIgnore]
    public StoredAssociationInfo[] Associations;

    /// <summary>
    /// <see cref="DomainModel.Hierarchies"/>
    /// </summary>
    [XmlIgnore]
    public StoredHierarchyInfo[] Hierarchies;

    /// <summary>
    /// Deserializes <see cref="StoredDomainModel"/> from string.
    /// </summary>
    /// <param name="serialized">Serialized instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static StoredDomainModel Deserialize(string serialized)
    {
      ArgumentValidator.EnsureArgumentNotNull(serialized, "serialized");

      StoredDomainModel result;
      using (var reader = new StringReader(serialized)) {
        result = (StoredDomainModel) Serializer.Deserialize(reader);
      }

      return result;
    }

    /// <summary>
    /// Serializes this instance to string.
    /// </summary>
    /// <returns>Serialized instance.</returns>
    public string Serialize()
    {
      string result;
      using (var writer = new StringWriter()) {
        Serializer.Serialize(writer, this);
        result = writer.ToString();
      }
      return result;
    }

    /// <summary>
    /// Updates references between nodes.
    /// </summary>
    public void UpdateReferences()
    {
      new ReferenceUpdater().UpdateReferences(this);
    }
  }
}