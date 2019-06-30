using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CommonDef
{
    public  class CouplerFileReader
    {
        struct CoupleData
        {
            public double frequency;
            public double power;
            public double phase;
        }
         CoupleData[] m_CouplerData = null;
         int m_coupleFilePoints = 0;
         double m_couplerFileFrequencyStep = 0.5;
         string m_rootDir;

         public Dictionary<double, double>[] m_capDataPower = new Dictionary<double, double>[2];
         public Dictionary<double, double>[] m_capDataPhase = new Dictionary<double, double>[2];

        public  double getPower(double frequency)
        {
            try
            {
                int index = (int)((frequency - 2400) / m_couplerFileFrequencyStep);
                return m_CouplerData[index].power;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

        public  double getPhase(double frequency)
        {
            try
            {
                int index = (int)((frequency - 2400) / m_couplerFileFrequencyStep);
                return m_CouplerData[index].phase;
            }
            catch (Exception err)
            {
                return 0;
            }
        }
        public  void SetRootDir(string root)
        {
            m_rootDir = root;
        }

        public CouplerFileReader(int channel, string directory, bool LoadReflected  ,double startFreq = 902) 
        {
            string fileName;

            try
            {
                DetectorsType [] detType = {DetectorsType.ForwardDetector, DetectorsType.ReflectedDetector};
                for (int k = 0; k < 2; k++)
                {
                    fileName = directory;

                    if (LoadReflected == false && k == 1)
                        return;

                    if (detType[k] == DetectorsType.ForwardDetector)
                    {
                        fileName += "FWD" + (channel + 1) + ".csv";
                    }
                    else
                    {
                        fileName += "REF" + (channel + 1) + ".csv";
                    }
                    StreamReader reader = new StreamReader(fileName);
                    string line;
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    if (line != "Frequency, Formatted Data, Formatted Data")
                    {
                        throw new System.Runtime.InteropServices.ExternalException("Invalid coupler file format", 0);
                    }
                    int lineCount = 0;
                    while (reader.EndOfStream == false)
                    {
                        line = reader.ReadLine();
                        lineCount++;
                    }
                    m_coupleFilePoints = lineCount;
                    if (m_CouplerData == null)
                        m_CouplerData = new CoupleData[lineCount];

                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    reader.BaseStream.Position = 0;
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    line = reader.ReadLine();
                    lineCount = 0;
                    int detIndex = (int)detType[k];
                    m_capDataPower[detIndex] = new Dictionary<double, double>();
                    m_capDataPhase[detIndex] = new Dictionary<double, double>();
                    while (reader.EndOfStream == false)
                    {
                        string[] s = reader.ReadLine().Split(new Char[] { ',' });
                        m_CouplerData[lineCount].frequency = double.Parse(s[0]);
                        if ((lineCount == 0) && (m_CouplerData[lineCount].frequency != startFreq * 1000000))
                            throw new System.Runtime.InteropServices.ExternalException("Invalid coupler file format\nMust start with 800 Mhz", 0);
                        m_CouplerData[lineCount].power = double.Parse(s[1]);
                        m_CouplerData[lineCount].phase = double.Parse(s[2]);


                        m_capDataPower[detIndex].Add((double)m_CouplerData[lineCount].frequency / 1000000.0, m_CouplerData[lineCount].power);
                        m_capDataPhase[detIndex].Add((double)m_CouplerData[lineCount].phase, m_CouplerData[lineCount].phase);
                        lineCount++;
                    }
                    reader.Close();
                }
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }
    }

    
}
