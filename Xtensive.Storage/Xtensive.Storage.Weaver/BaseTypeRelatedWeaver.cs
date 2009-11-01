// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.19

using System;
using PostSharp.CodeModel;
using PostSharp.Laos.Weaver;

namespace Xtensive.Storage.Weaver
{
  internal abstract class BaseTypeRelatedWeaver :
    LaosAspectWeaver
  {
    private ITypeSignature baseTypeSignature;

    protected ITypeSignature BaseTypeSignature
    {
      get { return baseTypeSignature; }
    }


    // Constructors

    internal BaseTypeRelatedWeaver(ITypeSignature baseTypeSignature)
    {
      this.baseTypeSignature = baseTypeSignature;
    }
  }
}
