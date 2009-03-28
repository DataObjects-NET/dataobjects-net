using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers.
  /// Compiles <see cref="CompilableProvider"/>s int <see cref="ExecutableProvider"/>.
  /// </summary>
  public abstract class Compiler : ICompiler
  {
    /// <summary>
    /// Gets the method that returns compiled <see cref="ExecutableProvider"/> by its <see cref="CompilableProvider"/>.
    /// </summary>
    protected static Func<object, ExecutableProvider> GetBound {
      get {
        return CompilationContext.Current.BindingContext.GetBound;
      }
    }

    /// <summary>
    /// Gets execution site location.
    /// </summary>
    public virtual UrlInfo Location { get; private set; }

    /// <inheritdoc/>
    public abstract bool IsCompatible(ExecutableProvider provider);

    /// <inheritdoc/>
    public abstract ExecutableProvider ToCompatible(ExecutableProvider provider);


    /// <summary>
    /// Compiles the specified <see cref="CompilableProvider"/>.
    /// </summary>
    /// <param name="cp">The provider to compile.</param>
    public virtual ExecutableProvider Compile (CompilableProvider cp)
    {
      if (cp == null)
        return null;
      ExecutableProvider result;
      ProviderType providerType = cp.Type;
      switch (providerType) {
        case ProviderType.Index:
          result = VisitIndex((IndexProvider)cp);
          break;
        case ProviderType.Reindex:
          result = VisitReindex((ReindexProvider)cp);
          break;
        case ProviderType.Store:
          result = VisitStore((StoreProvider)cp);
          break;
        case ProviderType.Aggregate:
          result = VisitAggregate((AggregateProvider)cp);
          break;
        case ProviderType.Alias:
          result = VisitAlias((AliasProvider)cp);
          break;
        case ProviderType.Calculate:
          result = VisitCalculate((CalculateProvider)cp);
          break;
        case ProviderType.Distinct:
          result = VisitDistinct((DistinctProvider)cp);
          break;
        case ProviderType.Filter:
          result = VisitFilter((FilterProvider)cp);
          break;
        case ProviderType.Join:
          result = VisitJoin((JoinProvider)cp);
          break;
        case ProviderType.PredicateJoin:
          result = VisitPredicateJoin((PredicateJoinProvider)cp);
          break;
        case ProviderType.Sort:
          result = VisitSort((SortProvider)cp);
          break;
        case ProviderType.Range:
          result = VisitRange((RangeProvider)cp);
          break;
        case ProviderType.RangeSet:
          result = VisitRangeSet((RangeSetProvider)cp);
          break;
        case ProviderType.Raw:
          result = VisitRaw((RawProvider)cp);
          break;
        case ProviderType.Seek:
          result = VisitSeek((SeekProvider)cp);
          break;
        case ProviderType.Select:
          result = VisitSelect((SelectProvider)cp);
          break;
        case ProviderType.Skip:
          result = VisitSkip((SkipProvider)cp);
          break;
        case ProviderType.Take:
          result = VisitTake((TakeProvider)cp);
          break;
        case ProviderType.Transfer:
          result = VisitTransfer((TransferProvider)cp);
          break;
        case ProviderType.Apply:
          result = VisitApply((ApplyProvider)cp);
          break;
        case ProviderType.RowNumber:
          result = VisitRowNumber((RowNumberProvider)cp);
          break;
        case ProviderType.Existence:
          result = VisitExistence((ExistenceProvider)cp);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      result.Location = Location;
      return IsCompatible(result) ? result : ToCompatible(result);
    }

    /// <summary>
    /// Compiles <see cref="TransferProvider"/>.
    /// </summary>
    /// <param name="provider">Execution site provider.</param>
    protected abstract ExecutableProvider VisitTransfer(TransferProvider provider);

    /// <summary>
    /// Compiles <see cref="TakeProvider"/>.
    /// </summary>
    /// <param name="provider">Take provider.</param>
    protected abstract ExecutableProvider VisitTake(TakeProvider provider);

    /// <summary>
    /// Compiles <see cref="SkipProvider"/>.
    /// </summary>
    /// <param name="provider">Skip provider.</param>
    protected abstract ExecutableProvider VisitSkip(SkipProvider provider);

    /// <summary>
    /// Compiles <see cref="SelectProvider"/>.
    /// </summary>
    /// <param name="provider">Select provider.</param>
    protected abstract ExecutableProvider VisitSelect(SelectProvider provider);

    /// <summary>
    /// Compiles <see cref="SeekProvider"/>.
    /// </summary>
    /// <param name="provider">Seek provider.</param>
    protected abstract ExecutableProvider VisitSeek(SeekProvider provider);

    /// <summary>
    /// Compiles <see cref="RawProvider"/>.
    /// </summary>
    /// <param name="provider">Raw provider.</param>
    protected abstract ExecutableProvider VisitRaw(RawProvider provider);

    /// <summary>
    /// Compiles <see cref="RangeProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract ExecutableProvider VisitRange(RangeProvider provider);

    /// <summary>
    /// Compiles <see cref="RangeSetProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract ExecutableProvider VisitRangeSet(RangeSetProvider provider);

    /// <summary>
    /// Compiles <see cref="SortProvider"/>.
    /// </summary>
    /// <param name="provider">Sort provider.</param>
    protected abstract ExecutableProvider VisitSort(SortProvider provider);

    /// <summary>
    /// Compiles <see cref="JoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract ExecutableProvider VisitJoin(JoinProvider provider);

    /// <summary>
    /// Compiles <see cref="PredicateJoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract ExecutableProvider VisitPredicateJoin(PredicateJoinProvider provider);
    
    /// <summary>
    /// Compiles <see cref="FilterProvider"/>.
    /// </summary>
    /// <param name="provider">Filter provider.</param>
    protected abstract ExecutableProvider VisitFilter(FilterProvider provider);

    /// <summary>
    /// Compiles <see cref="DistinctProvider"/>.
    /// </summary>
    /// <param name="provider">Distinct provider.</param>
    protected abstract ExecutableProvider VisitDistinct(DistinctProvider provider);

    /// <summary>
    /// Compiles <see cref="CalculateProvider"/>.
    /// </summary>
    /// <param name="provider">Calculate provider.</param>
    protected abstract ExecutableProvider VisitCalculate(CalculateProvider provider);

    /// <summary>
    /// Compiles <see cref="AliasProvider"/>.
    /// </summary>
    /// <param name="provider">Alias provider.</param>
    protected abstract ExecutableProvider VisitAlias(AliasProvider provider);

    /// <summary>
    /// Compiles <see cref="AggregateProvider"/>.
    /// </summary>
    /// <param name="provider">Aggregate provider.</param>
    /// <returns></returns>
    protected abstract ExecutableProvider VisitAggregate(AggregateProvider provider);

    /// <summary>
    /// Compiles <see cref="StoreProvider"/>.
    /// </summary>
    /// <param name="provider">Store provider.</param>
    protected abstract ExecutableProvider VisitStore(StoreProvider provider);

    /// <summary>
    /// Compiles <see cref="IndexProvider"/>.
    /// </summary>
    /// <param name="provider">Index provider.</param>
    protected abstract ExecutableProvider VisitIndex(IndexProvider provider);

    /// <summary>
    /// Compiles <see cref="ReindexProvider"/>.
    /// </summary>
    /// <param name="provider">Reindex provider.</param>
    /// <returns></returns>
    protected abstract ExecutableProvider VisitReindex(ReindexProvider provider);

    /// <summary>
    /// Compiles <see cref="ApplyProvider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected abstract ExecutableProvider VisitApply(ApplyProvider provider);


    /// <summary>
    /// Compiles <see cref="RowNumberProvider"/>.
    /// </summary>
    /// <param name="provider">Row number provider.</param>
    protected abstract ExecutableProvider VisitRowNumber(RowNumberProvider provider);

    /// <summary>
    /// Compiles <see cref="ExistenceProvider"/>.
    /// </summary>
    /// <param name="provider">Existence provider.</param>
    protected abstract ExecutableProvider VisitExistence(ExistenceProvider provider);


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