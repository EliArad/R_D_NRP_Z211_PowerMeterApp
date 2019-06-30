using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeasurementsToolsClassLib;
using System.Threading;
using CommonDef;
using System.Diagnostics;

namespace PowerMeterApp
{
    public partial class MeasurementAssistance : Form
    {
        PowerMeterForm m_powerForm = null;
        Thread m_PowerMeterThread = null;
        bool m_formActive = false;
        NRP_Z211PowerMeter[] m_powerMeter = { null, null, null, null };
        bool useCoupleFiles = false;
        CouplerFileReader[] m_couplerReader;
        string[] m_sensorsName;
        float m_frequency = 902;
        float m_coupleStatic = 41;
        public MeasurementAssistance()
        {

           try
           {
              InitializeComponent();
              Control.CheckForIllegalCrossThreadCalls = false;
              m_formActive = true;
              if (FillRodeSwartzPowerMeterList() == 0)
              {
                 button1.Enabled = false;
                 button2.Enabled = false;
              }
           }
           catch (Exception err)
           {
              MessageBox.Show(err.Message);
           }
        }

        public void setCurrentFrequency(float freq)
        {
            m_frequency = freq;
        }
        void PowerMeterThreadProcess()
        {
            Stopwatch sw = new Stopwatch();
            Label[] lbl = { label1, label2, label3, label4 };
            while (m_formActive)
            {
                lock (this)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (m_powerMeter[i] != null)
                        {
                            if (m_powerMeter[i] != null)
                            {
                                sw.Restart();
                                double powerValue = m_powerMeter[i].Read(1);
                                Console.WriteLine(sw.ElapsedMilliseconds);
                                if (noCouplerToolStripMenuItem.Checked == false)
                                {
                                    if (useCoupleFiles == true)
                                    {
                                        double cp = m_couplerReader[i].getPower(m_frequency);
                                        powerValue = powerValue - cp;
                                    }
                                    else
                                    {
                                        powerValue = powerValue - m_coupleStatic;
                                    }
                                }
                                lbl[i].Text = powerValue.ToString("0.0000");                                
                            }
                        }
                    }
                }
                //Thread.Sleep(100);
            }
            for (int i = 0; i < 4; i++)
            {
                if (m_powerMeter[i] != null)
                {
                    m_powerMeter[i].Close();
                }
            }
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            PowerMeterSelect[] pmSelect;

