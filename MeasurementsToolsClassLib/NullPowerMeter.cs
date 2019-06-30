using System;
using System.Collections.Generic;
using System.Text;

namespace MeasurementsToolsClassLib
{
    public class NullPowerMeter : NRP_Z211PowerMeter
    {
        double initialePower = -0.238;
        double powerStep;
        public NullPowerMeter() 
        {
            powerStep = 0.1;
        }
        public override double Read(int Channel, int time_out)
        {
           return 50;
        }

        public override double Read(int numberOfReads)
        {
            double p = initialePower + powerStep;
            powerStep += 0.1;
            return p;            
        }
    }
}
