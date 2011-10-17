// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.10

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Linq
{
  internal sealed class MemberCompilerCollection
  {
    private readonly Dictionary<MemberInfo, MemberCompilerRegistration> items = new Dictionary<MemberInfo, MemberCompilerRegistration>();

    public int Count { get { return items.Count; } }

    public void Add(MemberCompilerRegistration registration)
    {
      items.Add(registration.TargetMember, registration);
    }

    public MemberCompilerRegistration Get(MemberInfo member)
    {
      MemberCompilerRegistration result;
      items.TryGetValue(member, out result);
      return result;
    }

    public void MergeWith(MemberCompilerCollection otherSource, bool failOnDuplicate)
    {
      if (failOnDuplicate) {
        foreach (var key in otherSource.items.Keys)
          if (items.ContainsKey(key))
            throw new InvalidOperationException(string.Format(Strings.ExCompilerForXIsAlreadyRegistered, key.GetFullName(true)));
      }

      foreach (var registration in otherSource.items.Values)
        items[registration.TargetMember] = registration;
    }


    // Constructors

    public MemberCompilerCollection()
    {
    }
  }
}