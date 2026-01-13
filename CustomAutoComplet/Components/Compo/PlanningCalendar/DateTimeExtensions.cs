namespace CustomAutoComplet.Components.Compo.PlanningCalendar;

public static class DateTimeExtensions
{
    public static DateTime AddPeriod(this DateTime date, int delta, CalendarView view)
        => view switch
        {
            CalendarView.Week => date.AddDays(7 * delta),
            CalendarView.Month => date.AddMonths(delta),
            CalendarView.Year => date.AddYears(delta),
            _ => date
        };
}
