using Xunit;

namespace Monads.Buildin;

// https://blog.ploeh.dk/2022/04/19/the-list-monad/

public class IEnumerableMonadTests
{
    // First monad law: Left identity
    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData("foo,bar")]
    [InlineData("foo,bar,baz")]
    public void TestFirstMonadLaw(string a)
    {
        Func<string, IEnumerable<string>> @return = x => new[] { x };
        Func<string, IEnumerable<string>> h = s => s.Split(',');

        Assert.Equal(
            @return(a).SelectMany(h),
            h(a));
    }

    // Second monad law: Right identity
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    public void TestSecondMonadLaw(int a)
    {
        Func<string, IEnumerable<string>> @return = x => new[] { x };
        Func<int, IEnumerable<string>> f = i => Enumerable.Repeat("foo", i);

        IEnumerable<string> m = f(a);

        Assert.Equal(
            m.SelectMany(@return),
            m);
    }

    // Third monad law: Associativity
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(8)]
    public void TestThirdMonadLaw(int a)
    {
        Func<int, IEnumerable<string>> f = i => Enumerable.Repeat("foo", i);
        Func<string, IEnumerable<char>> g = s => s;
        Func<char, IEnumerable<string>> h = c => new[]
        {
            c.ToString().ToUpper(),
            c.ToString().ToLower(),
        };

        var m = f(a);

        Assert.Equal(
            m.SelectMany(g).SelectMany(h),
            m.SelectMany(x => g(x).SelectMany(h)));
    }
}