using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandsManager
{
    public enum UpdateMethod
    {
        NONE,
        /// <summary>
        /// Commands queue is updated every time when list of command or contexts is changed.
        /// </summary>
        EVENT_DRIVEN,
        /// <summary>
        /// Commands queue is updated manualy, ouside this class.
        /// </summary>
        MANUAL
    }

    private UpdateMethod queueUpdateMethod;
    /// <summary>
    /// Queue updating method
    /// </summary>
    public UpdateMethod QueueUpdateMethod
    {
        get
        {
            return queueUpdateMethod;
        }
        set
        {
            if (queueUpdateMethod == value)
                return;
            
            if (queueUpdateMethod == UpdateMethod.EVENT_DRIVEN)
                InternalUpdate -= UpdateQueue;
            else if(value == UpdateMethod.EVENT_DRIVEN)
                InternalUpdate += UpdateQueue;

            queueUpdateMethod = value;
        }
    }

    /// <summary>
    /// List of unactive commads, waiting in queue for non-busy context.
    /// </summary>
    private List<ICommand> pendingCommands;    
    /// <summary>
    /// List of commands curently active in contexts.
    /// </summary>
    private List<ICommand> activeCommands;
    /// <summary>
    /// List of all added context to CommandManager.
    /// </summary>
    private Dictionary<string, IContext> contexts;

    /// <summary>
    /// Called when changes in queue or contexts were made.
    /// </summary>
    private Action InternalUpdate = () => { };    

    public CommandsManager(UpdateMethod method = UpdateMethod.EVENT_DRIVEN)
    {
        pendingCommands = new List<ICommand>();
        activeCommands = new List<ICommand>();
        contexts = new Dictionary<string, IContext>();

        QueueUpdateMethod = method;
    }

    public void AddContext(IContext context)
    {
        if(contexts.ContainsKey(context.Name))
        {
            Debug.LogWarning(string.Format("[CommandsManager] Context with Name: {0} already exists.", context.Name));
            return;
        }

        contexts.Add(context.Name, context);

        InternalUpdate();
    }

    public void RemoveContext(IContext context)
    {
        RemoveContext(context.Name);
    }

    public void RemoveContext(string name)
    {
        if (!contexts.ContainsKey(name))
        {
            Debug.LogWarning(string.Format("[CommandsManager] There is no context with Name: {0}.", name));
            return;
        }

        contexts.Remove(name);

        InternalUpdate();
    }

    public void AddToQueue(ICommand command)
    {
        command.OnExecutionStart += OnExecutionStart;
        command.OnExecutionComplete += OnExecutionComplete;

        pendingCommands.Add(command);

        InternalUpdate();
    }

    /// <summary>
    /// Returns string with information about queue state. Use for debug purposes.
    /// </summary>
    public string PrintQueue()
    {
        var queue = "";
        foreach(var key in contexts)
        {
            queue += key.Key + ":\n";
            queue += "  Active: \n";

            for(int i = 0; i < activeCommands.Count; i++)
            {
                if(activeCommands[i].IsInContext(key.Key))
                    queue += "      " + activeCommands[i].Name + "\n";
            }

            queue += "  Pending: \n";
            for (int i = 0; i < pendingCommands.Count; i++)
            {
                if (pendingCommands[i].IsInContext(key.Key))
                    queue += "      " + pendingCommands[i].Name + "\n";
            }
        }

        return queue;
    }

    /// <summary>
    /// If queue update method was tet to manual, use this to update queue state, e.g. once per frame.
    /// </summary>
    public void UpdateQueue()
    {
        var commandsClone = new List<ICommand>(pendingCommands);
        for (int i = 0; i < commandsClone.Count; i++)
        {
            if (CanBeExecuted(commandsClone[i]))
                commandsClone[i].Execute();
        }
    }

    private bool CanBeExecuted(ICommand command)
    {
        int len = command.Contexts.Count;
        for(int i = 0; i < len; i++)
        {
            IContext context = null;
            if (contexts.TryGetValue(command.Contexts[i], out context))
            {
                if (context.Busy || !context.Enabled)
                    return false;
            }
            else
                return false;
        }

        return true;
    }

    private void OnExecutionStart(ICommand command)
    {
        command.OnExecutionStart -= OnExecutionStart;

        int len = command.Contexts.Count;
        for (int i = 0; i < len; i++)
        {
            IContext context = null;
            if (contexts.TryGetValue(command.Contexts[i], out context))
                context.AddCommand(command);
        }

        pendingCommands.Remove(command);
        activeCommands.Add(command);
    }

    private void OnExecutionComplete(ICommand command)
    {
        command.OnExecutionComplete -= OnExecutionComplete;

        int len = command.Contexts.Count;
        for (int i = 0; i < len; i++)
        {
            IContext context = null;
            if (contexts.TryGetValue(command.Contexts[i], out context))
                context.RemoveCommand(command);
        }

        activeCommands.Remove(command);

        InternalUpdate();
    }
}
