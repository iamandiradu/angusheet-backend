namespace TimesheetValidationApi.Models
{
    public class TimesheetEntry
    {
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Duration => (int)(EndDateTime - StartDateTime).TotalMinutes;
        public string TaskFlags { get; set; }
    }
}