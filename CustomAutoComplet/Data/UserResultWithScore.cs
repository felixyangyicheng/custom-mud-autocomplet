namespace CustomAutoComplet.Data;

public class UserResultWithScore:User
{
    public string MatchType { get; init; } = "";
    public int Score { get; init; } 
    public string DisplayName =>
        $"{FirstName} {LastName}";
}
