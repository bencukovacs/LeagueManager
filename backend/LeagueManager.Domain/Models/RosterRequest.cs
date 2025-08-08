namespace LeagueManager.Domain.Models;

public enum RosterRequestType
{
    JoinRequest, // Initiated by a player
    Invite         // Initiated by a team leader
}

public enum RosterRequestStatus
{
    PendingLeaderApproval,   // A player is waiting for the leader
    PendingPlayerAcceptance, // A leader is waiting for the player
    Approved,
    Rejected,
    Cancelled
}

public class RosterRequest
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    
    public RosterRequestType Type { get; set; }
    public RosterRequestStatus Status { get; set; }
}