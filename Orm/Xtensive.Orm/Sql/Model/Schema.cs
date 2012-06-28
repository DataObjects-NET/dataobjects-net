// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// A schema is a persistent descriptor that includes the name of the SQL-schema,
  /// the  <see cref="CharacterSet"> default character set</see> for the schema,
  /// and the descriptor of every component of the schema.
  /// </summary>
  [Serializable]
  public class Schema : CatalogNode
  {
    private CharacterSet defaultCharacterSet;

    /// <summary>
    /// Creates the sequence.
    /// </summary>
    /// <param name="name">The name.</param>
    public Sequence CreateSequence(string name)
    {
      return new Sequence(this, name);
    }

    /// <summary>
    /// Creates the temporary table.
    /// </summary>
    /// <param name="name">The name.</param>
    public TemporaryTable CreateTemporaryTable(string name)
    {
      return new TemporaryTable(this, name);
    }

    /// <summary>
    /// Creates the table.
    /// </summary>
    /// <param name="name">The name.</param>
    public Table CreateTable(string name)
    {
      return new Table(this, name);
    }

    /// <summary>
    /// Creates the view.
    /// </summary>
    /// <param name="name">The name.</param>
    public View CreateView(string name)
    {
      return new View(this, name);
    }

    /// <summary>
    /// Creates the view.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="definition">The select statement.</param>
    public View CreateView(string name, SqlNative definition)
    {
      return new View(this, name, definition);
    }

    /// <summary>
    /// Creates the view.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="definition">The select statement.</param>
    /// <param name="checkOptions">The check options.</param>
    public View CreateView(string name, SqlNative definition, CheckOptions checkOptions)
    {
      return new View(this, name, definition, checkOptions);
    }

    /// <summary>
    /// Creates the assertion.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="isDeferrable">Is assertion deferrable.</param>
    /// <param name="isInitiallyDeferred">Is assertion initially deferred.</param>
    public Assertion CreateAssertion(string name, SqlExpression condition, bool? isDeferrable, bool? isInitiallyDeferred)
    {
      return new Assertion(this, name, condition, isDeferrable, isInitiallyDeferred);
    }

    /// <summary>
    /// Creates the assertion.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="condition">The condition.</param>
    public Assertion CreateAssertion(string name, SqlExpression condition)
    {
      return new Assertion(this, name, condition, null, null);
    }

    /// <summary>
    /// Creates the character set.
    /// </summary>
    /// <param name="name">The name.</param>
    public CharacterSet CreateCharacterSet(string name)
    {
      return new CharacterSet(this, name);
    }

    /// <summary>
    /// Creates the translation.
    /// </summary>
    /// <param name="name">The name.</param>
    public Translation CreateTranslation(string name)
    {
      return new Translation(this, name);
    }

    /// <summary>
    /// Creates the collation.
    /// </summary>
    /// <param name="name">The name.</param>
    public Collation CreateCollation(string name)
    {
      return new Collation(this, name);
    }

    /// <summary>
    /// Creates the domain.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="dataType">Datatype.</param>
    /// <param name="defaultValue">The default value.</param>
    public Domain CreateDomain(string name, SqlValueType dataType, SqlExpression defaultValue)
    {
      return new Domain(this, name, dataType, defaultValue);
    }

    /// <summary>
    /// Creates the domain.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="dataType">Datatype.</param>
    public Domain CreateDomain(string name, SqlValueType dataType)
    {
      return new Domain(this, name, dataType, null);
    }

    /// <summary>
    /// Gets the sequences.
    /// </summary>
    /// <value>The sequences.</value>
    public PairedNodeCollection<Schema, Sequence> Sequences { get; private set; }

    /// <summary>
    /// Gets the assertions.
    /// </summary>
    /// <value>The assertions.</value>
    public PairedNodeCollection<Schema, Assertion> Assertions { get; private set; }

    /// <summary>
    /// Gets the domains.
    /// </summary>
    /// <value>The domains.</value>
    public PairedNodeCollection<Schema, Domain> Domains { get; private set; }

    /// <summary>
    /// Gets the collations.
    /// </summary>
    /// <value>The collations.</value>
    public PairedNodeCollection<Schema, Collation> Collations { get; private set; }

    /// <summary>
    /// Gets the character sets.
    /// </summary>
    /// <value>The character sets.</value>
    public PairedNodeCollection<Schema, CharacterSet> CharacterSets { get; private set; }

    /// <summary>
    /// Gets or sets the default character set.
    /// </summary>
    /// <value>The default character set.</value>
    public CharacterSet DefaultCharacterSet
    {
      get
      {
        if (defaultCharacterSet != null)
          return defaultCharacterSet;
        if (CharacterSets.Count > 0)
          return CharacterSets[0];
        return null;
      }
      set {
        this.EnsureNotLocked();
        if (defaultCharacterSet == value)
          return;
        if (value!=null && !CharacterSets.Contains(value))
          CharacterSets.Add(value);
        defaultCharacterSet = value;
      }
    }

    /// <summary>
    /// Gets the translations.
    /// </summary>
    /// <value>The translations.</value>
    public PairedNodeCollection<Schema, Translation> Translations { get; private set; }

    /// <summary>
    /// Gets the views.
    /// </summary>
    /// <value>The views.</value>
    public PairedNodeCollection<Schema, View> Views { get; private set; }

    /// <summary>
    /// Gets the tables.
    /// </summary>
    /// <value>The tables.</value>
    public PairedNodeCollection<Schema, Table> Tables { get; private set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public string Owner { get; set; }

    #region CatalogNode Members

    /// <summary>
    /// Changes the catalog.
    /// </summary>
    /// <param name="value">The new value of catalog property.</param>
    protected override void ChangeCatalog(Catalog value)
    {
      if (Catalog!=null)
        Catalog.Schemas.Remove(this);
      if (value!=null)
        value.Schemas.Add(this);
    }

    #endregion

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      Tables.Lock(recursive);
      Views.Lock(recursive);
      Assertions.Lock(recursive);
      CharacterSets.Lock(recursive);
      Collations.Lock(recursive);
      Translations.Lock(recursive);
      Domains.Lock(recursive);
      Sequences.Lock(recursive);
    }

    #endregion

    // Constructors

    internal Schema(Catalog catalog, string name)
      : base(catalog, name)
    {
      Tables = new PairedNodeCollection<Schema, Table>(this, "Tables");
      Views = new PairedNodeCollection<Schema, View>(this, "Views");
      Assertions = new PairedNodeCollection<Schema, Assertion>(this, "Assertions");
      CharacterSets = new PairedNodeCollection<Schema, CharacterSet>(this, "CharacterSets");
      Collations = new PairedNodeCollection<Schema, Collation>(this, "Collations");
      Translations = new PairedNodeCollection<Schema, Translation>(this, "Translations");
      Domains = new PairedNodeCollection<Schema, Domain>(this, "Domains");
      Sequences = new PairedNodeCollection<Schema, Sequence>(this, "Sequences");
    }
  }
}