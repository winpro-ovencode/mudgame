using System.Collections.Generic;

namespace MudGameTuto
{
    public class Zone
    {
        public Room EntryRoom;
        public Dictionary<int, Room> Rooms = new Dictionary<int, Room>();

        static public Zone CreateZone()
        {
            Zone zone = new Zone();
            var newRoom = Room.NewRoom();
            zone.Rooms.Add(newRoom.id, newRoom);
            zone.EntryRoom = newRoom;

            return zone;
        }

        public void Enter(Mobile mob)
        {
            EntryRoom.LookAt(mob);
            EntryRoom.Enter(mob);
        }
    }
}