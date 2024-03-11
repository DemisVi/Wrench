using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wrench.Services;

public interface ICommandSequence
{
    public IEnumerable<ICommand>? Commands { get; set; }
    public void Run();
}
