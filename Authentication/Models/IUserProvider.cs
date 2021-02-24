namespace Planner.Authentication.Models
{
    public interface IUserProvider
    {
        User User { get; }
    }
}