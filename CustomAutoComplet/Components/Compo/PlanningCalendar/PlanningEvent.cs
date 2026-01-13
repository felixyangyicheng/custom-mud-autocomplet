namespace CustomAutoComplet.Components.Compo.PlanningCalendar;

public class PlanningEvent
{
    public int Id { get; set; }
    public int? PersonId { get; set; }
    public int? ResourceId { get; set; }

    public DateTime Date { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}
