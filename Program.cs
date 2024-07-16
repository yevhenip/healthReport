// Print health report for past 14 days for each particular day, use IDateTimeProvider to get current date
// Format: {ServiceName} {Date} {Uptime} {UptimePercent} {UnhealthyPercent} {DegradedPercent}
// Consider health data could be unavailable, for example monitoring started 1 day ago,
// in that case display Unavailable for periods preceding

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testing.HealthReport;

IDateTimeProvider dateProvider = new DateTimeProvider();
var healthData = new List<HealthDataItem>
{
    new("Service1", DateTimeOffset.Parse("2023-07-01 05:50:34 +03:00"), HealthStatus.Healthy),
    new("Service1", DateTimeOffset.Parse("2023-07-02 05:50:34 +03:00"), HealthStatus.Unhealthy),
    new("Service1", DateTimeOffset.Parse("2023-07-09 05:50:34 +03:00"), HealthStatus.Healthy),
    new("Service1", DateTimeOffset.Parse("2023-07-10 03:50:34 +03:00"), HealthStatus.Degraded),
    new("Service1", DateTimeOffset.Parse("2023-07-10 03:55:04 +03:00"), HealthStatus.Healthy),
    new("Service1", DateTimeOffset.Parse("2023-07-11 03:55:04 +03:00"), HealthStatus.Unhealthy),
    new("Service1", DateTimeOffset.Parse("2023-07-11 04:15:04 +03:00"), HealthStatus.Healthy)
};

Console.WriteLine(dateProvider.OffsetNow);
Console.WriteLine("Hello, World!");

const string unavailable = "Unavailable";

var groupedHealthData = healthData
    .GroupBy(x => new { x.Date.Year, x.Date.Month, x.Date.Day })
    .ToList();

foreach (var dataItems in groupedHealthData)
{
    var dataItemsList = dataItems.ToList();
    var dateTime = dataItemsList[0].Date;

    if (dateTime < dateProvider.OffsetNow.AddDays(-14))
        continue;

    var count = dataItemsList.Count;
    var degradedCount = dataItemsList.Count(x => x.Status == HealthStatus.Degraded);
    var unhealthyCount = dataItemsList.Count(x => x.Status == HealthStatus.Unhealthy);

    var minDate = dataItemsList.Min(x => x.Date);
    var maxDate = dataItemsList.Max(x => x.Date);
    var timeDifference = maxDate - minDate;
    var uptimePercentage = timeDifference.TotalHours / TimeSpan.FromDays(1).TotalHours * 100;
    uptimePercentage = uptimePercentage == 0 ? 100 : uptimePercentage;

    var degradedPercentage = unavailable;
    var unhealthyPercentage = unavailable;

    if (dateTime.DateTime.Date != dateProvider.OffsetNow.DateTime.Date)
    {
        degradedPercentage = $"{(double)degradedCount / count * 100:F2}%";
        unhealthyPercentage = $"{(double)unhealthyCount / count * 100:F2}%";
    }

    Console.WriteLine(
        $"{dataItemsList[0].Service} {dateTime.DateTime:yyyy-M-d} {timeDifference} {uptimePercentage:F2}% {unhealthyPercentage} {degradedPercentage}");
}