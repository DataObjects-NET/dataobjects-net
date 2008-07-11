// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public sealed class IndexProvider : CompilableProvider
  {
    private readonly IndexInfo index;

    // TODO: replace with IndexInfoRef
    public IndexInfo Index
    {
      get { return index; }
    }

    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.Indexed | ProviderOptions.Ordered; }
    }

    protected override RecordHeader BuildHeader()
    {
      return new RecordHeader(index);
    }

    // Constructor

    public IndexProvider(IndexInfo index)
    {
      this.index = index;
    }
  }
}