namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Stores commands and supports previous/next navigation.
    /// </summary>
    public interface ICommandHistory
    {
        void Add(string command);
        string Previous(string currentInput);
        string Next();
    }
}