            m_powerForm = new PowerMeterForm();
            for (int i = 0; i < m_sensorsName.Length; i++)
                m_powerForm.Add(m_sensorsName[i]);
            m_powerForm.ShowDialog();
            if (m_powerForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                pmSelect = m_powerForm.getSelection();
                AllocatePowerMeterResources(pmSelect);
                SaveAssigments(pmSelect);
            }
             
        }

        bool AllocatePowerMeterResources(PowerMeterSelect[] pmSelect)
        {
            Label[] lbl = { label1, label2, label3, label4 };

            lock (this)
            {
                string str = string.Empty;
                for (int i = 0; i < 4; i++)
                {
                    lbl[i].Text = "0";
                    if (m_powerMeter[i] != null)
                        m_powerMeter[i].Close();
                    m_powerMeter[i] = null;
                    if (pmSelect[i].ampId != -1)
                    {
                        str += "Channel " + (i + 1) + " assign to " + pmSelect[i].sensorName + Environment.NewLine;
                        label7.Text = str;

                        m_powerMeter[i] = new NRP_Z211PowerMeter();
                        NRP_Z211PowerMeter m_nrp = (NRP_Z211PowerMeter)m_powerMeter[i];
                        m_nrp.Mode = NRP_Z211_Modes.NONBURSTED;
                        if (m_powerMeter[i].Initialize(pmSelect[i].sensorName) == true)
                        {
                        }
                    }
                }
                if (m_PowerMeterThread == null)
                {
                    m_PowerMeterThread = new Thread(new ThreadStart(PowerMeterThreadProcess));
                    m_PowerMeterThread.Priority = ThreadPriority.Highest;
                    m_PowerMeterThread.Start();
                }
                else
                    if (m_PowerMeterThread.IsAlive == false)
                    {
                        m_PowerMeterThread.Start();
                    }

            }
            return true;

        }

        private int FillRodeSwartzPowerMeterList()
        {
           try
           {
              int c = NRP_Z211PowerMeter.GetSensorCount();
              if (c == 0)
              {
                 return c;
              }
              m_sensorsName = new string[c];

              string SensorType = string.Empty;
              string SensorName = string.Empty;
              string SensorSerial = string.Empty;
              for (int i = 1; i < (c + 1); i++)
              {
                 NRP_Z211PowerMeter.GetSensorInfo(i,
                                      out SensorType,
                                      out SensorName,
                                      out SensorSerial);

                 m_sensorsName[i - 1] = SensorName;
              }
              return c;
           }
           catch (Exception err)
           {
              throw (new SystemException(err.Message));
           }
        }

        private void MeasurementAssistance_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            m_formActive = false;
            while (m_PowerMeterThread != null && m_PowerMeterThread.IsAlive == true)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }
        void SaveAssigments(PowerMeterSelect[] pmSelect)
        {
            Properties.Settings.Default.PowerMeterID_1 = pmSelect[0].sensorName;
            Properties.Settings.Default.PowerMeterID_2 = pmSelect[1].sensorName;
            Properties.Settings.Default.PowerMeterID_3 = pmSelect[2].sensorName;
            Properties.Settings.Default.PowerMeterID_4 = pmSelect[3].sensorName;
            Properties.Settings.Default.PowerMeterID_1_ToAmp = pmSelect[0].ampId;
            Properties.Settings.Default.PowerMeterID_2_ToAmp = pmSelect[1].ampId;
            Properties.Settings.Default.PowerMeterID_3_ToAmp = pmSelect[2].ampId;
            Properties.Settings.Default.PowerMeterID_4_ToAmp = pmSelect[3].ampId;
            Properties.Settings.Default.Save();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            PowerMeterSelect[] pmSelect = new PowerMeterSelect[4];
            for (int i = 0; i < 4; i++)
                pmSelect[i].ampId = -1;
            LoadAssigments(ref pmSelect);
            AllocatePowerMeterResources(pmSelect);
        }

        void LoadAssigments(ref PowerMeterSelect[] pmSelect)
        {

            pmSelect[0].sensorName = Properties.Settings.Default.PowerMeterID_1;
            pmSelect[1].sensorName = Properties.Settings.Default.PowerMeterID_2;
            pmSelect[2].sensorName = Properties.Settings.Default.PowerMeterID_3;
            pmSelect[3].sensorName = Properties.Settings.Default.PowerMeterID_4;
            pmSelect[0].ampId = Properties.Settings.Default.PowerMeterID_1_ToAmp;
            pmSelect[1].ampId = Properties.Settings.Default.PowerMeterID_2_ToAmp;
            pmSelect[2].ampId = Properties.Settings.Default.PowerMeterID_3_ToAmp;
            pmSelect[3].ampId = Properties.Settings.Default.PowerMeterID_4_ToAmp;
        }

        private void refreshPowerMeterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FillRodeSwartzPowerMeterList() == 0)
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alwaysOnTopToolStripMenuItem.Checked = !alwaysOnTopToolStripMenuItem.Checked;
            TopMost = alwaysOnTopToolStripMenuItem.Checked;
        }

        private void sizeableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sizeableToolStripMenuItem.Checked = !sizeableToolStripMenuItem.Checked;
            if (sizeableToolStripMenuItem.Checked == true)
            {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            else
            {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            }
        }

        private void noCouplerToolStripMenuItem_Click(object sender, EventArgs e)
        {
           noCouplerToolStripMenuItem.Checked = !noCouplerToolStripMenuItem.Checked;
        }
    }
}
