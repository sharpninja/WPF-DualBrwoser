using System.Collections;

namespace StudyBrowser.Views;

internal class DictionaryEntryEquatable : IEqualityComparer<DictionaryEntry>
{
    public bool Equals(DictionaryEntry x, DictionaryEntry y)
    {
        return x.Key.Equals(y.Key);
    }

    public int GetHashCode(DictionaryEntry obj)
    {
        return obj.Key.GetHashCode();
    }
}