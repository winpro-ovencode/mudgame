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
        AdapterResultType Dispatch(Session s, string arg);
        void Enter(IPrompt roomSettingPrompt);
    }

    public class PromptAttribute : Attribute
    {
        internal PromptType promptType;

        public PromptAttribute(PromptType type)
        {
            promptType = type;
        }
    }

    public enum PromptType
    {
        Auth,
        MakeAccount,
        Game,
        None,
    }

    [Prompt(PromptType.Auth)]
    public class AuthPrompt : IPrompt
    {
        private Server owner;
        private AuthPromptState state;
        private Dictionary<AuthPromptState, string> cmd = new Dictionary<AuthPromptState, string>();
        private Session session;

        public Server OwnerServer {
            get 
            {
                return owner;
            }
        }
            
        public AuthPrompt(Server server, Session s)
        {
            this.owner = server;
            this.session = s;
            s.Send("동물의 땅에 접속한 것을 환영합니다.\r\n" +
                "처음 접속하셨으면 '손님'을 입력하시고 기존 사용자면 계정을 입력해주세요.\r\n");
            s.Send("계정을 입력하세요. : ");
        }

        public AdapterResultType Dispatch(Session s, string arg)
        {
            AdapterResultType result = AdapterResultType.Progress;

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
                    if(Login(s))
                        result = AdapterResultType.Next;

                    break;
            }

            return result;
        }
    
        private bool Login(Session s)
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
                    s.Send("계정이나 비밀번호가 틀립니다.\r\n계정을 다시 입력하세요 : ");
                    state = AuthPromptState.Ready;
                    cmd.Clear();
                    return false;
                }

                s.Send("인증 완료 되었습니다.\r\n");
                owner.Broadcast($"{accountId}님이 접속하셨습니다.\r\n");

                s.authInfo.accountId = accountId;
                return true;
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
                s.Send("비밀번호가 서로 틀립니다.\r\n다시 입력하세요. :");
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

        public void Enter(IPrompt roomSettingPrompt)
        {
            throw new NotImplementedException();
        }
    }

    
}