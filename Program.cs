// Print health report for past 14 days for each particular day, use IDateTimeProvider to get current date
// Format: {ServiceName} {Date} {Uptime} {UptimePercent} {UnhealthyPercent} {DegradedPercent}
// Consider health data could be unavailable, for example monitoring started 1 day ago,
// in that case display Unavailable for periods preceding
/*
 * Report for past 14 days for Service1
Format: {ServiceName} {Date} {Uptime} {UptimePercent} {UnhealthyPercent} {DegradedPercent}
Service1 6/28/2023 Unavailable
Service1 6/29/2023 Unavailable
Service1 6/30/2023 Unavailable
Service1 7/1/2023 18:09:26 75.65% 0.00% 0.00%
Service1 7/2/2023 05:50:34 24.35% 75.65% 0.00%
Service1 7/3/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/4/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/5/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/6/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/7/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/8/2023 00:00:00 0.00% 100.00% 0.00%
Service1 7/9/2023 18:09:26 75.65% 24.35% 0.00%
Service1 7/10/2023 23:55:30 99.69% 0.00% 0.31%
Service1 7/11/2023 23:40:00 98.61% 1.39% 0.00%
Service1 7/12/2023 24:00:00 100.00% 0.00% 0.00%`
 */

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

const string unavailable = "Unavailable";
var healthyTime = TimeSpan.Zero;
var degradedTime = TimeSpan.Zero;
var unhealthyTime = TimeSpan.Zero;
var lastStatus = HealthStatus.Unhealthy;
var lastDateTime = DateTimeOffset.MinValue;
var currentDate = dateProvider.OffsetNow.AddDays(-14);
var totalSeconds = TimeSpan.FromDays(1).TotalSeconds;
healthData.Sort((prev, curr) => prev.Date.CompareTo(curr.Date));
var i = 0;

while (currentDate.Date <= dateProvider.OffsetNow)
{
    if (i == healthData.Count)
    {
        --i;
        ProcessState();
        ++i;
    }
    else if (currentDate.Date < healthData[i].Date.Date)
    {
        if (i == 0)
        {
            Console.WriteLine($"{healthData[i].Service} {currentDate.DateTime:MM/dd/yyyy} {unavailable}");
            currentDate = currentDate.AddDays(1);
            continue;
        }

        ProcessState();
    }
    else if (currentDate.Date == healthData[i].Date.Date)
    {
        if (i == 0)
        {
            lastStatus = healthData[i].Status;
            lastDateTime = healthData[i].Date;
            ++i;
            continue;
        }

        var diff = healthData[i].Date.Subtract(lastDateTime);
        AddTime(diff, lastStatus);

        lastStatus = healthData[i].Status;
        lastDateTime = healthData[i].Date;
        ++i;
    }
}

Console.WriteLine("Hello, World!");
return;

void AddTime(TimeSpan time, HealthStatus status)
{
    switch (status)
    {
        case HealthStatus.Unhealthy:
            unhealthyTime = unhealthyTime.Add(time);
            break;
        case HealthStatus.Degraded:
            degradedTime = degradedTime.Add(time);
            break;
        case HealthStatus.Healthy:
            healthyTime = healthyTime.Add(time);
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}

void ProcessState()
{
    var missedTime = lastDateTime.Date.AddDays(1).Subtract(lastDateTime.DateTime);
    AddTime(missedTime, lastStatus);

    Console.WriteLine(
        $"{healthData[i].Service} {currentDate.DateTime:MM/dd/yyyy} {healthyTime.ToString("")} " +
        $"{healthyTime.TotalSeconds / totalSeconds * 100:F}% " +
        $"{unhealthyTime.TotalSeconds / totalSeconds * 100:F}% " +
        $"{degradedTime.TotalSeconds / totalSeconds * 100:F}%");

    healthyTime = TimeSpan.Zero;
    degradedTime = TimeSpan.Zero;
    unhealthyTime = TimeSpan.Zero;
    lastDateTime = currentDate = currentDate.AddDays(1).Date;
}