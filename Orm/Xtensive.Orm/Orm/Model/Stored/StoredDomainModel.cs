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
    /// Deserializes <see cref="StoredDomainModel"/>
    /// from the specified <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">Reader to provider XML text.</param>
    /// <returns>Deserialized instance.</returns>
    public static StoredDomainModel Deserialize(TextReader reader)
    {
      ArgumentValidator.EnsureArgumentNotNull(reader, "reader");
      return (StoredDomainModel) Serializer.Deserialize(reader);
    }

    /// <summary>
    /// Serializes this instance writing to the specified <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">A writer to use.</param>
    public void Serialize(TextWriter writer)
    {
      Serializer.Serialize(writer, this);
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