using System;

/// <summary>
/// Sets Unity script execution order.
/// </summary>
public class ScriptOrderAttribute : Attribute
{
    public readonly int ExecutionOrder;

    public ScriptOrderAttribute (int order)
    {
        ExecutionOrder = order;
    }
}

