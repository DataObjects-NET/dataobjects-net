// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider for equality join operation between <see cref="BinaryProvider.Left"/> and <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class JoinProvider : BinaryProvider
  {
    /// <summary>
    /// Indicates whether current join operation should be executed as left join.
    /// </summary>
    public bool LeftJoin { get; private set; }

    /// <summary>
    /// Pairs of equal column indexes.
    /// </summary>
    public Pair<int>[] EqualIndexes { get; private set; }

    /// <summary>
    /// Pairs of equal columns.
    /// </summary>
    public Pair<Column>[] EqualColumns { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return string.Format("{0}, {1}",
        LeftJoin ? "Left" : "Inner",
        EqualColumns.Select(p => p.First.Name + "=" + p.Second.Name).ToCommaDelimitedString());
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      EqualColumns = new Pair<Column>[EqualIndexes.Length];
      for (int i = 0; i < EqualIndexes.Length; i++)
        EqualColumns[i] = new Pair<Column>(
          Left.Header.Columns[EqualIndexes[i].First],
          Right.Header.Columns[EqualIndexes[i].Second]);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="left">The left provider to join.</param>
    /// <param name="right">The right provider to join.</param>
    /// <param name="leftJoin">If set to <see langword="true"/>, left join will be performed;
    /// otherwise, inner join will be performed.</param>
    /// <param name="equalIndexes">The <see cref="EqualIndexes"/> property value.</param>
    /// <exception cref="ArgumentException">Wrong arguments.</exception>
    public JoinProvider(CompilableProvider left, CompilableProvider right, bool leftJoin, 
      params Pair<int>[] equalIndexes)
      : base(left, right)
    {
      if (equalIndexes==null || equalIndexes.Length==0)
        throw new ArgumentException(
          Strings.ExAtLeastOneColumnIndexPairMustBeSpecified, "equalIndexes");
      LeftJoin = leftJoin;
      EqualIndexes = equalIndexes;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="left">The left provider to join.</param>
    /// <param name="right">The right provider to join.</param>
    /// <param name="leftJoin">If set to <see langword="true"/>, left join will be performed;
    /// <param name="equalIndexes">Transformed to the <see cref="EqualIndexes"/> property value.</param>
    /// otherwise, inner join will be performed.</param>
    /// <exception cref="ArgumentException">Wrong arguments.</exception>
    public JoinProvider(CompilableProvider left, CompilableProvider right, bool leftJoin, 
      params int[] equalIndexes)
      : base(left, right)
    {
      if (equalIndexes==null || equalIndexes.Length<2)
        throw new ArgumentException(
          Strings.ExAtLeastOneColumnIndexPairMustBeSpecified, "equalIndexes");
      var ei = new Pair<int>[equalIndexes.Length / 2];
      for (int i = 0, j = 0; i < ei.Length; i++)
        ei[i] = new Pair<int>(equalIndexes[j++], equalIndexes[j++]);
      LeftJoin = leftJoin;
      EqualIndexes = ei;
    }
  }
}