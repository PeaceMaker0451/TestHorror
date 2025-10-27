using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITextActor
{
    public TextBoxLine PersonalizeLine(string text);
    public List<TextBoxLine> PersonalizeLines(IEnumerable<string> text);
}
