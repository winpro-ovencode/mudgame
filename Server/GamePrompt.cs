namespace MudGameTuto
{
    [Prompt(PromptType.Game)]
    public class GamePrompt : IPrompt
    {
        Server owner;
        Session _session;
        Player _player;
        Zone _zone;
        Dispatcher _dispatcher = new Dispatcher();
        GameCommand gameCommand = new GameCommand();
        IPrompt _childPrompt;

        public GamePrompt(Server owner, Zone zone, Session s)
        {
            this.owner = owner;
            _session = s;
            _zone = zone;
            _dispatcher.RegisterFromAssembly();

            Player p = new Player();
            p.name = s.authInfo.accountId;
            p.session = s;
            _player = p;

            _zone.Enter(p);
            gameCommand.Entry(p);
            s.Send("[Command]: ");
        }

        public AdapterResultType Dispatch(Session s, string arg)
        {
            AdapterResultType result = AdapterResultType.Progress;

            if(_childPrompt != null)
            {
                result = _childPrompt.Dispatch(s, arg);

                if(result == AdapterResultType.Leave)
                {
                    s.Send("[Command]: ");
                    _childPrompt = null;
                }

                return AdapterResultType.Progress; 
            }

            if(!_dispatcher.Dispatch(gameCommand, _player, arg))
            {
                s.Send("알 수 없는 명령입니다.\r\n");
            }

            if(_childPrompt != null)
                return result;
            
            _session.Send("[Command]: ");
            return result;
        }

        public void Enter(IPrompt roomSettingPrompt)
        {
            _childPrompt = roomSettingPrompt;
        }
    }

    public class AuthInfo
    {
        public string accountId;
        public string ipAddress;
    }
}