using System;

namespace MudGameTuto
{
    [CommandGroup]
    public class GameCommand : Command
    {
        public override void Entry(Player player)
        {
            player.Broadcast($"{player.name}님이 입장 하였습니다.\r\n");
        }

        [Command("동", "서", "남", "북")]
        public void MoveWest(Player player, string arg)
        {
            Room room = player.GetCurrentRoom();

            Direction dir = room.FindDirection(arg);

            if(dir == null)
            {
                player.Write("출구가 없습니다.\r\n");
                return;
            }

            var route = dir.target.FindRoute(room);

            room.Leave(player);
            room.Broadcast($"{player.name}님이 {arg}쪽으로 갔습니다.\r\n");
            dir.target.Broadcast($"{player.name}님이 {route}쪽에서 왔습니다.\r\n");
            dir.target.LookAt(player);
            dir.target.Enter(player);
        }
        
        [Command("방생성")]
        public void RoomMake(Player player, string arg)
        {
            var args = arg.Split(' ');
            var srcRoom = player.currentRoom;
            var dstRoom = Room.NewRoom(); 
            var cmd = args[0];
            var arg1 = args[1];

            if(arg1 == "서")
            {
                srcRoom.Link("서", dstRoom);
                dstRoom.Link("동", srcRoom);
            } else if(arg1 == "동")
            {
                srcRoom.Link("동", dstRoom);
                dstRoom.Link("서", srcRoom);
            }
            else if(arg1 == "남")
            {
                srcRoom.Link("남", dstRoom);
                dstRoom.Link("북", srcRoom);
            }
            else if(arg1 == "북")
            {
                srcRoom.Link("북", dstRoom);
                dstRoom.Link("남", srcRoom);
            }

            player.Write("방이 생성 되었습니다.\r\n");
        }

        [Command("봐")]
        public void Look(Player player, string arg)
        {
            player.currentRoom.LookAt(player);
        }

        [Command("말")]
        public void Say(Player player, string arg)
        {
            var args = arg.Split(' ');

            if(args.Length < 2)
            {
                player.Write("Help: 말 [할말]\r\n");
                return;
            }

            string msg = $"{player.name}님: {args[1]}\r\n";
            player.Broadcast(msg);
        }
        
        [Command("방설정")]
        public void RoomSetting(Player player, string arg)
        {
            player.EnterPrompt(new RoomSettingPrompt(player.session, player.currentRoom));
        }
    }
}
