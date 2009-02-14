using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation.New
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers.
  /// Compiles <see cref="CompilableProvider"/>s int <see cref="ExecutableProvider"/>.
  /// </summary>
  public abstract class Compiler : ICompiler
  {
    /// <summary>
    /// Gets execution site location.
    /// </summary>
    public UrlInfo Location { get; private set; }

    /// <inheritdoc/>
    ExecutableProvider ICompiler.Compile(CompilableProvider provider, ExecutableProvider[] sources)
    {
      if (provider == null)
        return null;
      
      if (sources.Any(s => s == null))
        return null;
      var ep = Compile(provider, sources);
      ep.Location = Location;
      return IsCompatible(ep) ? ep : ToCompatible(ep);
    }

    /// <inheritdoc/>
    public abstract bool IsCompatible(ExecutableProvider provider);

    /// <inheritdoc/>
    public abstract ExecutableProvider ToCompatible(ExecutableProvider provider);


    /// <summary>
    /// Compiles the specified <see cref="CompilableProvider"/>.
    /// </summary>
    /// <param name="cp">The provider to compile.</param>
    /// <param name="sources">Compiled sources.</param>
    public virtual ExecutableProvider Compile (CompilableProvider cp, ExecutableProvider[] sources)
    {
      if (cp == null)
        return null;
      ExecutableProvider result;
      ProviderType providerType = cp.Type;
      switch (providerType) {
        case ProviderType.Index:
          result = VisitIndex((IndexProvider)cp, sources);
          break;
        case ProviderType.Reindex:
          result = VisitReindex((ReindexProvider)cp, sources);
          break;
        case ProviderType.Store:
          result = VisitStore((StoredProvider)cp, sources);
          break;
        case ProviderType.Aggregate:
          result = VisitAggregate((AggregateProvider)cp, sources);
          break;
        case ProviderType.Alias:
          result = VisitAlias((AliasProvider)cp, sources);
          break;
        case ProviderType.Calculate:
          result = VisitCalculate((CalculationProvider)cp, sources);
          break;
        case ProviderType.Distinct:
          result = VisitDistinct((DistinctProvider)cp, sources);
          break;
        case ProviderType.Filter:
          result = VisitFilter((FilterProvider)cp, sources);
          break;
        case ProviderType.Join:
          result = VisitJoin((JoinProvider)cp, sources);
          break;
        case ProviderType.Sort:
          result = VisitSort((SortProvider)cp, sources);
          break;
        case ProviderType.Range:
          result = VisitRange((RangeProvider)cp, sources);
          break;
        case ProviderType.Raw:
          result = VisitRaw((RawProvider)cp, sources);
          break;
        case ProviderType.Seek:
          result = VisitSeek((SeekProvider)cp, sources);
          break;
        case ProviderType.Select:
          result = VisitSelect((SelectProvider)cp, sources);
          break;
        case ProviderType.Skip:
          result = VisitSkip((SkipProvider)cp, sources);
          break;
        case ProviderType.Take:
          result = VisitTake((TakeProvider)cp, sources);
          break;
        case ProviderType.ExecutionSite:
          result = VisitExecutionSite((ExecutionSiteProvider)cp, sources);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return result;
    }

    /// <summary>
    /// Compiles <see cref="ExecutionSiteProvider"/>.
    /// </summary>
    /// <param name="provider">Execution site provider.</param>
    protected abstract ExecutableProvider VisitExecutionSite(ExecutionSiteProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="TakeProvider"/>.
    /// </summary>
    /// <param name="provider">Take provider.</param>
    protected abstract ExecutableProvider VisitTake(TakeProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="SkipProvider"/>.
    /// </summary>
    /// <param name="provider">Skip provider.</param>
    protected abstract ExecutableProvider VisitSkip(SkipProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="SelectProvider"/>.
    /// </summary>
    /// <param name="provider">Select provider.</param>
    protected abstract ExecutableProvider VisitSelect(SelectProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="SeekProvider"/>.
    /// </summary>
    /// <param name="provider">Seek provider.</param>
    protected abstract ExecutableProvider VisitSeek(SeekProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="RawProvider"/>.
    /// </summary>
    /// <param name="provider">Raw provider.</param>
    protected abstract ExecutableProvider VisitRaw(RawProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="RangeProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract ExecutableProvider VisitRange(RangeProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="SortProvider"/>.
    /// </summary>
    /// <param name="provider">Sort provider.</param>
    protected abstract ExecutableProvider VisitSort(SortProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="JoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract ExecutableProvider VisitJoin(JoinProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="FilterProvider"/>.
    /// </summary>
    /// <param name="provider">Filter provider.</param>
    protected abstract ExecutableProvider VisitFilter(FilterProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="DistinctProvider"/>.
    /// </summary>
    /// <param name="provider">Distinct provider.</param>
    protected abstract ExecutableProvider VisitDistinct(DistinctProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="CalculationProvider"/>.
    /// </summary>
    /// <param name="provider">Calculation provider.</param>
    protected abstract ExecutableProvider VisitCalculate(CalculationProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="AliasProvider"/>.
    /// </summary>
    /// <param name="provider">Alias provider.</param>
    protected abstract ExecutableProvider VisitAlias(AliasProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="AggregateProvider"/>.
    /// </summary>
    /// <param name="provider">Aggregate provider.</param>
    /// <returns></returns>
    protected abstract ExecutableProvider VisitAggregate(AggregateProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="StoredProvider"/>.
    /// </summary>
    /// <param name="provider">Store provider.</param>
    protected abstract ExecutableProvider VisitStore(StoredProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="IndexProvider"/>.
    /// </summary>
    /// <param name="provider">Index provider.</param>
    protected abstract ExecutableProvider VisitIndex(IndexProvider provider, ExecutableProvider[] sources);

    /// <summary>
    /// Compiles <see cref="ReindexProvider"/>.
    /// </summary>
    /// <param name="provider">Reindex provider.</param>
    /// <returns></returns>
    protected abstract ExecutableProvider VisitReindex(ReindexProvider provider, ExecutableProvider[] sources);


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Compiler(UrlInfo location)
    {
      Location = location;
    }
  }
}