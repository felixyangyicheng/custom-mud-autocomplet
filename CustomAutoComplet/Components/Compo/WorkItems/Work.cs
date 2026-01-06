namespace CustomAutoComplet.Components.Compo.WorkItems;

public class Work
{
    public bool Selection { get; set; }
    public DateTime Open { get; set; }
    public DateTime Close { get; set; }
    public int ProjetId { get; set; }
    public string ProjetCode { get; set; } = "";

    public string Description { get; set; } = "";
    public string Commentaire { get; set; } = "";
    public TimeSpan? Hours { get; set; }

    private TimeSpan HoursCount;

    public TimeSpan _hourCount
    {
        get { return HoursCount; }
        set { HoursCount = (Close-Open); }
    }

}
