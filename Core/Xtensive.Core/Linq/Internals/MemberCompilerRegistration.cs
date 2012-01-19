// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.16

using System;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class MemberCompilerRegistration
  {
    public readonly MemberInfo TargetMember;
    public readonly Delegate CompilerInvoker;

    public MemberCompilerRegistration(MemberInfo targetMember, Delegate compilerInvoker)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetMember, "targetMember");
      ArgumentValidator.EnsureArgumentNotNull(compilerInvoker, "compilerInvoker");

      TargetMember = targetMember;
      CompilerInvoker = compilerInvoker;
    }
  }
}