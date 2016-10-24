using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LeastSquearsWpfVersion02
{
    class DataTable : Grid
    {


        public DataTable()
        {

        }

        public void AddColumn(string name)
        {
            this.ColumnDefinitions.Add(new ColumnDefinition());
        }
    }
}
