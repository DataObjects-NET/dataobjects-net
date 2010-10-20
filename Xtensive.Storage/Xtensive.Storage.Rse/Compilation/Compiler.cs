using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers that implements visitor pattern.
  /// Compiles <see cref="CompilableProvider"/>s into <see cref="ExecutableProvider"/>.
  /// </summary>
  public abstract class Compiler<TResult> : ICompiler<TResult> 
    where TResult : ExecutableProvider
  {
    private CompilableProvider rootProvider;

    protected CompilableProvider RootProvider { get { return rootProvider; } }

    /// <summary>
    /// Gets execution site location.
    /// </summary>
    public Location Location { get; private set; }

    /// <inheritdoc/>
    public BindingCollection<ApplyParameter, ExecutableProvider> OuterReferences { get; private set; }

    /// <inheritdoc/>
    public abstract bool IsCompatible(ExecutableProvider provider);

    /// <inheritdoc/>
    ExecutableProvider ICompiler.ToCompatible(ExecutableProvider provider)
    {
      return ToCompatible(provider);
    }

    /// <inheritdoc/>
    public abstract TResult ToCompatible(ExecutableProvider provider);

    /// <inheritdoc/>
    ExecutableProvider ICompiler.Compile(CompilableProvider provider)
    {
      return Compile(provider);
    }

    /// <summary>
    /// Compiles the specified <see cref="CompilableProvider"/>.
    /// </summary>
    /// <param name="cp">The provider to compile.</param>
    public TResult Compile (CompilableProvider cp)
    {
      if (cp == null)
        return null;
      if (rootProvider == null)
        rootProvider = cp;
      TResult result;
      switch (cp.Type) {
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
        case ProviderType.Intersect:
          result = VisitIntersect((IntersectProvider) cp);
          break;
        case ProviderType.Except:
          result = VisitExcept((ExceptProvider)cp);
          break;
        case ProviderType.Concat:
          result = VisitConcat((ConcatProvider) cp);
          break;
        case ProviderType.Union:
          result = VisitUnion((UnionProvider) cp);
          break;
        case ProviderType.Lock:
          result = VisitLock((LockProvider) cp);
          break;
        case ProviderType.Include:
          result = VisitInclude((IncludeProvider) cp);
          break;
        case ProviderType.FreeText:
          result = VisitFreeText((FreeTextProvider) cp);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      result.Location = Location;
      return result;
    }

    /// <summary>
    /// Compiles <see cref="TransferProvider"/>.
    /// </summary>
    /// <param name="provider">Execution site provider.</param>
    protected abstract TResult VisitTransfer(TransferProvider provider);

    /// <summary>
    /// Compiles <see cref="TakeProvider"/>.
    /// </summary>
    /// <param name="provider">Take provider.</param>
    protected abstract TResult VisitTake(TakeProvider provider);

    /// <summary>
    /// Compiles <see cref="SkipProvider"/>.
    /// </summary>
    /// <param name="provider">Skip provider.</param>
    protected abstract TResult VisitSkip(SkipProvider provider);

    /// <summary>
    /// Compiles <see cref="SelectProvider"/>.
    /// </summary>
    /// <param name="provider">Select provider.</param>
    protected abstract TResult VisitSelect(SelectProvider provider);

    /// <summary>
    /// Compiles <see cref="SeekProvider"/>.
    /// </summary>
    /// <param name="provider">Seek provider.</param>
    protected abstract TResult VisitSeek(SeekProvider provider);

    /// <summary>
    /// Compiles <see cref="RawProvider"/>.
    /// </summary>
    /// <param name="provider">Raw provider.</param>
    protected abstract TResult VisitRaw(RawProvider provider);

    /// <summary>
    /// Compiles <see cref="RangeProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract TResult VisitRange(RangeProvider provider);

    /// <summary>
    /// Compiles <see cref="RangeSetProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract TResult VisitRangeSet(RangeSetProvider provider);

    /// <summary>
    /// Compiles <see cref="SortProvider"/>.
    /// </summary>
    /// <param name="provider">Sort provider.</param>
    protected abstract TResult VisitSort(SortProvider provider);

    /// <summary>
    /// Compiles <see cref="JoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract TResult VisitJoin(JoinProvider provider);

    /// <summary>
    /// Compiles <see cref="PredicateJoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract TResult VisitPredicateJoin(PredicateJoinProvider provider);
    
    /// <summary>
    /// Compiles <see cref="FilterProvider"/>.
    /// </summary>
    /// <param name="provider">Filter provider.</param>
    protected abstract TResult VisitFilter(FilterProvider provider);

    /// <summary>
    /// Compiles <see cref="DistinctProvider"/>.
    /// </summary>
    /// <param name="provider">Distinct provider.</param>
    protected abstract TResult VisitDistinct(DistinctProvider provider);

    /// <summary>
    /// Compiles <see cref="CalculateProvider"/>.
    /// </summary>
    /// <param name="provider">Calculate provider.</param>
    protected abstract TResult VisitCalculate(CalculateProvider provider);

    /// <summary>
    /// Compiles <see cref="AliasProvider"/>.
    /// </summary>
    /// <param name="provider">Alias provider.</param>
    protected abstract TResult VisitAlias(AliasProvider provider);

    /// <summary>
    /// Compiles <see cref="AggregateProvider"/>.
    /// </summary>
    /// <param name="provider">Aggregate provider.</param>
    /// <returns></returns>
    protected abstract TResult VisitAggregate(AggregateProvider provider);

    /// <summary>
    /// Compiles <see cref="StoreProvider"/>.
    /// </summary>
    /// <param name="provider">Store provider.</param>
    protected abstract TResult VisitStore(StoreProvider provider);

    /// <summary>
    /// Compiles <see cref="IndexProvider"/>.
    /// </summary>
    /// <param name="provider">Index provider.</param>
    protected abstract TResult VisitIndex(IndexProvider provider);

    /// <summary>
    /// Compiles <see cref="ReindexProvider"/>.
    /// </summary>
    /// <param name="provider">Reindex provider.</param>
    /// <returns></returns>
    protected abstract TResult VisitReindex(ReindexProvider provider);

    /// <summary>
    /// Compiles <see cref="ApplyProvider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected abstract TResult VisitApply(ApplyProvider provider);


    /// <summary>
    /// Compiles <see cref="RowNumberProvider"/>.
    /// </summary>
    /// <param name="provider">Row number provider.</param>
    protected abstract TResult VisitRowNumber(RowNumberProvider provider);

    /// <summary>
    /// Compiles <see cref="ExistenceProvider"/>.
    /// </summary>
    /// <param name="provider">Existence provider.</param>
    protected abstract TResult VisitExistence(ExistenceProvider provider);

    /// <summary>
    /// Compiles <see cref="IntersectProvider"/>.
    /// </summary>
    /// <param name="provider">Intersect provider.</param>
    protected abstract TResult VisitIntersect(IntersectProvider provider);

    /// <summary>
    /// Compiles <see cref="ExceptProvider"/>.
    /// </summary>
    /// <param name="provider">Except provider.</param>
    protected abstract TResult VisitExcept(ExceptProvider provider);

    /// <summary>
    /// Compiles <see cref="ConcatProvider"/>.
    /// </summary>
    /// <param name="provider">Concat provider.</param>
    protected abstract TResult VisitConcat(ConcatProvider provider);

    /// <summary>
    /// Compiles <see cref="UnionProvider"/>.
    /// </summary>
    /// <param name="provider">Union provider.</param>
    protected abstract TResult VisitUnion(UnionProvider provider);

    /// <summary>
    /// Compiles <see cref="LockProvider"/>.
    /// </summary>
    /// <param name="provider">Lock provider.</param>
    protected abstract TResult VisitLock(LockProvider provider);

    /// <summary>
    /// Compiles <see cref="IncludeProvider"/>.
    /// </summary>
    /// <param name="provider">Include provider.</param>
    protected abstract TResult VisitInclude(IncludeProvider provider);

    /// <summary>
    /// Compiles <see cref="FreeTextProvider"/>.
    /// </summary>
    /// <param name="provider">FreeText provider.</param>
    protected abstract TResult VisitFreeText(FreeTextProvider provider);


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="location">Location.</param>
    protected Compiler(Location location)
    {
      Location = location;
      OuterReferences = new BindingCollection<ApplyParameter, ExecutableProvider>();
    }
  }
}
