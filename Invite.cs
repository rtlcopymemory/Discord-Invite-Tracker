namespace InviteTracker;

public class Invite
{
    public int Id { get; set; }
    public string InviterId { get; set; }
    public string InviteCode { get; set; }
    public DateTimeOffset? ExprireDate { get; set; }
    public int Uses { get; set; }
    public int MaxUses { get; set; }
}