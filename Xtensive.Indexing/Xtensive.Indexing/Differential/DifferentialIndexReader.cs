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
      if (reader.MoveNext() && MoveIsPossible())
        return true;

      bool result;

      if (flag==1) {
        reader = insertionsReader;
        result = MoveIsPossible();
      }
      else {
        reader = originReader;
        insertionFlag = false;
        result = MoveIsPossible();
      }
      return result;
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
        originReader.MoveTo(Entire<TKey>.Create(index.Origin.GetFullRange().GetLowerEndpoint(Direction).Value));
      }
      if ((index.Origin.ContainsKey(key.Value)) && (!index.Removals.ContainsKey(key.Value))) {
          originReader.MoveTo(key);
          reader = originReader;
          flag = 1;
          insertionsReader.MoveTo(Entire<TKey>.Create(index.Insertions.GetFullRange().GetLowerEndpoint(Direction).Value));
      }
      removalsReader.MoveTo(Entire<TKey>.Create(index.Removals.GetFullRange().GetLowerEndpoint(Direction).Value));
      isFirstMove = true;
    }

    #region Private / Internal methods.

    private int IsGreater(TKey key1, TKey key2)
    {
      AdvancedComparer<TKey> comparer = AdvancedComparer<TKey>.Default;
      int result = comparer.Compare(key2, key1);
      if (Direction == Direction.Positive)
        return result;
      return -result;
    }

    private bool MoveIsPossible()
    {
      if (isFirstMove)
      {
        if (flag == 1)
          insertionsReader.MoveNext();
        if (flag == 2)
          originReader.MoveNext();
        removalsReader.MoveNext();
        isFirstMove = false;
      }
      while (originFlag && removalFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current)) == 0))
      {
        if (originReader.MoveNext())
        {
          reader = originReader;
          flag = 1;
        }
        else
        {
          originFlag = false;
          if (insertionFlag)
          {
            reader = insertionsReader;
            flag = 2;
            break;
          }
        }
        if (!removalsReader.MoveNext())
        {
          removalFlag = false;
          break;
        }
      }
      if ((originFlag && insertionFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) > 0)) ||
         (originFlag && !insertionFlag))
      {
        flag = 1;
        reader = originReader;
        return true;

      }

      if ((originFlag && insertionFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) < 0)) ||
        (insertionFlag && !originFlag))
      {
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
    }

  }
}