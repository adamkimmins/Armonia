using System.Collections.Generic;

namespace Armonia.App.Models
{
    public class EditGraph
    {
        public List<string> Operations { get; set; } = new();
        public void AddOperation(string op) => Operations.Add(op);
    }
}
