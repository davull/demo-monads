using System.Text.Json;

namespace Monads.EitherExample;

public static class Controller
{
    // public static int GetReport0(string id)
    // {
    //     var reportName = Services.GetReportName(id);
    //     if (string.IsNullOrEmpty(reportName))
    //         return 0;
    //
    //     var numbers = Services.GetNumbers(reportName);
    //     if (!numbers.Any())
    //         return 0;
    //
    //     var report = Services.GetReport(numbers);
    //     return report is null
    //         ? 0
    //         : 1;
    // }

    public static string GetReport1(string id)
    {
        var result = Services.GetReportName(id)
            .Bind(Services.GetNumbers)
            .Bind(Services.GetReport);

        return GetResponse(result);
    }

    public static string GetReport2(string id)
    {
        var result = from x in Services.GetReportName(id)
                     from y in Services.GetNumbers(x)
                     from z in Services.GetReport(y)
                     select z;

        return GetResponse(result);
    }

    public static string GetReport3(string id)
    {
        var result = from x in Services.GetReportName(id)
                     from y in Services.GetNumbers(x)
                     from z in Services.GetReport(id, x, y)
                     select z;

        return GetResponse(result);
    }

    private static string GetResponse(Either<Error, Report> result)
    {
        return result.Match(
            onLeft: error => $"Error: {error.Message}",
            onRight: report => $"Report: {JsonSerializer.Serialize(report)}");
    }
}