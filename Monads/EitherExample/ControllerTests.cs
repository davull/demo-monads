using Xunit;
using Xunit.Abstractions;

namespace Monads.EitherExample;

public class ControllerTests
{
    private readonly ITestOutputHelper _output;

    public ControllerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("foo")]
    [InlineData("001")]
    public void InvalidInputShouldReturnZero1(string id)
    {
        var actual = Controller.GetReport1(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Error", actual);
    }

    [Theory]
    [InlineData("002")]
    [InlineData("003")]
    public void ValidInputShouldReturnOne1(string id)
    {
        var actual = Controller.GetReport1(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Report", actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("foo")]
    [InlineData("001")]
    public void InvalidInputShouldReturnZero2(string id)
    {
        var actual = Controller.GetReport2(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Error", actual);
    }

    [Theory]
    [InlineData("002")]
    [InlineData("003")]
    public void ValidInputShouldReturnOne2(string id)
    {
        var actual = Controller.GetReport2(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Report", actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("foo")]
    [InlineData("001")]
    public void InvalidInputShouldReturnZero3(string id)
    {
        var actual = Controller.GetReport3(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Error", actual);
    }

    [Theory]
    [InlineData("002")]
    [InlineData("003")]
    public void ValidInputShouldReturnOne3(string id)
    {
        var actual = Controller.GetReport3(id);
        _output.WriteLine(actual);

        Assert.StartsWith("Report", actual);
    }
}