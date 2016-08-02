using System;
using System.Collections.Generic;

public interface ICommand
{
    string Name { get; }
    List<string> Contexts { get; }

    void Execute();
    void Abort();
    bool IsInContext(string context);

    event Action<ICommand> OnExecutionStart;
    event Action<ICommand> OnExecutionComplete;
}
