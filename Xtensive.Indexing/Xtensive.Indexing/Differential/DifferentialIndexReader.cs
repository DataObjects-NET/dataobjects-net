using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Differential
{
  public class DifferentialIndexReader<TKey, TItem, TImpl> : IIndexReader<TKey, TItem>
    where TImpl : IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
  {
    private readonly DifferentialIndex<TKey, TItem, TImpl> index;
    private Range<IEntire<TKey>> range;
    private readonly IIndexReader<TKey, TItem> originReader;
    private readonly IIndexReader<TKey, TItem> insertionsReader;
    private readonly IIndexReader<TKey, TItem> removalsReader;
    private IIndexReader<TKey, TItem> reader;
    private int flag = 1;
    private bool originFlag = true;
    private bool insertionFlag = true;
    private bool removalFlag = true;
    private bool isFirstMove = true;


    /// <summary>
    /// Gets the index.
    /// </summary>
    public IIndex<TKey, TItem> Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <summary>
    /// Gets the reader.
    /// </summary>
    public IIndexReader<TKey, TItem> Reader
    {
      [DebuggerStepThrough]
      get { return reader; }
    }

    /// <inheritdoc/>
    public Range<IEntire<TKey>> Range
    {
      [DebuggerStepThrough]
      get { return range; }
    }

    /// <inheritdoc/>
    public Direction Direction
    {
      [DebuggerStepThrough]
      get { return originReader.Direction; }
    }

    /// <inheritdoc/>
    public TItem Current
    {
      [DebuggerStepThrough]
      get { return reader.Current; }
    }

    /// <inheritdoc/>
    object IEnumerator.Current
    {
      [DebuggerStepThrough]
      get { return Current; }
    }


    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      return new DifferentialIndexReader<TKey, TItem, TImpl>(index, range);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
      //---Check if Index is Empty.-----
      if (isFirstMove) {
        if (index.Origin.Count==0) {
          originFlag = false;
          reader = insertionsReader;
          flag = 2;
        }
        if (index.Insertions.Count==0) {
          insertionFlag = false;
          reader = originReader;
          flag = 1;
        }
        if (!originFlag && !insertionFlag)
          return false;
        if (index.Removals.Count==0)
          removalFlag = false;
      }

      //---Try to move next.-----
      bool moveNextResult = reader.MoveNext();
      if (moveNextResult && MoveIsPossible())
        return true;

      if ((flag==1) && (insertionFlag)) {
        reader = insertionsReader;
        moveNextResult = MoveIsPossible();
      }
      if ((flag==2) && (originFlag)) {
        reader = originReader;
        insertionFlag = false;
        moveNextResult = MoveIsPossible();
      }
      return moveNextResult;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      MoveTo(range.EndPoints.First);
    }

    /// <inheritdoc/>
    public void MoveTo(IEntire<TKey> key)
    {
      if (index.Insertions.ContainsKey(key.Value)) {
        insertionsReader.MoveTo(key);
        reader = insertionsReader;
        flag = 2;
        if (index.Origin.Count != 0)
          originReader.MoveTo(Entire<TKey>.Create(index.Origin.GetFullRange().EndPoints.First.Value));
      }
      if ((index.Origin.ContainsKey(key.Value)) && (!index.Removals.ContainsKey(key.Value))) {
        originReader.MoveTo(key);
        reader = originReader;
        flag = 1;
        if (index.Insertions.Count != 0)
          insertionsReader.MoveTo(Entire<TKey>.Create(index.Insertions.GetFullRange().EndPoints.First.Value));
      }
      if (index.Removals.Count != 0)
        removalsReader.MoveTo(Entire<TKey>.Create(index.Removals.GetFullRange().EndPoints.First.Value));
      isFirstMove = true;
    }

    #region Private / Internal methods.

    private int IsGreater(TKey key1, TKey key2)
    {
      AdvancedComparer<TKey> comparer = AdvancedComparer<TKey>.Default;
      int result = comparer.Compare(key2, key1);
      if (Direction==Direction.Positive)
        return result;
      return -result;
    }

    private bool MoveIsPossible()
    {
      if (isFirstMove) {
        if ((flag == 1) && insertionFlag && !insertionsReader.MoveNext())
          insertionFlag = false;
        if ((flag == 2) && originFlag && !originReader.MoveNext())
          originFlag = false;
        if (removalFlag && !removalsReader.MoveNext())
          removalFlag = false;
        isFirstMove = false;
      }
      while (originFlag && removalFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))==0)) {
        if (originReader.MoveNext()) {
          reader = originReader;
          flag = 1;
        }
        else {
          originFlag = false;
          if (insertionFlag) {
            reader = insertionsReader;
            flag = 2;
            break;
          }
        }
        if (!removalsReader.MoveNext()) {
          removalFlag = false;
          break;
        }
      }
      if ((originFlag && insertionFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) > 0)) ||
        (originFlag && !insertionFlag)) {
        flag = 1;
        reader = originReader;
        return true;
      }

      if ((originFlag && insertionFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) < 0)) ||
        (insertionFlag && !originFlag)) {
        flag = 2;
        reader = insertionsReader;
        return true;
      }
      return false;
    }

    #endregion

    //Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DifferentialIndexReader&lt;TKey, TItem, TImpl&gt;"/> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range.</param>
    public DifferentialIndexReader(DifferentialIndex<TKey, TItem, TImpl> index, Range<IEntire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      originReader = index.Origin.CreateReader(range);
      reader = originReader;
      insertionsReader = index.Insertions.CreateReader(range);
      removalsReader = index.Removals.CreateReader(range);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      originReader.Dispose();
      insertionsReader.Dispose();
      removalsReader.Dispose();
      reader.Dispose();
    }
  }
}