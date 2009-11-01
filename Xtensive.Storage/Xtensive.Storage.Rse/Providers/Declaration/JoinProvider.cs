// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using Xtensive.Core;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public class JoinProvider : CompilableProvider
  {
    private readonly Pair<int>[] joiningPairs;

    public CompilableProvider Left { get; private set; }

    public CompilableProvider Right { get; private set; }

    public bool LeftJoin { get; private set; }

    public Pair<int>[] JoiningPairs
    {
      get { return joiningPairs; }
    }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(Left.Header, Right.Header);
    }

    // Constructor

    public JoinProvider(CompilableProvider left, CompilableProvider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(left, right)
    {
      this.joiningPairs = joiningPairs;
      Left = left;
      Right = right;
      LeftJoin = leftJoin;
    }
  }
}