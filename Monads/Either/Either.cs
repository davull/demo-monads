using System.Diagnostics;
using Xunit;

namespace Monads;

// "Either is also known as Result, in which case right is usually called
// success or OK, while left is called error or failure."
//
// https://blog.ploeh.dk/2022/05/09/an-either-monad/

public abstract record Either<L, R>
{
    public abstract T Match<T>(
        Func<L, T> onLeft,
        Func<R, T> onRight);
}

[DebuggerDisplay("{{_left}}")]
public record Left<L, R> : Either<L, R>
{
    private readonly L _left;

    public Left(L left)
    {
        _left = left;
    }

    public override T Match<T>(Func<L, T> onLeft, Func<R, T> onRight) =>
        onLeft(_left);
}

[DebuggerDisplay("{{_right}}")]
public record Right<L, R> : Either<L, R>
{
    private readonly R _right;

    public Right(R right)
    {
        _right = right;
    }

    public override T Match<T>(Func<L, T> onLeft, Func<R, T> onRight) =>
        onRight(_right);
}

public static class EitherExtensions
{
    // aka Map()
    public static Either<L1, R1> SelectBoth<L, L1, R, R1>(
        this Either<L, R> source,
        Func<L, L1> leftSelector,
        Func<R, R1> rightSelector)
    {
        return source.Match<Either<L1, R1>>(
            onLeft: l => new Left<L1, R1>(leftSelector(l)),
            onRight: r => new Right<L1, R1>(rightSelector(r)));
    }

    // aka MapLeft()
    public static Either<L1, R> SelectLeft<L, L1, R>(
        this Either<L, R> source,
        Func<L, L1> selector)
    {
        return source.SelectBoth(
            leftSelector: selector,
            rightSelector: r => r);
    }

    // aka MapRight()
    public static Either<L, R1> SelectRight<L, R, R1>(
        this Either<L, R> source,
        Func<R, R1> selector)
    {
        return source.SelectBoth(
            leftSelector: l => l,
            rightSelector: selector);
    }

    public static Either<L, R1> Select<L, R, R1>(
        this Either<L, R> source,
        Func<R, R1> selector)
    {
        return source.SelectRight(selector);
    }

    public static Either<L, R> Join<L, R>(
        this Either<L, Either<L, R>> source)
    {
        // Flatten the nested right side of the Either monad.
        return source.Match(
            onLeft: l => new Left<L, R>(l),
            onRight: r => r);
    }

    // aka SelectMany()
    public static Either<L, R1> Bind<L, R, R1>(
        this Either<L, R> source,
        Func<R, Either<L, R1>> selector)
    {
        return source
            .Select(selector)
            .Join();
    }

    public static Either<L, R1> SelectMany<L, R, R1>(
        this Either<L, R> source,
        Func<R, Either<L, R1>> selector) => Bind(source, selector);

    public static Either<L, R1> SelectMany<L, R, R1, U>(
        this Either<L, R> source,
        Func<R, Either<L, U>> k,
        Func<R, U, R1> s)
    {
        return source.SelectMany(x => k(x).Select(y => s(x, y)));
    }
}

public class EitherMonadLawsTests
{
    // First monad law: Left identity
    [Theory]
    [InlineData("2")]
    [InlineData("2.3:00")]
    [InlineData("4.5:30")]
    [InlineData("0:33:44")]
    [InlineData("foo")]
    public void TestLeftIdentity(string a)
    {
        Func<string, Either<string, string>> @return = s => new Right<string, string>(s);
        Func<string, Either<string, TimeSpan>> h = TryParseDuration;

        Assert.Equal(@return(a).SelectMany(h), h(a));
    }

    // Second monad law: Right identity
    [Theory]
    [InlineData("2022-03-22")]
    [InlineData("2022-03-21T16:57")]
    [InlineData("bar")]
    public void TestRightIdentity(string a)
    {
        Func<string, Either<string, DateTime>> f = TryParseDate;
        Func<DateTime, Either<string, DateTime>> @return = d => new Right<string, DateTime>(d);

        Either<string, DateTime> m = f(a);

        Assert.Equal(m.SelectMany(@return), m);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("-2.3:00")]
    [InlineData("4.5:30")]
    [InlineData("0:33:44")]
    [InlineData("0")]
    [InlineData("foo")]
    public void AssociativityLaw(string a)
    {
        Func<string, Either<string, TimeSpan>> f = TryParseDuration;
        Func<TimeSpan, Either<string, double>> g = DaysForward;
        Func<double, Either<string, int>> h = Nat;

        var m = f(a);

        Assert.Equal(m.SelectMany(g).SelectMany(h), m.SelectMany(x => g(x).SelectMany(h)));
    }

    private static Either<string, DateTime> TryParseDate(string candidate)
    {
        return DateTime.TryParse(candidate, out var d)
            ? new Right<string, DateTime>(d)
            : new Left<string, DateTime>(candidate);
    }

    private static Either<string, TimeSpan> TryParseDuration(string candidate)
    {
        return TimeSpan.TryParse(candidate, out var ts)
            ? new Right<string, TimeSpan>(ts)
            : new Left<string, TimeSpan>(candidate);
    }

    private static Either<string, double> DaysForward(TimeSpan ts)
    {
        if (ts < TimeSpan.Zero)
            return new Left<string, double>($"Negative durations not allowed: {ts}.");

        return new Right<string, double>(ts.TotalDays);
    }

    private static Either<string, int> Nat(double d)
    {
        if (d % 1 != 0)
            return new Left<string, int>($"Non-integers not allowed: {d}.");
        if (d < 1)
            return new Left<string, int>($"Non-positive numbers not allowed: {d}.");

        return new Right<string, int>((int)d);
    }
}