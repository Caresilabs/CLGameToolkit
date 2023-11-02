using System;


[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class RequireState : Attribute
{
    public readonly Type[] types;

    public RequireState(params Type[] types)
    {
        this.types = types;
    }
}
