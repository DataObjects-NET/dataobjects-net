// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

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
    /// Column joining pairs for equality join.
    /// </summary>
    public Pair<int>[] JoiningPairs { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return new RecordSetHeader(Left.Header, Right.Header);
    }

    protected override void Initialize()
    {}


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public JoinProvider(CompilableProvider left, CompilableProvider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(left, right)
    {
      LeftJoin = leftJoin;
      JoiningPairs = joiningPairs;
    }
  }
}