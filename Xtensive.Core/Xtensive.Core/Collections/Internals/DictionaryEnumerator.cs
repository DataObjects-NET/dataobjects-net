using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Xtensive.Core.Conversion;

namespace Xtensive.Core.Collections
{
  internal class DictionaryEnumerator<TKey, TValue> : 
    ConvertingEnumerator<KeyValuePair<TKey,TValue>, object>,
    IDictionaryEnumerator
  {
    public DictionaryEntry Entry
    {
      get { return new DictionaryEntry(InnerCurrent.Key, InnerCurrent.Value);}
    }

    public object Key
    {
      get { return InnerCurrent.Key; }
    }

    public object Value
    {
      get { return InnerCurrent.Value; }
    }


    // Constructors

    public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> innerEnumerator, 
      System.Converter<KeyValuePair<TKey, TValue>, object> converter)
      : base(innerEnumerator, converter)
    {
    }
  }
}
