namespace API.Helpers;

public class UserParams : PaginationParams
{
    public string CurrentUsername { get; set; }
    public string DesiredGender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}
