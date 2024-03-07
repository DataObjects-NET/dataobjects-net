// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.09.13

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
    /// and returns new <see cref="RecordSetReader"/> bound to specified <paramref name="session"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="session">The session.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <returns>New <see cref="RecordSetReader"/> bound to specified <paramref name="session"/>.</returns>
    public static RecordSetReader GetRecordSetReader(
      this CompilableProvider provider, Session session, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, nameof(provider));
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      var executableProvider = session.Compile(provider);
      return executableProvider.GetRecordSetReader(session, parameterContext);
    }

    /// <summary>
    /// Calculates count of elements of provided <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="session">The session.</param>
    public static long Count(this CompilableProvider provider, Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, nameof(provider));
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      using var recordSetReader = provider
        .Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count))
        .GetRecordSetReader(session, new ParameterContext());
      return recordSetReader.MoveNext() && recordSetReader.Current != null
        ? recordSetReader.Current.GetValue<long>(0)
        : throw new InvalidOperationException("Sequence contains no elements.");
    }
  }
}