using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDef
{
    public struct PowerMeterSelect
    {
        public string sensorName;
        public int ampId;
    }

    public enum DetectorsType
    {
        ForwardDetector,
        ReflectedDetector
    }

 
}
