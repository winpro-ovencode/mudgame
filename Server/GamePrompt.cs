using MudGameTuto;

[Prompt(PromptType.Game)]
public class GamePrompt : IPrompt
{
    Server owner;
    Session _session;

    public GamePrompt(Server owner, Session s)
    {
        this.owner = owner;
        _session = s;
        s.Send("[Command]: ");
    }

    public AdapterResultType Dispatch(Session s, string arg)
    {
        AdapterResultType result = AdapterResultType.Progress;
        owner.Broadcast(string.Format("{0}: {1}\r\n", s.authInfo.accountId, arg));
        _session.Send("[Command]: ");
        return result;
    }
}

public class AuthInfo
{
    public string accountId;
    public string ipAddress;
}