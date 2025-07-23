using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;

namespace AuthGatun.Services;

public class UserStatus
{
    private UserStatus() {}

    private static UserStatus? _instance;
    private static readonly object Lock = new object();

    public User? User { get; set; }
    
    public static UserStatus GetInstance()
    {
        if (_instance is null)
        {
            lock (Lock)
            {
                if (_instance is null)
                {
                    _instance = new UserStatus();
                }
            }
        }

        return _instance;
    }
}