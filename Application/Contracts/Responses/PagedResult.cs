namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class PagedResult<T>(IReadOnlyList<T> data, int limit, int offset, bool hasMore) : IReadOnlyList<T>
{
    public IReadOnlyList<T> Data { get; } = data;
    public int Limit { get; } = limit;
    public int Offset { get; } = offset;
    public bool HasMore { get; } = hasMore;
    public int? NextOffset { get; } = hasMore ? offset + limit : null;

    public int Count => Data.Count;
    public T this[int index] => Data[index];

    public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static PagedResult<T> FromRows(IReadOnlyList<T> rows, int limit, int offset)
    {
        var hasMore = rows.Count > limit;
        var data = hasMore ? rows.Take(limit).ToArray() : rows;
        return new PagedResult<T>(data, limit, offset, hasMore);
    }
}
