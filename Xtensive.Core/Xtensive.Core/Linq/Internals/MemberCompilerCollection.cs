// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.10

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class MemberCompilerCollection : Dictionary<MemberInfo, Pair<Delegate, MethodInfo>>
  {
    // Constructors

    public MemberCompilerCollection()
    {
    }

    public MemberCompilerCollection(IDictionary<MemberInfo, Pair<Delegate, MethodInfo>> dictionary)
      : base(dictionary)
    {
    }
  }
}