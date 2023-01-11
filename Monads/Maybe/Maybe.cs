using FsCheck.Xunit;
using Xunit;

namespace Monads.Maybe;

public sealed class Maybe<T>
{
    public T Item { get; }

    public bool HasItem { get; }

    private Maybe(bool hasItem, T item)
    {
        HasItem = hasItem;
        Item = item;
    }

    public static Maybe<T> Some(T item)
    {
        if (item is null)
            throw new ArgumentNullException(
                message: "Item has to be some value",
                paramName: nameof(item));

        return new Maybe<T>(hasItem: true, item: item);
    }

    public static Maybe<T> None()
    {
        return new Maybe<T>(hasItem: false, item: default!);
    }


    public Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        return HasItem
            ? Maybe<TResult>.Some(selector(Item))
            : Maybe<TResult>.None();
    }

    public void Match(Action<T> some, Action none)
    {
        if (some is null)
            throw new ArgumentNullException(nameof(some));

        if (none is null)
            throw new ArgumentNullException(nameof(none));

        if (HasItem)
            some(Item);
        else
            none();
    }

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        if (some is null)
            throw new ArgumentNullException(nameof(some));

        if (none is null)
            throw new ArgumentNullException(nameof(none));

        return HasItem
            ? some(Item)
            : none();
    }

    public override string ToString()
    {
        return Match(
            some: item => $"Some: {item}",
            none: () => "None");
    }

    public override int GetHashCode()
    {
        return Match(
            some: item => item!.GetHashCode(),
            none: () => 0);
    }

    public override bool Equals(object? obj) => obj is Maybe<T> that && Equals(Item, that.Item);
}

public static class MaybeExtensions
{
    public static Maybe<TResult> Bind<T, TResult>(
        this Maybe<T> source,
        Func<T, Maybe<TResult>> selector)
    {
        return source.HasItem
            ? selector(source.Item)
            : Maybe<TResult>.None();
    }

    public static Maybe<TResult> SelectMany<T, U, TResult>(
        this Maybe<T> source,
        Func<T, Maybe<U>> k,
        Func<T, U, TResult> s)
    {
        return source.Bind(x => k(x).Select(y => s(x, y)));
    }
}

public class MaybeTests
{
    [Fact]
    public void Some_WhenItemIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Maybe<string>.Some(null!));
    }

    [Property]
    public void Select_WhenSome(int v)
    {
        var maybe = Maybe<int>.Some(v);

        var result = maybe.Select(x => x * 2);

        Assert.Equal(result.Item, v * 2);
    }

    [Fact]
    public void Select_WhenNone()
    {
        var maybe = Maybe<int>.None();

        var result = maybe.Select(x => x * 2);

        Assert.False(result.HasItem);
    }

    [Fact]
    public void Bind_WhenNone()
    {
        var maybe = Maybe<double>.None();

        var result = maybe.Bind(Sqrt);

        Assert.False(result.HasItem);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    public void Bind_WhenSome(double d)
    {
        var maybe = Maybe<double>.Some(d);

        var result = maybe.Bind(Sqrt);

        Assert.True(result.HasItem);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(-3)]
    public void Bind_WhenSomeAndResultIsNone(double d)
    {
        var maybe = Maybe<double>.Some(d);

        var result = maybe.Bind(Sqrt);

        Assert.False(result.HasItem);
    }

    [Property]
    public void Match_WhenSome(int v)
    {
        var maybe = Maybe<int>.Some(v);

        maybe.Match(
            some: x => Assert.Equal(x, v),
            none: () => Assert.True(false));

        Assert.Equal(
            v,
            maybe.Match(x => x, () => 0));
    }

    [Fact]
    public void Match_WhenNone()
    {
        var maybe = Maybe<int>.None();

        maybe.Match(
            some: _ => Assert.True(false),
            none: () => Assert.True(true));

        Assert.Equal(
            0,
            maybe.Match(x => x, () => 0));
    }

    [Property]
    public void SelectWithQuerySyntax(int v)
    {
        var maybe = v % 2 == 0
            ? Maybe<int>.Some(v)
            : Maybe<int>.None();

        var result = from i in maybe
                     select i;

        Assert.Equal(maybe, result);
    }

    [Property]
    public void BindWithQuerySyntax(int v)
    {
        var maybe = v % 2 == 0
            ? Maybe<int>.Some(v)
            : Maybe<int>.None();

        _ = from i in maybe
            from s in Sqrt(i)
            select s;
    }

    private static Maybe<double> Sqrt(double d)
    {
        var result = Math.Sqrt(d);
        switch (result)
        {
            case double.NaN:
            case double.PositiveInfinity: return Maybe<double>.None();
            default: return Maybe<double>.Some(result);
        }
    }
}