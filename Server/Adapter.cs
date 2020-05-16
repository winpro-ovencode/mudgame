using System;
using System.Collections.Generic;

namespace MudGameTuto
{
    class Adapter
    {
        Zone zone;
        Server owner;
        List<Player> player = new List<Player>();

        public Adapter(Server server)
        {
            owner = server;
            server.SessionOpen = SessionOpen;
            server.SessionClose = SessionClose;
            server.SessionPacket = SessionPacket;
            zone = Zone.CreateZone();
        }

        private Player CreatePlayer(Session s)
        {
            Player p = new Player();
            p.session = s;
            player.Add(p);
            return p;
        }

        private void SessionPacket(Session s, string arg)
        {
            IPrompt prompt = (IPrompt) s.tag;
            AdapterResultType result = prompt.Dispatch(s, arg);

            if(result == AdapterResultType.Next)
            {
                switch(GetPromptType(prompt))
                {
                    case PromptType.Auth:
                        s.tag = new GamePrompt(owner, zone, s);
                        break;

                    case PromptType.Game:
                        break;
                }
            }
        }

        private PromptType GetPromptType(IPrompt prompt) //AuthPrompt GamePrompt
        {
            var attrs = prompt.GetType().GetCustomAttributes(true);

            if(attrs.Length == 0)
                return PromptType.None;

            var att = (PromptAttribute) attrs[0];
            
            return att.promptType;
        }

        private void SessionClose(Session obj)
        {
        }

        protected void SessionOpen(Session s)
        {
            Console.WriteLine("SessionOpen");
            s.tag = new AuthPrompt(owner, s);
            s.authInfo = new AuthInfo();
        }
    }
}