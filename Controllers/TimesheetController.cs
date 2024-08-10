using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimesheetValidationApi.Data;
using TimesheetValidationApi.Models;

namespace TimesheetValidationApi.Controllers
{
  [Route("api/timesheet")]
  [ApiController]
  public class TimesheetController : ControllerBase
  {
    private readonly TimesheetDbContext _context;
    private readonly TimesheetProcessor _timesheetProcessor;


    public TimesheetController(TimesheetProcessor timesheetProcessor)
    {
      _timesheetProcessor = timesheetProcessor;
    }

    // Class that maps to the CSV columns
    public class CsvRecord
    {
      public string Description { get; set; }
      public string StartDateTime { get; set; }
      public string EndDateTime { get; set; }
    }

    // POST: api/timesheet/Upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTimesheet(IFormFile file)
    {
      if (file == null || file.Length == 0)
      {
        return BadRequest("No file uploaded.");
      }

      try
      {
        using (var reader = new StreamReader(file.OpenReadStream()))
        using (
          var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
              Delimiter = ";",
              HeaderValidated = null,
              MissingFieldFound = null,
              HasHeaderRecord = false
            }
          )
        )
        {
          // Read records as a list of dynamic objects
          var records = csv.GetRecords<dynamic>().ToList();

          // Manually extract and format values from specific columns (by index)
          var data = records
            .Select(record =>
            {
              // Convert dynamic record to IDictionary to access values by index
              var dict = record as IDictionary<string, object>;

              // Convert each value in the record to a string array
              return dict.Values.Select(value => value?.ToString() ?? string.Empty).ToArray();
            })
            .ToArray();

          // Process CSV data
          List<ProcessedData> summaryData = _timesheetProcessor.Summarize(data);


          return Ok(summaryData);
        }
      }
      catch (Exception ex)
      {
        return StatusCode(
          StatusCodes.Status500InternalServerError,
          $"Error processing file: {ex.Message}"
        );
      }
    }


  }
}
