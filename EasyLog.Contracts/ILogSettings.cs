using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyLog.Contracts;

namespace EasyLog.Contracts
{
    public interface ILogSettings
    {
        LogMode GetLogMode();
        string GetLogServerUrl();
    }
}
