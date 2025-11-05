// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Transformation
{
  /// <summary>
  /// Corrects <see cref="AggregateProvider"/>'s columns of <see cref="decimal"/> type
  /// by adding information about desirable precision and scale (if such info successfully gathered).
  /// </summary>
  public sealed class AggregateOverDecimalColumnCorrector : IPreCompiler
  {
    private readonly DomainModel domainModel;

    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return new DecimalAggregateColumnRewriter(domainModel, rootProvider).Rewrite();
    }

    public AggregateOverDecimalColumnCorrector(DomainModel domainModel)
    {
      this.domainModel = domainModel;
    }
  }
}