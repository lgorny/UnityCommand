public interface IContext
{
    string Name { get; }
    bool Busy { get; }
    bool Enabled { get; set; }

    void AddCommand(ICommand command);
    void RemoveCommand(ICommand command);    
}
