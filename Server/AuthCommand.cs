using System;

namespace MudGameTuto
{
    public enum AuthState
    {
        None,
        Close,
        Connect,
        Account,
        Password,
    }

    public enum PromptType
    {
        Auth,
        MakeAccount,
        Game,
    }

    public class AuthCommand : Command
    {
        [Command]
        public void Connect(Session session)
        {
            // 접속
            // 계정을 입력하세요.
            // 암호를 입력하세요.
            // 끊김

            session.Send(
                @"동물의 땅에 접속한 것을 환영합니다.
                처음 접속하셨으면 손님을 입력하시고 기존 사용자면 계정을 입력해주세요."
            );

          //  SetPrompt(new SequencePrompt(PromptType.Auth, "계정을 입력하세요: ", "암호를 입력하세요 :"));
        }

        [Command]
        public void Close(Session session)
        {

        }

        public void Auth(Session session, AuthPrompt arg)
        {

        }
    }
}
