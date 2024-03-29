// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A mapping model of storage.
  /// </summary>
  [Serializable]
  public sealed class DomainModel : Node
  {
    /// <summary>
    /// Gets the <see cref="TypeInfo"/> instances contained in this instance.
    /// </summary>
    public TypeInfoCollection Types { get; }

    /// <summary>
    /// Gets real indexes contained in this instance.
    /// </summary>
    public IndexInfoCollection RealIndexes { get; }

    /// <summary>
    /// Gets full-text indexes contained in this instance.
    /// </summary>
    public FullTextIndexInfoCollection FullTextIndexes { get; }

    /// <summary>
    /// Gets the hierarchies.
    /// </summary>
    public HierarchyInfoCollection Hierarchies { get; }

    /// <summary>
    /// Gets the collection providing information about associations.
    /// </summary>
    public AssociationInfoCollection Associations { get; }

    /// <summary>
    /// Gets the collection providing information about databases.
    /// </summary>
    public NodeCollection<DatabaseInfo> Databases { get; }

    /// <inheritdoc/>
    public override void UpdateState()
    {
      base.UpdateState();

      Hierarchies.UpdateState();
      Types.UpdateState();
      RealIndexes.UpdateState();
      Associations.UpdateState();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Hierarchies.Lock(true);
      Types.Lock(true);
      RealIndexes.Lock(true);
      Associations.Lock(true);
      Databases.Lock(true);
      FullTextIndexes.Lock(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel"/> class.
    /// </summary>
    public DomainModel()
    {
      Types = new TypeInfoCollection(this, "Types");
      RealIndexes = new IndexInfoCollection(this, "RealIndexes");
      Hierarchies = new HierarchyInfoCollection(this, "Hierarchies");
      Associations = new AssociationInfoCollection(this, "Associations");
      Databases = new NodeCollection<DatabaseInfo>(this, "Databases");
      FullTextIndexes = new FullTextIndexInfoCollection();
    }
  }
}
