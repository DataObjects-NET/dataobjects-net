// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A descriptor of temporary table.
  /// </summary>
  public sealed class TemporaryTableDescriptor : IPersistDescriptor
  {
    /// <summary>
    /// Gets the unique name of this temporary table.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets or sets the tuple descriptor associated with this table descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; set; }

    /// <summary>
    /// Gets or sets the table creation script.
    /// </summary>
    public string CreateStatement { get; set; }

    /// <summary>
    /// Gets or sets the table destruction script.
    /// </summary>
    public string DropStatement { get; set; }

    /// <summary>
    /// Gets or sets the persist request used to store data in temporary table.
    /// </summary>
    public PersistRequest StoreRequest { get; set; }

    /// <summary>
    /// Gets or sets the clear reqest used to delete all data from temporary table.
    /// </summary>
    public PersistRequest ClearRequest { get; set; }

    /// <summary>
    /// Gets or sets the query statement associated with this table descriptor.
    /// </summary>
    public SqlSelect QueryStatement { get; set; }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">A value for <see cref="Name"/>.</param>
    public TemporaryTableDescriptor(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
    }
  }
}