namespace Monads.EitherExample;

public record Report(
    string Id,
    string Name,
    IReadOnlyCollection<int> Numbers,
    double Average);


public record Error(string Message);