// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.13

using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="CompilableProvider"/> and <see cref="ExecutableProvider"/> related extension methods.
  /// </summary>
  public static class ProviderExtensions
  {
    /// <summary>
    /// Compiles specified <paramref name="provider"/>
    /// and returns new <see cref="RecordSet"/> bound to specified <paramref name="session"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="session">The session.</param>
    /// <returns>New <see cref="RecordSet"/> bound to specified <paramref name="session"/>.</returns>
    public static RecordSet GetRecordSet(this CompilableProvider provider, Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      var compiled = session.CompilationService.Compile(provider);
      return new RecordSet(session.CreateEnumerationContext(), compiled);
    }

    /// <summary>
    /// Gets <see cref="RecordSet"/> bound to the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Provider to get <see cref="RecordSet"/> for.</param>
    /// <param name="session">Session to bind.</param>
    /// <returns>New <see cref="RecordSet"/> bound to specified <paramref name="session"/>.</returns>
    public static RecordSet GetRecordSet(this ExecutableProvider provider, Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return new RecordSet(session.CreateEnumerationContext(), provider);
    }

    /// <summary>
    /// Calculates count of elements of provided <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="session">The session.</param>
    public static long Count(this CompilableProvider provider, Session session)
    {
      return provider
        .Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count))
        .GetRecordSet(session)
        .First()
        .GetValue<long>(0);
    }
  }
}