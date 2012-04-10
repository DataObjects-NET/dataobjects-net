// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;



namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Produces equality join between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class JoinProvider : BinaryProvider
  {
    private const string ToStringFormat = "{0}, {1}";

    /// <summary>
    /// Join operation type.
    /// </summary>
    public JoinType JoinType { get; private set; }

    /// <summary>
    /// Pairs of equal column indexes.
    /// </summary>
    public Pair<int>[] EqualIndexes { get; private set; }

    /// <summary>
    /// Pairs of equal columns.
    /// </summary>
    public Pair<Column>[] EqualColumns { get; private set; }

    
    protected override string ParametersToString()
    {
      return string.Format(ToStringFormat,
        JoinType,
        EqualColumns.Select(p => p.First.Name + " == " + p.Second.Name).ToCommaDelimitedString());
    }

    
    protected override void Initialize()
    {
      base.Initialize();
      EqualColumns = new Pair<Column>[EqualIndexes.Length];
      for (int i = 0; i < EqualIndexes.Length; i++)
        EqualColumns[i] = new Pair<Column>(
          Left.Header.Columns[EqualIndexes[i].First],
          Right.Header.Columns[EqualIndexes[i].Second]
          );
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to join.</param>
    /// <param name="right">The right provider to join.</param>
    /// <param name="joinType">The join operation type.</param>
    /// <param name="equalIndexes">The <see cref="EqualIndexes"/> property value.</param>
    /// <exception cref="ArgumentException">Wrong arguments.</exception>
    public JoinProvider(CompilableProvider left, CompilableProvider right, JoinType joinType, params Pair<int>[] equalIndexes)
      : base(ProviderType.Join, left, right)
    {
      if (equalIndexes==null || equalIndexes.Length==0)
        throw new ArgumentException(
          Strings.ExAtLeastOneColumnIndexPairMustBeSpecified, "equalIndexes");
      JoinType = joinType;
      EqualIndexes = equalIndexes;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to join.</param>
    /// <param name="right">The right provider to join.</param>
    /// <param name="joinType">The join operation type.</param>
    /// <param name="equalIndexes">Transformed to the <see cref="EqualIndexes"/> property value.</param>
    /// <exception cref="ArgumentException">Wrong arguments.</exception>
    public JoinProvider(CompilableProvider left, CompilableProvider right, JoinType joinType, params int[] equalIndexes)
      : base(ProviderType.Join, left, right)
    {
      if (equalIndexes==null || equalIndexes.Length<2)
        throw new ArgumentException(
          Strings.ExAtLeastOneColumnIndexPairMustBeSpecified, "equalIndexes");
      var ei = new Pair<int>[equalIndexes.Length / 2];
      for (int i = 0, j = 0; i < ei.Length; i++)
        ei[i] = new Pair<int>(equalIndexes[j++], equalIndexes[j++]);
      JoinType = joinType;
      EqualIndexes = ei;
    }
  }
}