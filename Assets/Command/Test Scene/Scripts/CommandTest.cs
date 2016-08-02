using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CommandTest : MonoBehaviour
{
    internal class CorutineCommand : BaseCommand
    {
        private float secondsToWait;
        private MonoBehaviour parent;

        public CorutineCommand(float secondsToWait, MonoBehaviour parent, params string[] contexts) 
            : this("CorutineCommand", secondsToWait, parent, contexts)
        {
        }

        public CorutineCommand(string name, float secondsToWait, MonoBehaviour parent, params string[] contexts)
            : base(name, contexts)
        {
            this.secondsToWait = secondsToWait;
            this.parent = parent;
        }

        public override void Execute()
        {
            base.Execute();
            Debug.Log("Execute: " + Name);
            parent.StartCoroutine(WaitForSeconds());
        }

        private IEnumerator WaitForSeconds()
        {
            yield return new WaitForSeconds(secondsToWait);
            Abort();
        }

        public override void Abort()
        {
            Debug.Log("Abort: " + Name);
            base.Abort();
        }
    }

    internal class ChangeUpdateModeCommand : BaseCommand
    {
        private CommandsManager.UpdateMethod mode;
        private CommandsManager commandsManager;

        public ChangeUpdateModeCommand(string name, CommandsManager.UpdateMethod mode, CommandsManager commandsManager, params string[] contexts)
            : base(name, contexts)
        {
            this.mode = mode;
            this.commandsManager = commandsManager;
        }

        public override void Execute()
        {
            base.Execute();
            Debug.Log("Execute: " + Name);
            commandsManager.QueueUpdateMethod = mode;

            Abort();
        }

        public override void Abort()
        {
            Debug.Log("Abort: " + Name);
            base.Abort();
        }
    }

    public string commandsQueue;

    private CommandsManager commandManager;

	void Start ()
    {
        commandManager = new CommandsManager(CommandsManager.UpdateMethod.EVENT_DRIVEN);

        var commands = new List<ICommand>();
        commands.Add(new CorutineCommand("A", 1f, this, "Context 1", "Context 2", "Context 3"));
        commands.Add(new CorutineCommand("B", 1f, this, "Context 1", "Context 2", "Context 3"));
        commands.Add(new CorutineCommand("C", 1f, this, "Context 1", "Context 3"));
        commands.Add(new CorutineCommand("D", 1f, this, "Context 1", "Context 3"));
        commands.Add(new CorutineCommand("E", 1f, this, "Context 2"));
        commands.Add(new CorutineCommand("F", 1f, this, "Context 2"));
        commands.Add(new CorutineCommand("G", 1f, this, "Context 2", "Context 3"));
        commands.Add(new CorutineCommand("H", 1f, this, "Context 2"));
        commands.Add(new CorutineCommand("I", 1f, this, "Context 3"));
        commands.Add(new CorutineCommand("J", 1f, this, "Context 3"));

        var changeUpdateModeEvent = new ChangeUpdateModeCommand("Change to UpdateMethod.EVENT_DRIVEN", CommandsManager.UpdateMethod.EVENT_DRIVEN, commandManager, "Context 1", "Context 2", "Context 3");
        var changeUpdateModeManual = new ChangeUpdateModeCommand("Change to UpdateMethod.MANUAL", CommandsManager.UpdateMethod.MANUAL, commandManager, "Context 1", "Context 2", "Context 3");

        var contexts = new List<IContext>();
        contexts.Add(new BaseContext("Context 1"));
        contexts.Add(new BaseContext("Context 2"));
        contexts.Add(new BaseContext("Context 3"));

        Action StartManualUpdate = () => 
        {
            FillWithCommands(commands);
            commandManager.AddToQueue(changeUpdateModeEvent);
        };

        Action StartEventUpdate = () =>
        {
            FillWithCommands(commands);
            commandManager.AddToQueue(changeUpdateModeManual);
        };

        changeUpdateModeEvent.OnExecutionComplete += (c) => StartEventUpdate();
        changeUpdateModeManual.OnExecutionComplete += (c) => StartManualUpdate();

        StartEventUpdate();
        FillWithContexts(contexts);
    }

    private void FillWithCommands(List<ICommand> commands)
    {
        for (int i = 0; i < commands.Count; i++)
            commandManager.AddToQueue(commands[i]);
    }

    private void FillWithContexts(List<IContext> contexts)
    {
        for (int i = 0; i < contexts.Count; i++)
            commandManager.AddContext(contexts[i]);
    }

    void Update()
    {
        if (commandManager.QueueUpdateMethod == CommandsManager.UpdateMethod.MANUAL)
            commandManager.UpdateQueue();

        commandsQueue = commandManager.PrintQueue();
    }
}
