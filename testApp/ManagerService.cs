using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testApp
{
    public class ManagerService : IManagerService
    {
        public string GetData(string value)
        {
            int formattedValue = Convert.ToInt32(value);
            return string.Format("You entered: {0}", formattedValue);
        }
    }
}
