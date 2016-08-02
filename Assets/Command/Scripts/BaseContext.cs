using System;

public class BaseContext : IContext
{
    private string name;
    public string Name
    {
        get
        {
            return name;
        }
    }

    public bool Busy
    {
        get
        {
            return queued > 0;
        }
    }

    public bool Enabled { get; set; }

    private int queued;

    public BaseContext(string name)
    {
        this.name = name;
        Enabled = true;
        queued = 0;
    }

    public virtual void AddCommand(ICommand command)
    {
        queued++;
    }

    public virtual void RemoveCommand(ICommand command)
    {
        queued--;
    }
}
