using System;
using System.Collections.Generic;
using System.Reflection;

namespace MudGameTuto
{
    public struct TargetInfo
    {
        public Command commnad;
        public Type parameter;
        public MethodInfo method;
        
    }

    public class Dispatcher
    {
        Dictionary<string, TargetInfo> protocolDic = new Dictionary<string, TargetInfo>();

        public bool Dispatch(Command cmd, Player player, string arg)
        {
            var args = arg.Split(' ');

            if(!protocolDic.ContainsKey(args[0]))
                return false;

            var p = protocolDic[args[0]];

            p.method.Invoke(cmd, new object[] { player, arg });
            return true;
        }

        public void RegisterFromAssembly()
        {
            Assembly assembly = GetType().Assembly;

            foreach(var type in assembly.GetTypes())
            {
                if( !Attribute.IsDefined(type, typeof(CommandGroupAttribute)) )
                    continue;

                Register(type);
            }
        }

        void Register(Type type)
        {
            foreach(MethodInfo field in type.GetMethods())
            {
                object[] attr = field.GetCustomAttributes(typeof(CommandAttribute), true);
                if(attr.Length == 0)
                    continue;

                CommandAttribute targetAttr = (CommandAttribute)attr[0];
                TargetInfo info = new TargetInfo();
                info.commnad = (Command)Activator.CreateInstance(type);
                info.method = field;
                info.parameter = field.GetParameters()[1].ParameterType;

                foreach(var c in targetAttr.cmds)
                {
                    protocolDic.Add(c, info);
                }
            }
        }
    }

    public class ProtocolAttribute
    {
    }

    public class Command
    {
        public virtual void Entry(Player player)
        {

        }
    }

    public class CommandAttribute : Attribute
    {
        public List<string> cmds = new List<string>();

        public CommandAttribute(string cmd)
        {
            cmds.Add(cmd);
        }

        public CommandAttribute(params string[] cmd )
        {
            cmds.AddRange(cmd);
        }
    }

    public class CommandGroupAttribute : Attribute
    {
    }
}