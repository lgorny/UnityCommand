using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class BaseCommand : ICommand
{
    private string name;
    public string Name { get { return name; } }

    private List<string> contexts;
    public List<string> Contexts { get { return contexts; } }   

    public event Action<ICommand> OnExecutionStart;
    public event Action<ICommand> OnExecutionComplete;

    public BaseCommand(string name, params string[] contexts)
    {
        this.name = name;
        this.contexts = new List<string>(contexts);
    }

    public virtual void Execute()
    {
        OnExecutionStart(this);
    }

    public virtual void Abort()
    {
        OnExecutionComplete(this);
    }

    public bool IsInContext(string context)
    {
        return Contexts.Contains(context);
    }
}
