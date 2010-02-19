// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.19

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal sealed class ValidateVersionOperation : UniqueOperationBase
  {
    public VersionInfo OriginalVersion { get; private set; }

    public override bool IgnoreDuplicate { get { return true; }}

    public override void Prepare(OperationExecutionContext context)
    {
      context.RegisterKey(Key, false);
    }

    public override void Execute(OperationExecutionContext context)
    {
      var entity = Query.Single(context.Session, Key);
      if (entity.VersionInfo != OriginalVersion)
        throw new InvalidOperationException(string
          .Format(Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, Key));
    }


    // Constructors

    public ValidateVersionOperation(Key key, VersionInfo originalVersion)
      : base(key, OperationType.ValidateVersion)
    {
      ArgumentValidator.EnsureArgumentNotNull(originalVersion, "originalVersion");

      OriginalVersion = originalVersion;
    }

    // Serialization

    protected ValidateVersionOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {}
  }
}