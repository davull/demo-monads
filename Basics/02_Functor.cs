using FsCheck.Xunit;
using Xunit;

namespace Basics;

// "[..] a functor is a mapping between categories."
//
// A functor maps objects/types of one category to objects/types of another category, preserving
// the structure of the category. In other words, a functor is a mapping between categories that
// preserves the structure of the categories. A functor also maps functions between objects (morphisms).
//
// https://blog.ploeh.dk/2018/03/22/functors/
// https://bartoszmilewski.com/2015/01/20/functors/
// https://en.wikipedia.org/wiki/Functor

public record MyFunctorType1<T>(T Value)
{
    // Func<T, T> id = x => x;
    public static T Identity(T value) => value;

    public MyFunctorType1<TResult> Map<TResult>(Func<T, TResult> map)
        => new(map(Value));

    // This function signature is required for using LINQ select query syntax
    public MyFunctorType1<TResult> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);
}

public class FunctorTests
{
    // First functor law: Mapping the identity function returns the functor unchanged
    [Property]
    public void TestFirstFunctorLaw(int v)
    {
        // Arrange
        var functor = new MyFunctorType1<int>(v);
        var id = MyFunctorType1<int>.Identity;

        // Act
        var areEqual = functor.Map(id) == functor;

        // Assert
        Assert.True(areEqual);
    }

    // Second functor law: Mapping with two functions is the same as mapping
    // with the composition of those functions
    [Property]
    public void TestSecondFunctorLaw(int v)
    {
        // Arrange
        var functor = new MyFunctorType1<int>(v);

        // G = INT -> STRING
        Func<int, string> g = i => $"{i}";
        // F = STRING -> STRING
        Func<string, string> f = s => new string(s.Reverse().ToArray());

        // Act
        // (f . g)(x) = f(g(x))
        var areEqual = functor.Select(g).Select(f) ==
                       functor.Select(i => f(g(i)));

        // Assert
        Assert.True(areEqual);
    }

    [Property]
    public void MapFunctionInQuerySyntax(int v)
    {
        // Arrange
        var functor = new MyFunctorType1<int>(v);

        // Act
        var result = from f in functor
                     select f * 2;

        // Assert
        Assert.Equal(v * 2, result.Value);
    }
}