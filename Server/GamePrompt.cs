using MudGameTuto;

public class GamePrompt : IPrompt
{
    Server owner;
    AuthInfo _authInfo;
    Session _session;

    public GamePrompt(Server owner, AuthInfo info, Session s)
    {
        this.owner = owner;
        _authInfo = info;
        _session = s;
        s.Send("[Command]: ");
    }
    
    public void Dispatch(Session s, string arg)
    {
        owner.Broadcast(string.Format("{0}: {1}\r\n", _authInfo.accountId, arg));
        s.Send("[Command]: ");
    }
}

public class AuthInfo
{
    public string accountId;
    public string ipAddress;
}