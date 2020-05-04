using System;
using System.Collections.Generic;
using ServiceStack.OrmLite;

namespace MudGameTuto
{
    public enum AuthPromptState
    {
        Ready,
        CreateAccount,
        MakePassword,
        MakePassword2,
        Password,
        End
    }

    public class t_account
    {
        public string accountId {get;set;}
        public string password {get;set;}
        public DateTime creation {get;set;}
    }
    public interface IPrompt
    {
        void Dispatch(Session s, string arg);
    }
    
    public class AuthPrompt : IPrompt
    {
        private Server owner;
        private AuthPromptState state;
        private Dictionary<AuthPromptState, string> cmd = new Dictionary<AuthPromptState, string>();

        public Server OwnerServer {
            get 
            {
                return owner;
            }
        }
            
        public AuthPrompt(Server server)
        {
            this.owner = server;
        }

        public void Dispatch(Session s, string arg)
        {
            switch(state)
            {
                case AuthPromptState.Ready:
                    if(arg == "손님")
                    {
                        s.Send("만드실 계정을 입력하세요 : ");
                        state = AuthPromptState.CreateAccount;
                    } else
                    {
                        s.Send("비밀번호를 입력하세요 : ");
                        cmd.Add(state, arg);
                        state = AuthPromptState.Password;
                    }
                    break;
                case AuthPromptState.CreateAccount:
                    cmd.Add(state, arg);
                    s.Send("비밀번호를 입력하세요: ");
                    state = AuthPromptState.MakePassword;
                break;

                case AuthPromptState.MakePassword:
                    cmd.Add(state, arg);
                    s.Send("비밀번호를 다시 입력하세요: ");
                    state = AuthPromptState.MakePassword2;
                break;

                case AuthPromptState.MakePassword2:
                    cmd.Add(state, arg);
                    state = AuthPromptState.End;
                    CreateAccount(s);
                break;

                case AuthPromptState.Password:
                    cmd.Add(state, arg);
                    state = AuthPromptState.End;
                    Login(s);
                    break;
            }
        }
    
        private void Login(Session s)
        {
            s.Send("인증 확인중입니다...\r\n");
            using(var con = DBContext.Open())
            {
                string accountId = cmd[AuthPromptState.Ready];
                string password = cmd[AuthPromptState.Password];

                t_account acc = con.Single<t_account>(w => w.accountId == accountId && w.password == password);
                //SELECT * FROM t_account WHERE accountId = accountId and password = password limit 1

                if(acc == null)
                {
                    s.Send("계정이나 비밀번호가 틀립니다. 계정을 다시 입력하세요 : ");
                    state = AuthPromptState.Ready;
                    cmd.Clear();
                    return;
                }
                s.Send("인증 완료 되었습니다.\r\n");
                owner.Broadcast($"{accountId}님이 접속하셨습니다.\r\n");

                AuthInfo authInfo = new AuthInfo();
                authInfo.accountId = accountId;
                s.tag = new GamePrompt(owner, authInfo, s);
            }
        }

        private void CreateAccount(Session s)
        {
            s.Send("계정 생성중입니다...\r\n");
            string accountId = cmd[AuthPromptState.CreateAccount];
            string password = cmd[AuthPromptState.MakePassword];
            string password2 = cmd[AuthPromptState.MakePassword2];

            if(password != password2)
            {
                s.Send("비밀번호가 서로 틀립니다. 다시 입력하세요. :");
                state = AuthPromptState.MakePassword;
                cmd.Remove(AuthPromptState.MakePassword);
                cmd.Remove(AuthPromptState.MakePassword2);
                return;
            }

            t_account acc = new t_account();
            acc.accountId = accountId;
            acc.password = password;
            acc.creation = DateTime.Now;

            using(var con = DBContext.Open())
            {
                if(con.Exists<t_account>(w => w.accountId == accountId))
                {
                    s.Send($"{accountId} 계정이 이미 있습니다. 계정을 다시 입력하세요: ");
                    state = AuthPromptState.CreateAccount;
                    cmd.Clear();
                    return;
                }
                
                con.Insert(acc);
            }
        
            s.Send($"{accountId} 계정이 생성되었습니다. 접속하실 계정을 입력하세요: ");
            state = AuthPromptState.Ready;
            cmd.Clear();
        }
    }
}