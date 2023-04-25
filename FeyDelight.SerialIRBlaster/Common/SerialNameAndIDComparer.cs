using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    public class SerialNameAndIdComparer : IComparer<SerialNameAndId>
    {
        public int Compare(SerialNameAndId x, SerialNameAndId y)
        {
            if (x == null && y == null) return 0;
            if (x.Description.ToLower().Contains("arduino")) return 1;
            if (y.Description.ToLower().Contains("arduino")) return -1;
            else return x.Description.CompareTo(y.Description);
        }
    }
}
