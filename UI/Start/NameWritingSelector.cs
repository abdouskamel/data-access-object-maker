using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UI.Start
{
    public class NameWritingSelector : ComboBox
    {
        public NameWritingSelector() : base()
        {
            AddText("lowerCamelCase");
            AddText("PascalCase");
            AddText("c_case");
            AddText("Défaut");

            SelectedIndex = 1;
        }
    }
}
