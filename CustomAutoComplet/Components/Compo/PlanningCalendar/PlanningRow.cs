namespace CustomAutoComplet.Components.Compo.PlanningCalendar;

public enum RowType
{
    Person,
    Resource
}

public class PlanningRow
{
    public RowType Type { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
}