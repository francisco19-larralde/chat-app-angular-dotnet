namespace ChatApp.Application.Interfaces;

public interface IUserConnectionTracker
{

    bool AddConnection(int userId, string connectionId);


    bool RemoveConnection(int userId, string connectionId);

    bool IsUserOnline(int userId);
}