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


    /// <summary>
    /// Gets a value indicating whether to ignore the duplicate of this operation,
    /// or to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IgnoreIfDuplicate { get { return true; } }


    /// <summary>
    /// Gets object identifier.
    /// </summary>
    public object Identifier
    {
      get { return Key; }
    }


    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public override string Title
    {
      get { return "Validate version"; }
    }


    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description {
      get {
        return "{0}, Version = {1}".FormatWith(base.Description, Version);
      }
    }


    /// <summary>
    /// Prepares the self.
    /// </summary>
    /// <param name="context">The context.</param>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      context.RegisterKey(Key, false);
    }


    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    /// <exception cref="VersionConflictException">Version check failed.</exception>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var entity = session.Query.Single(Key);
      if (entity.VersionInfo != Version) {
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3,
            session, Key, entity.VersionInfo, Version);
        throw new VersionConflictException(
          string.Format(Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, Key));
      }
    }


    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    /// <param name="clone"></param>
    /// <returns></returns>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new ValidateVersionOperation(Key, Version);
      return clone;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
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


    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateVersionOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    public ValidateVersionOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Version = (VersionInfo) info.GetValue("Version", typeof (VersionInfo));
    }


    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Version", Version);
    }
  }
}