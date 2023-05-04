// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
    private Schema defaultSchema;
    private PairedNodeCollection<Catalog, Schema> schemas;
    private PairedNodeCollection<Catalog, PartitionFunction> partitionFunctions;
    private PairedNodeCollection<Catalog, PartitionSchema> partitionSchemas;

    /// <inheritdoc />
    public override string Name
    {
      get {
        if (!IsNamesReadingDenied)
          return base.Name;
        throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!IsNamesReadingDenied)
          base.Name = value;
        else
          throw new InvalidOperationException(Strings.ExNameValueReadingOrSettingIsDenied);
      }
    }

    /// <inheritdoc />
    public override string DbName
    {
      get {
        if (!IsNamesReadingDenied)
          return base.DbName;
        throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
      set {
        if (!IsNamesReadingDenied)
          base.DbName = value;
        else
          throw new InvalidOperationException(Strings.ExDbNameValueReadingOrSettingIsDenied);
      }
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
        EnsureNotLocked();
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
    public PairedNodeCollection<Catalog, Schema> Schemas => schemas;

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
        if (partitionSchemas == null)
          partitionSchemas =
            new PairedNodeCollection<Catalog, PartitionSchema>(this, "PartitionSchemas");
        return partitionSchemas;
      }
    }

    internal bool IsNamesReadingDenied { get; private set; }

    /// <summary>
    /// Creates a schema.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public Schema CreateSchema(string name) => new(this, name);

    /// <summary>
    /// Creates the partition function.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="dataType">Type of the input parameter.</param>
    /// <param name="boundaryValues">The boundary values.</param>
    public PartitionFunction CreatePartitionFunction(string name, SqlValueType dataType, params string[] boundaryValues) =>
      new(this, name, dataType, boundaryValues);

    /// <summary>
    /// Creates the partition schema.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="partitionFunction">The partition function.</param>
    /// <param name="filegroups">The filegroups.</param>
    public PartitionSchema CreatePartitionSchema(string name, PartitionFunction partitionFunction, params string[] filegroups) =>
      new(this, name, partitionFunction, filegroups);


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
      IsNamesReadingDenied = true;
      Schemas.ForEach(s => s.MakeNamesUnreadable());
      PartitionFunctions.ForEach(pf => pf.MakeNamesUnreadable());
      PartitionSchemas.ForEach(ps => ps.MakeNamesUnreadable());
    }

    internal string GetActualName(IReadOnlyDictionary<string, string> catalogNameMap)
    {
      if (!IsNamesReadingDenied)
        return Name;
      ArgumentNullException.ThrowIfNull(catalogNameMap);

      var name = GetNameInternal();
      return catalogNameMap.TryGetValue(name, out var actualName) ? actualName : name;
    }

    internal string GetActualDbName(IReadOnlyDictionary<string, string> catalogNameMap)
    {
      if (!IsNamesReadingDenied)
        return DbName;
      if (catalogNameMap==null)
        throw new ArgumentNullException("Unable to calculate real name for catalog");

      var name = GetDbNameInternal();
      return catalogNameMap.TryGetValue(name, out var actualName) ? actualName : name;
    }

    // Constructors

    public Catalog(string name)
      : base(name)
    {
      schemas =
        new PairedNodeCollection<Catalog, Schema>(this, "Schemas", 1);
    }

    public Catalog(string name, bool caseSensitiveNames = false)
      : base(name)
    {
      schemas = caseSensitiveNames
        ? new PairedNodeCollection<Catalog, Schema>(this, "Schemas", 1, StringComparer.Ordinal)
        : new PairedNodeCollection<Catalog, Schema>(this, "Schemas", 1, StringComparer.OrdinalIgnoreCase);
    }
  }
}