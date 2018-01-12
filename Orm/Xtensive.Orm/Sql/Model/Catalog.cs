// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a single database catalog that can contain multiple database schemas.
  /// </summary>
  [Serializable]
  public class Catalog : Node
  {
    private bool isNamesReadingDenied = false;

    private Schema defaultSchema;
    private PairedNodeCollection<Catalog, Schema> schemas;
    private PairedNodeCollection<Catalog, PartitionFunction> partitionFunctions;
    private PairedNodeCollection<Catalog, PartitionSchema> partitionSchemas;

    /// <inheritdoc />
    public override string Name
    {
      get {
        if (!isNamesReadingDenied)
          return base.Name;
        throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!isNamesReadingDenied)
          base.Name = value;
        throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
    }

    /// <inheritdoc />
    public override string DbName
    {
      get {
        if (!isNamesReadingDenied)
          return base.DbName;
        throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!isNamesReadingDenied)
          base.DbName = value;
        throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
    }

    /// <summary>
    /// Creates a schema.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public Schema CreateSchema(string name)
    {
      return new Schema(this, name);
    }

    /// <summary>
    /// Creates the partition function.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="dataType">Type of the input parameter.</param>
    /// <param name="boundaryValues">The boundary values.</param>
    public PartitionFunction CreatePartitionFunction(string name, SqlValueType dataType, params string[] boundaryValues)
    {
      return new PartitionFunction(this, name, dataType, boundaryValues);
    }

    /// <summary>
    /// Creates the partition schema.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="partitionFunction">The partition function.</param>
    /// <param name="filegroups">The filegroups.</param>
    public PartitionSchema CreatePartitionSchema(string name, PartitionFunction partitionFunction, params string[] filegroups)
    {
      return new PartitionSchema(this, name, partitionFunction, filegroups);
    }

    /// <summary>
    /// Default <see cref="Schema"/> of this instance.
    /// </summary>
    /// <value></value>
    public Schema DefaultSchema
    {
      get
      {
        if (defaultSchema != null)
          return defaultSchema;
        if (Schemas.Count > 0)
          return Schemas[0];
        return null;
      }
      set {
        this.EnsureNotLocked();
        if (defaultSchema == value)
          return;
        if (value!=null && !schemas.Contains(value))
          schemas.Add(value);
        defaultSchema = value;
      }
    }

    /// <summary>
    /// Gets the schemas.
    /// </summary>
    /// <value>The schemas.</value>
    public PairedNodeCollection<Catalog, Schema> Schemas
    {
      get { return schemas; }
    }

    /// <summary>
    /// Gets the partition functions.
    /// </summary>
    /// <value>The partition functions.</value>
    public PairedNodeCollection<Catalog, PartitionFunction> PartitionFunctions
    {
      get
      {
        if (partitionFunctions==null)
          partitionFunctions = new PairedNodeCollection<Catalog, PartitionFunction>(this, "PartitionFunctions");
        return partitionFunctions;
      }
    }

    /// <summary>
    /// Gets the partition schemes.
    /// </summary>
    /// <value>The partition schemes.</value>
    public PairedNodeCollection<Catalog, PartitionSchema> PartitionSchemas
    {
      get
      {
        if (partitionSchemas==null)
          partitionSchemas =
            new PairedNodeCollection<Catalog, PartitionSchema>(this, "PartitionSchemas");
        return partitionSchemas;
      }
    }

    #region ILockable Members 

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      schemas.Lock(recursive);
    }

    #endregion

    internal void MakeNamesUnreadable()
    {
      isNamesReadingDenied = true;
      this.Schemas.ForEach(s => s.MakeNamesUnreadable());
      this.PartitionFunctions.ForEach(pf => pf.MakeNamesUnreadable());
      this.PartitionSchemas.ForEach(ps => ps.MakeNamesUnreadable());
    }

    internal string GetActualName(IDictionary<string, string> databaseMap)
    {
      if (!isNamesReadingDenied)
        return Name;
      if (databaseMap==null)
        throw new InvalidOperationException("Unable to calculate real name for catalog");
      var name = GetNameInternal();
      string actualName;
      if (databaseMap.TryGetValue(name, out actualName))
        return actualName;
      return name;
    }

    internal string GetActualDbName(IDictionary<string, string> databaseMap)
    {
      if (!isNamesReadingDenied)
        return DbName;
      if (databaseMap==null)
        throw new InvalidOperationException("Unable to calculate real name for catalog");
      var name = GetDbNameInternal();
      string actualName;
      if (databaseMap.TryGetValue(name, out actualName))
        return actualName;
      return name;
    }

    // Constructors

    public Catalog(string name) : base(name)
    {
      schemas =
        new PairedNodeCollection<Catalog, Schema>(this, "Schemas", 1);
    }
  }
}