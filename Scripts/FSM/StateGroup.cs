using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class StateGroupAttribute : Attribute
{
    public readonly string groupName;

   
    public StateGroupAttribute(string displayName)
    {
        this.groupName = displayName;
    }
}