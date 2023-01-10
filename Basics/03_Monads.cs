using FsCheck.Xunit;
using Xunit;

namespace Basics;

// "A monad is a functor you can flatten."
//
// Monads are subtypes of functors with two additional methods: One to wrap
// a value in the monad type (Return) and one to flatten the monad (Bind).
// A Monad must follow the three monad laws:
//  - Left identity
//  - Right identity
//  - Associativity
//
// https://blog.ploeh.dk/2022/03/28/monads/
// https://blog.ploeh.dk/2022/04/04/kleisli-composition/
// https://en.wikipedia.org/wiki/Monad_(functional_programming)

public record MyMonadType<T>
{
    public T Value { get; }

    private MyMonadType(T value)
    {
        Value = value;
    }

    // Functor methods
    public MyMonadType<TResult> Map<TResult>(Func<T, TResult> map) => new(map(Value));

    public MyMonadType<TResult> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);

    // Monad method
    public static MyMonadType<T> Return(T value) => new(value);

    public override string ToString() => $"Value: {Value}";
}

public static class MonadicExtensions
{
    // Flatten aka join
    public static MyMonadType<T> Flatten<T>(this MyMonadType<MyMonadType<T>> source) =>
        MyMonadType<T>.Return(source.Value.Value);

    public static MyMonadType<TResult> Bind<T, TResult>(
        this MyMonadType<T> source,
        Func<T, MyMonadType<TResult>> selector)
    {
        // Monad<Monad<TResult>>
        var nested = source.Map(selector);
        return nested.Flatten();
    }

    // In C# the Bind()-Method is called SelectMany()
    public static MyMonadType<TResult> SelectMany<T, TResult>(
        this MyMonadType<T> source,
        Func<T, MyMonadType<TResult>> selector)
    {
        return source.Bind(selector);
    }

    // This function signature is required for using LINQ select
    // query syntax with multiple select statements.
    public static MyMonadType<TResult> SelectMany<T, U, TResult>(
        this MyMonadType<T> source,
        Func<T, MyMonadType<U>> k,
        Func<T, U, TResult> s)
    {
        Func<T, MyMonadType<TResult>> selector = x => k(x).Select(y => s(x, y));
        return source.SelectMany(selector);
    }

    // Kleisli composition
    public static Func<T, MyMonadType<T2>> Compose<T, T1, T2>(
        this Func<T, MyMonadType<T1>> action1,
        Func<T1, MyMonadType<T2>> action2)
    {
        return x => action1(x).Bind(action2);
    }
}

public class MonadTests
{
    // First monad law: Left identity
    [Property]
    public void TestFirstMonadLaw(int v)
    {
        // Return() acts as the identity function for monads

        // Arrange
        Func<int, MyMonadType<int>> @return = MyMonadType<int>.Return;
        Func<int, MyMonadType<string>> g = i => MyMonadType<string>.Return($"{i}");

        // Act
        Func<int, MyMonadType<string>> composed = @return.Compose(g);
        var areEqual = composed(v) == g(v);

        // Assert
        Assert.True(areEqual);
    }

    // Second monad law: Right identity
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("lorem ipsum")]
    public void TestSecondMonadLaw(string v)
    {
        // Return() acts as the identity function for monads

        // Arrange
        Func<string, MyMonadType<int>> g = s => MyMonadType<int>.Return(s.Length);
        Func<int, MyMonadType<int>> @return = MyMonadType<int>.Return;

        // Act
        Func<string, MyMonadType<int>> composed = g.Compose(@return);
        var areEqual = composed(v) == g(v);

        // Assert
        Assert.True(areEqual);
    }

    // Third monad law: Associativity
    [Property]
    public void TestThirdMonadLaw(double v)
    {
        // Arrange

        // F: DOUBLE -> BOOL
        Func<double, MyMonadType<bool>> f = d => MyMonadType<bool>.Return(d % 2 == 0);
        // G: BOOL -> STRING
        Func<bool, MyMonadType<string>> g = b => MyMonadType<string>.Return($"{b}");
        // H: STRING -> INT
        Func<string, MyMonadType<int>> h = s => MyMonadType<int>.Return(s.Length);

        Func<double, MyMonadType<int>> left = (f.Compose(g)).Compose(h);
        Func<double, MyMonadType<int>> right = f.Compose(g.Compose(h));

        // Act
        var areEqual = left(v) == right(v);

        // Assert
        Assert.True(areEqual);
    }

    [Property]
    public void TestMonadicBind(int v)
    {
        // Arrange
        var monad = MyMonadType<int>.Return(v);

        // Act
        var bind = monad.Bind(Triple);
        var areEqual = bind.Value == v * 3;

        // Assert
        Assert.True(areEqual);
    }

    [Property]
    public void BindFunctionInQuerySyntax(int v1, int v2)
    {
        // Arrange
        var monad1 = MyMonadType<int>.Return(v1);
        var monad2 = MyMonadType<int>.Return(v2);

        // Act
        var result = from m1 in monad1
                     from m2 in monad2
                     select m1 * m2;

        // Assert
        Assert.Equal(v1 * v2, result.Value);
    }

    private static MyMonadType<int> Triple(int x) => MyMonadType<int>.Return(x * 3);
}