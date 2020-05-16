using System;
using System.Collections.Generic;

namespace MudGameTuto
{
    public class Direction
    {
        public string name;
        public Room target;
    }

    public class Object
    {
        public int id;
        public string name;
        public string desc;
    }

    public class Mobile : Object
    {
        public Room currentRoom;

        internal virtual void Write(string v)
        {
            
        }

        internal void LookAt(Mobile player)
        {
            player.Write($"{name}님이 서 있습니다.\r\n");
        }
    }

    public class Monster : Mobile
    {
    }

    public class Player : Mobile
    {
        public Session session;

        internal void Broadcast(string v)
        {
            currentRoom.Broadcast(v);
        }

        internal Room GetCurrentRoom()
        {
            return currentRoom;
        }

        internal override void Write(string v)
        {
            session.Send(v);
        }

        internal void WriteLine(string v)
        {
            session.Send(v + "\r\n");
        }

        internal void EnterPrompt(RoomSettingPrompt roomSettingPrompt)
        {
            session.tag.Enter(roomSettingPrompt);
        }
    }

    public class Room
    {
        private static int keyGenerator = 1;

        public int id;
        public string name;
        public string desc;
        public List<Direction> dir = new List<Direction>();
        public List<Mobile> mobs = new List<Mobile>();

        static public Room NewRoom()
        {
            Room room = new Room();
            room.id = keyGenerator++;
            room.name = "빈 방";
            room.desc = "빈 공간입니다. 방설정으로 변경하세요.";
            return room;
        }

        public void Enter(Mobile mob)
        {
            mobs.Add(mob);
            mob.currentRoom = this;
        }

        public void Leave(Mobile mob)
        {
            mobs.Remove(mob);
            mob.currentRoom = null;
        }

        public void Link(string dirName, Room room)
        {
            Direction newDir = new Direction();
            newDir.name = dirName;
            newDir.target = room;
            dir.Add(newDir);
        }

        internal void Broadcast(string v)
        {
            foreach(var p in mobs)
            {
                p.Write(v);
            }
        }

        internal Direction FindDirection(string v)
        {
            foreach(var d in dir)
            {
                if(d.name == v)
                    return d;
            }

            return null;
        }

        public void LookAt(Mobile player)
        {
            var roomDesc = string.Format($"{name}\r\n{desc}\r\n");
            player.Write(roomDesc);
        
            string exit = "";
            bool first = true;
            string roomExit = "";
            foreach(var dir in this.dir)
            {
                if(!first)
                    exit += ' ';
                    
                first = false;
                exit += dir.name;
            }
            
            if(first)
            {
                roomExit = string.Format($"갈 수 있는 곳이 없습니다.\r\n");
            } else
            {
                roomExit = string.Format($"갈 수 있는 곳은 [{exit}] 입니다.\r\n");
            }

            player.Write(roomExit);
            player.Write("\r\n");

            foreach(var m in mobs)
            {
                m.LookAt(player);
            }
        }

        internal string FindRoute(Room room)
        {
            var finded = dir.Find(w=> w.target == room);

            if(finded == null)
            {
                return "";
            }

            return finded.name;
        }
    }
}