using System;
using System.Collections.Generic;
using System.Reflection;

public struct TargetInfo
{
    public Command commnad;
    public Type parameter;
    public MethodInfo method;
    
}

public class Dispatcher
{
    Dictionary<string, TargetInfo> protocolDic = new Dictionary<string, TargetInfo>();

    void RegisterFromAssembly()
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

            object[] attrs = info.parameter.GetCustomAttributes(typeof(ProtocolAttribute), true);
            if(attrs.Length == 0)
                continue; 


        }
    }
}

public class ProtocolAttribute
{
}

public class Command
{
}

public class CommandAttribute : Attribute
{
}

public class CommandGroupAttribute : Attribute
{
}