using FsCheck.Xunit;
using Xunit;

namespace Basics;

// "A Monoid is a Semigroup with an identity element."
//
// A monoid is a set with an associative binary operation and an identity element,
// e.g. the integers under addition with zero as an identity element.
//  a + (b + c) = (a + b) + c = a + b + c
//  a + 0 = 0 + a = a
//
// https://en.wikipedia.org/wiki/Monoid
// https://blog.ploeh.dk/2017/10/06/monoids/

public record MyMonoidType1(int Value)
{
    // Monoid defined over integer and addition

    public static MyMonoidType1 Identity { get; } = new(0);

    public static MyMonoidType1 AssociativeBinaryOperation(MyMonoidType1 o1, MyMonoidType1 o2)
        => new(o1.Value + o2.Value);
}

public record MyMonoidType2(bool Value)
{
    // Monoid defined over boolean and any().

    public static MyMonoidType2 Identity { get; } = new(false);

    public static MyMonoidType2 AssociativeBinaryOperation(MyMonoidType2 o1, MyMonoidType2 o2)
        => new(o1.Value || o2.Value);
}


public class MonoidTests
{
    // Test associativity
    [Property]
    public void TestAssociativityInt(int v1, int v2, int v3)
    {
        // Arrange
        var m1 = new MyMonoidType1(v1);
        var m2 = new MyMonoidType1(v2);
        var m3 = new MyMonoidType1(v3);
        var op = MyMonoidType1.AssociativeBinaryOperation;

        // Act
        var areEqual = op(op(m1, m2), m3) == op(m1, op(m2, m3));

        // Assert
        Assert.True(areEqual);
    }

    // Test identity
    [Property]
    public void TestIdentityInt(int v)
    {
        // Arrange
        var m = new MyMonoidType1(v);
        var identity = MyMonoidType1.Identity;
        var op = MyMonoidType1.AssociativeBinaryOperation;

        // Act
        var areEqual = op(m, identity) == op(identity, m) &&
                       op(m, identity) == m;

        // Assert
        Assert.True(areEqual);
    }


    // Test associativity
    [Property]
    public void TestAssociativityBool(bool v1, bool v2, bool v3)
    {
        // Arrange
        var m1 = new MyMonoidType2(v1);
        var m2 = new MyMonoidType2(v2);
        var m3 = new MyMonoidType2(v3);
        var op = MyMonoidType2.AssociativeBinaryOperation;

        // Act
        var areEqual = op(op(m1, m2), m3) == op(m1, op(m2, m3));

        // Assert
        Assert.True(areEqual);
    }

    // Test identity
    [Property]
    public void TestIdentityBool(bool v)
    {
        // Arrange
        var m = new MyMonoidType2(v);
        var identity = MyMonoidType2.Identity;
        var op = MyMonoidType2.AssociativeBinaryOperation;

        // Act
        var areEqual = op(m, identity) == op(identity, m) &&
                       op(m, identity) == m;

        // Assert
        Assert.True(areEqual);
    }
}