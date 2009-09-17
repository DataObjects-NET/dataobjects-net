// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using System;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building.FixupActions
{
  [Serializable]
  internal class CopyKeyFieldsAction : TypeAction
  {
    public TypeDef Source { get; private set; }

    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Copy key fields from '{0}' type to '{1}' type.", Source.Name, Type.Name);
    }


    // Constructors

    public CopyKeyFieldsAction(TypeDef target, TypeDef source)
      : base(target)
    {
      Source = source;
    }
  }
}