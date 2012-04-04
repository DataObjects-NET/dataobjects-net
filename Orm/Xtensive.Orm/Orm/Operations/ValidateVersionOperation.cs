// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.19

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> version validation operation.
  /// </summary>
  [Serializable]
  public sealed class ValidateVersionOperation : EntityOperation,
    IUniqueOperation
  {
    /// <summary>
    /// Gets the original version of <see cref="Entity"/>.
    /// </summary>
    public VersionInfo Version { get; private set; }

    /// <inheritdoc/>
    public bool IgnoreIfDuplicate { get { return true; } }

    /// <inheritdoc/>
    public object Identifier
    {
      get { return Key; }
    }

    /// <inheritdoc/>
    public override string Title
    {
      get { return "Validate version"; }
    }

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, Version = {1}".FormatWith(base.Description, Version);
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      context.RegisterKey(Key, false);
    }

    /// <inheritdoc/>
    /// <exception cref="VersionConflictException">Version check failed.</exception>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var entity = session.Query.Single(Key);
      if (entity.VersionInfo != Version) {
        if (OrmLog.IsLogged(LogEventTypes.Info))
          OrmLog.Info(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3,
            session, Key, entity.VersionInfo, Version);
        throw new VersionConflictException(
          string.Format(Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, Key));
      }
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new ValidateVersionOperation(Key, Version);
      return clone;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="key">The key of the <see cref="Entity"/>.</param>
    /// <param name="version">The original version.</param>
    public ValidateVersionOperation(Key key, VersionInfo version)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(version, "version");
      Version = version;
    }

    // Serialization

    /// <inheritdoc/>
    public ValidateVersionOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Version = (VersionInfo) info.GetValue("Version", typeof (VersionInfo));
    }

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Version", Version);
    }
  }
}