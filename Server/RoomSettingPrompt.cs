using System.Collections.Generic;

namespace MudGameTuto
{
    

    public enum RoomSettingPromptState
    {
        RoomName,
        RoomDesc,
        End
    }

    public class RoomSettingPrompt : IPrompt
    {
        private RoomSettingPromptState state;
        private string roomName;
        private List<string> roomDesc = new List<string>();
        private Room currentRoom;

        public RoomSettingPrompt(Session s, Room r)
        {
            s.Send("방 이름을 입력하세요: ");
            currentRoom = r;
        }

        public AdapterResultType Dispatch(Session s, string arg)
        {
            switch(state)
            {
                case RoomSettingPromptState.RoomName:
                    roomName = arg;
                    s.Send("방 설명을 입력하세요(여러행 가능) : ");
                    state = RoomSettingPromptState.RoomDesc;
                break;

                case RoomSettingPromptState.RoomDesc:
                    if(arg == " ")
                    {
                        state = RoomSettingPromptState.End;
                        currentRoom.name = roomName;
                        currentRoom.desc = string.Concat(roomDesc);
                        return AdapterResultType.Leave;
                    }

                    state = RoomSettingPromptState.RoomDesc;
                    roomDesc.Add(arg  + "\r\n");
                    s.Send(": ");
                break;
            }

            return AdapterResultType.Progress;
        }

        public void Enter(IPrompt roomSettingPrompt)
        {
        }
    }
}