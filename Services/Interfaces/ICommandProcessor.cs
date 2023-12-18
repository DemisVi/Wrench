using System.Collections.Generic;

namespace Wrench.Services;

public interface ICommandProcessor
{
    public IEnumerable<ICommand>? Commands { get; set; }
    public void Run();
}
