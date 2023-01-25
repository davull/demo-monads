namespace Monads.EitherExample;

public static class Services
{
    public static Either<Error, string> GetReportName(string id)
    {
        var dict = new Dictionary<string, string>
        {
            { "001", "report-001" },
            { "002", "report-002" },
            { "003", "report-003" }
        };

        if (string.IsNullOrWhiteSpace(id))
            return new Left<Error, string>(new Error("Invalid report id"));

        if (!dict.TryGetValue(id, out var reportName))
            return new Left<Error, string>(new Error("Report id not found"));

        return new Right<Error, string>(reportName);
    }

    public static Either<Error, IEnumerable<int>> GetNumbers(string reportName)
    {
        var dict = new Dictionary<string, IEnumerable<int>>
        {
            { "report-001", Array.Empty<int>() },
            { "report-002", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } },
            { "report-003", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } }
        };

        var numbers = dict[reportName];
        if (!numbers.Any())
            return new Left<Error, IEnumerable<int>>(new Error("Report has no numbers"));

        return new Right<Error, IEnumerable<int>>(numbers);
    }

    public static Either<Error, Report> GetReport(IEnumerable<int> numbers)
    {
        var report = new Report(
            Id: "",
            Name: "",
            Numbers: numbers.ToArray(),
            Average: numbers.Average());
        return new Right<Error, Report>(report);
    }

    public static Either<Error, Report> GetReport(string id, string name, IEnumerable<int> numbers)
    {
        var report = new Report(
            Id: id,
            Name: name,
            Numbers: numbers.ToArray(),
            Average: numbers.Average());
        return new Right<Error, Report>(report);
    }
}