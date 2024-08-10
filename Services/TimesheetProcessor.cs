using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TimesheetValidationApi.Models;



public class ProcessedData
{
    public string Date { get; set; }
    public int MinutesWorked { get; set; }
    public string HoursWorked { get; set; }
    public int Entries { get; set; }
    public string Flags { get; set; }
    public List<TimesheetEntry> ExpandedDetails { get; set; }
}


public class TimesheetProcessor
{


    public List<ProcessedData> Summarize(string[][] data)
    {
        var dataEntries = ParseCSVData(data);
        var summaryMap = new Dictionary<string, ProcessedData>();

        foreach (var entry in dataEntries)
        {
            string date = entry.StartDateTime.ToString("yyyy-MM-dd");

            if (!summaryMap.ContainsKey(date))
            {
                summaryMap[date] = new ProcessedData
                {
                    Date = entry.StartDateTime.ToString("dd-MM-yyyy"),
                    MinutesWorked = 0,
                    Entries = 0,
                    Flags = string.Empty,
                    ExpandedDetails = new List<TimesheetEntry>()
                };

            }

            var summary = summaryMap[date];
            summary.MinutesWorked += entry.Duration;
            summary.Entries += 1;
            summary.ExpandedDetails.Add(entry);
        }

        var summaryData = new List<ProcessedData>(summaryMap.Values);

        foreach (var summary in summaryData)
        {
            summary.HoursWorked = FormatDuration(summary.MinutesWorked);
            summary.Flags = GetFlags(summary);
        }

        return summaryData;
    }

    private List<TimesheetEntry> ParseCSVData(string[][] data)
    {
        var dataEntries = new List<TimesheetEntry>();

        // Sort the entries chronologically by the Start time
        dataEntries = dataEntries.OrderBy(e => e.StartDateTime).ToList();

        foreach (var row in data)
        {
            if (row.Length >= 2)
            {
                string description = row[0];
                DateTime start = DateTime.ParseExact(row[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime end = DateTime.ParseExact(row[2], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                dataEntries.Add(new TimesheetEntry
                {
                    Description = description,
                    StartDateTime = start,
                    EndDateTime = end,
                    TaskFlags = ""
                });
            }
        }

        // Detect overlaps between taks and set flags
        for (int i = 0; i < dataEntries.Count - 1; i++)
        {
            if (dataEntries[i].EndDateTime > dataEntries[i + 1].StartDateTime)
            {
                dataEntries[i].TaskFlags = "Overlapping";
                dataEntries[i + 1].TaskFlags = "Overlapping";
            }
        }
        return dataEntries;
    }

    private string FormatDuration(int totalMinutes)
    {
        TimeSpan t = TimeSpan.FromMinutes(totalMinutes);
        return $"{(int)t.TotalHours:D2}h{t.Minutes:D2}m";
    }

    private string GetFlags(ProcessedData summary)
    {
        var flags = new List<string>();

        if (IsOverEightHours(summary.MinutesWorked))
        {
            flags.Add("Over 8 hours");
        }

        if (DoEntriesOverlap(summary.ExpandedDetails))
        {
            flags.Add("Overlap");
        }

        return string.Join("; ", flags);
    }

    private bool IsOverEightHours(int durationInMinutes)
    {
        return durationInMinutes > 8 * 60; // Check if total duration exceeds 8 hours (480 minutes)
    }

    private bool DoEntriesOverlap(List<TimesheetEntry> entries)
    {

        // Assuming entries are sorted by start time
        for (int i = 1; i < entries.Count; i++)
        {
            if (entries[i].StartDateTime < entries[i - 1].EndDateTime)
            {
                return true; // There's an overlap
            }
        }
        return false;
    }
}
