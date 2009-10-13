// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  internal sealed class IndexRedundantColumnRemover : RedundantColumnRemoverBase
  {
    protected override Provider VisitIndex(IndexProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitRangeSet(RangeSetProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitRange(RangeProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitSeek(SeekProvider provider)
    {
      return SubstituteSelect(provider);
    }

    // Constructors

    public IndexRedundantColumnRemover(SelectProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}