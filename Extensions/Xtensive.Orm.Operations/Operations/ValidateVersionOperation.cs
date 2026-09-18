// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.19

using System;
using System.Text.Json.Serialization;
using Xtensive.Core;
using Xtensive.Orm.Operations.Interfaces;
using Xtensive.Orm.Operations.Serialization.Json;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> version validation operation.
  /// </summary>
  public sealed class ValidateVersionOperation : EntityOperation,
    IUniqueOperation
  {
    /// <summary>
    /// Gets the original version of <see cref="Entity"/>.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(VersionInfoConverter))]
    public VersionInfo Version { get; private set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool IgnoreIfDuplicate { get { return true; } }

    /// <inheritdoc/>
    [JsonIgnore]
    public object Identifier
    {
      get { return Key; }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Title
    {
      get { return "Validate version"; }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        return $"{base.Description}, Version = {Version}";
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
        //if (OrmLog.IsLogged(LogLevel.Info))
        //  OrmLog.Info(nameof(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3),
        //    session, Key, entity.VersionInfo, Version);
        throw new VersionConflictException(
          string.Format("Version of entity with key '{0}' differs from the expected one.", Key));
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
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="key">The key of the <see cref="Entity"/>.</param>
    /// <param name="version">The original version.</param>
    [JsonConstructor]
    public ValidateVersionOperation(Key key, VersionInfo version)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentIsNotDefault(version, nameof(version));
      Version = version;
    }
  }
}