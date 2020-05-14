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

    }

    public class Monster : Mobile
    {

    }

    public class Player : Mobile
    {
        public Session session;
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
        }

        public void Leave(Mobile mob)
        {
            mobs.Remove(mob);
        }

        public void Link(string dirName, Room room)
        {
            Direction newDir = new Direction();
            newDir.name = dirName;
            newDir.target = room;
            dir.Add(newDir);
        }
    }
}