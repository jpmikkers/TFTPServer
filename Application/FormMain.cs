/*

Copyright (c) 2010 Jean-Paul Mikkers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Security;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Reflection;


namespace TFTPServerApp
{
    public partial class FormMain : Form
    {
        private bool _hasAdministrativeRight;
        private ServiceController _service;
        private DateTime _timeFilter;
        private TransferUpdater _transferUpdater;

        public FormMain(ServiceController service)
        {
            _service = service;
            _hasAdministrativeRight = Program.HasAdministrativeRight();
            InitializeComponent();
            UpdateServiceStatus();
            timerServiceWatcher.Enabled = true;
            SetTimeFilter(DateTime.Now);
            listViewTransfers.Items.Clear();
            OverrideSetStyle(listViewTransfers,ControlStyles.AllPaintingInWmPaint | /*ControlStyles.Opaque|*/ ControlStyles.OptimizedDoubleBuffer, true);
            eventLog1.EnableRaisingEvents = true;
        }

        private static void OverrideSetStyle(Control c,ControlStyles styles,bool value)
        {
            MethodInfo info=c.GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (info != null)
            {
                info.Invoke(c, new object[] { styles, value });
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _transferUpdater = new TransferUpdater(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), OnUpdateTransfers);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _transferUpdater.Dispose();
            base.OnClosing(e);
        }

        private class ProgressTag
        {
            public enum ViewMode
            {
                None,
                Percentage,
                ProgressWalker,
            }

            public long   Id;
            public long   PreviousBytesTransferred;
            public ViewMode Mode;
            public double Fraction;
        }

        private void OnUpdateTransfers(object sender, TFTPLogEventArgs data)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<TFTPLogEventArgs>(OnUpdateTransfers), sender, data);
            }
            else
            {
                try
                {
                    var lv = this.listViewTransfers;

                    int nrOfColumns = lv.Columns.Count;

                    //lv.BeginUpdate();

                    // append new items at the front
                    while (lv.Items.Count < data.Entries.Count)
                    {
                        lv.Items.Insert(0, new ListViewItem(new string[nrOfColumns]));
                    }

                    // remove items from the back
                    while (lv.Items.Count > data.Entries.Count)
                    {
                        lv.Items.RemoveAt(lv.Items.Count - 1);
                    }

                    for (int t = 0; t < lv.Items.Count; t++)
                    {
                        ListViewItem lvItem = lv.Items[t];
                        TFTPLogEntry entry = data.Entries[t];

                        switch(entry.State)
                        {
                            case TFTPLogState.Busy:
                                lvItem.ForeColor = Color.DarkBlue;
                                break;

                            case TFTPLogState.Completed:
                                lvItem.ForeColor = Color.DarkGreen;
                                break;

                            case TFTPLogState.Stopped:
                                lvItem.ForeColor = Color.DarkRed;
                                break;
                        }

                        string[] rowAsStrings =
                            new string[]
                        {
                            entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            entry.IsUpload ? "▲" : "▼",
                            entry.Filename,
                            entry.Server,
                            $"{entry.Transferred}",
                            $"{(entry.FileLength<0 ? "?" : entry.FileLength.ToString())}",
                            ConvertSpeed(entry.Speed),
                            entry.State.ToString(),
                            entry.ErrorMessage,
                            ""
                        };

                        for (int c = 0; c < rowAsStrings.Length; c++)
                        {
                            ListViewItem.ListViewSubItem subItem = lvItem.SubItems[c];
                            if (subItem.Text != rowAsStrings[c])
                            {
                                subItem.Text = rowAsStrings[c];
                            }
                        }

                        var sub = lvItem.SubItems[rowAsStrings.Length - 1];

                        if (sub.Tag == null || ((ProgressTag)sub.Tag).Id != entry.Id)
                        {
                            sub.Tag = new ProgressTag() { Id = entry.Id, PreviousBytesTransferred = entry.Transferred };
                        }

                        ProgressTag tag = (ProgressTag)sub.Tag;

                        double fraction=GetProgressFraction(entry);

                        if (fraction >= 0.0)
                        {
                            tag.Fraction = fraction;
                            tag.Mode = ProgressTag.ViewMode.Percentage;
                        }
                        else
                        {
                            if (entry.State == TFTPLogState.Stopped)
                            {
                                tag.Mode = ProgressTag.ViewMode.None;
                            }
                            else
                            {
                                tag.Mode = ProgressTag.ViewMode.ProgressWalker;
                                if (tag.PreviousBytesTransferred != entry.Transferred)
                                {
                                    tag.Fraction = (tag.Fraction + 0.25) % 1.0;
                                    tag.PreviousBytesTransferred = entry.Transferred;
                                }
                            }
                        }
                    }
                    //lv.EndUpdate();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
        }

        private static string GetProgressPercentageAsString(TFTPLogEntry entry)
        {
            double progress = GetProgressFraction(entry);
            return progress >= 0.0 ? $"{progress:0.0} %" : "... %";
        }

        private static double GetProgressFraction(TFTPLogEntry entry)
        {
            double result;

            if (entry.State == TFTPLogState.Completed)
            {
                result = 1.0;
            }
            else if (entry.FileLength >= 0.0)
            {
                result = (1.0 * Math.Min(entry.Transferred, entry.FileLength)) / entry.FileLength;
            }
            else
            {
                result = -1.0;
            }

            return result;
        }

        private static string ConvertSpeed(double bytesPerSecond)
        {
            int[] precisions = new int[] { 0, 1, 2 };
            string[] prefixes = new string[] { "", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi" };
            int t = (Math.Abs(bytesPerSecond) < 1024.0) ? 0 : Math.Min(prefixes.Length - 1, (int)Math.Log(Math.Abs(bytesPerSecond), 1024.0));
            int precision = precisions[Math.Min(precisions.Length - 1, t)];
            return string.Format(string.Format("{{0:F{0}}} {{1}}B/s", precision), bytesPerSecond / Math.Pow(1024.0, t), prefixes[t]);
        }

        private void UpdateServiceStatus()
        {
            _service.Refresh();
            //System.Diagnostics.Debug.WriteLine(_service.Status.ToString());
            if (!Program.HasAdministrativeRight())
            {
                buttonStart.Enabled = false;
                buttonStop.Enabled = false;
                buttonConfigure.Enabled = false;
                buttonElevate.Enabled = true;
            }
            else
            {
                buttonStart.Enabled = (_service.Status == ServiceControllerStatus.Stopped);
                buttonStop.Enabled = (_service.Status == ServiceControllerStatus.Running);
                buttonConfigure.Enabled = true;
                buttonElevate.Enabled = false;
            }

            toolStripStatusLabel.Text = $"Service status: {_service.Status}";
        }

        private void StartService()
        {
            try
            {
                _service.Start();
            }
            catch (Exception)
            {
            }
            UpdateServiceStatus();
        }

        private void StopService()
        {
            try
            {
                _service.Stop();
            }
            catch (Exception)
            {
            }
            UpdateServiceStatus();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            StartService();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopService();
        }

        private void buttonElevate_Click(object sender, EventArgs e)
        {
            if (Program.RunElevated(""))
            {
                Close();
            }
        }

        private void timerServiceWatcher_Tick(object sender, EventArgs e)
        {
            UpdateServiceStatus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog(this);
        }

        private void buttonConfigure_Click(object sender, EventArgs e)
        {
            FormConfigureOverview f = new FormConfigureOverview(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"JPMikkers\\TFTP Server\\Configuration.xml"));
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                UpdateServiceStatus();
                if(_hasAdministrativeRight && _service.Status == ServiceControllerStatus.Running)
                {
                    if (MessageBox.Show("The TFTP Service has to be restarted to enable the new settings.\r\n" +
                        "This will cause any transfers in progress to be aborted.\r\n" +
                        "Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _service.Stop();
                        _service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
                        _service.Start();
                    }
                }
            }
        }

        private void eventLog1_EntryWritten(object sender, System.Diagnostics.EntryWrittenEventArgs e)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new EntryWrittenEventHandler(eventLog1_EntryWritten), sender, e);
                return;
            }
            else
            {
                AddEventLogEntry(e.Entry);
            }
        }

        private void AddEventLogEntry(EventLogEntry entry)
        {
            if (entry.TimeGenerated > _timeFilter)
            {
                string entryType;
                switch (entry.EntryType)
                {
                    case EventLogEntryType.Error:
                        entryType = "ERROR";
                        break;

                    case EventLogEntryType.Warning:
                        entryType = "WARNING";
                        break;

                    default:
                    case EventLogEntryType.Information:
                        entryType = "INFO";
                        break;
                }
                textBox1.AppendText(entry.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.fff") + " : " + entryType + " : " + entry.Message + "\r\n");
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            SetTimeFilter(DateTime.Now);
        }

        private void RebuildLog()
        {
            textBox1.Visible = false;
            //textBox1.Up
            textBox1.Clear();

            foreach (EventLogEntry entry in eventLog1.Entries)
            {
                AddEventLogEntry(entry);
            }
            textBox1.Visible = true;
        }

        private void buttonShowHistory_Click(object sender, EventArgs e)
        {
            try
            {
                SetTimeFilter(_timeFilter.AddDays(-1));
            }
            catch (Exception)
            {
            }
        }

        private void buttonHistoryOneHour_Click(object sender, EventArgs e)
        {
            try
            {
                SetTimeFilter(_timeFilter.AddHours(-1));
            }
            catch (Exception)
            {
            }
        }

        private void SetTimeFilter(DateTime filter)
        {
            _timeFilter = filter;
            if (_timeFilter == DateTime.MinValue)
            {
                labelFilter.Text = "Showing all logging";
            }
            else
            {
                labelFilter.Text = $"Showing log starting at: {filter.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
            }
            RebuildLog();
        }

        private void buttonHistoryAll_Click(object sender, EventArgs e)
        {
            SetTimeFilter(DateTime.MinValue);
        }

        private static ListViewItem TFTPLogEntryToListViewItem(TFTPLogEntry entry)
        {
            ListViewItem item = new ListViewItem(
                new string[]
                {
                    entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    entry.IsUpload ? "Up" : "Dn",
                    entry.Filename,
                    entry.Server,
                    $"{entry.Transferred} / {(entry.FileLength<0 ? "?" : entry.FileLength.ToString())}",
                    entry.State.ToString(),
                    entry.ErrorMessage
                }
            );
            return item;
        }

/*
        private void fastListView1_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            ColumnHeader c = fastListView1.Columns[e.ColumnIndex];
            if (c.Tag is string)
            {
                int minWidth;

                if (int.TryParse((string)c.Tag, out minWidth))
                {
                    if (c.Width < minWidth)
                    {
                        c.Width = minWidth;
                    }
                }
            }
        }
*/
        private void listViewTransfers_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.SubItem.BackColor = e.Item.BackColor;
            e.SubItem.ForeColor = e.Item.ForeColor;

            ProgressTag tag = e.SubItem.Tag as ProgressTag;

            Action<DrawListViewSubItemEventArgs, string> drawSubItemText = (x, text) => TextRenderer.DrawText(x.Graphics, text, x.SubItem.Font, x.Bounds, x.SubItem.ForeColor, x.SubItem.BackColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding);

            if (tag != null)
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.None;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                Rectangle r = e.SubItem.Bounds;
                r.Inflate(-1, -2);
                e.Graphics.DrawRectangle(Pens.LightGray, r);

                r.Offset(1, 1);
                r.Width = r.Width - 1;
                r.Height = r.Height - 1;
                e.Graphics.FillRectangle(Brushes.LightYellow, r);

                if (tag.Mode == ProgressTag.ViewMode.Percentage)
                {
                    var rf=RectangleF.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
                    var txt=$"{100.0 * tag.Fraction:0.0} %";

                    Rectangle part1 = Rectangle.FromLTRB(r.Left, r.Top, r.Left + (int)(tag.Fraction * r.Width), r.Bottom);

                    e.Graphics.SetClip(part1);
                    e.Graphics.FillRectangle(Brushes.Green, part1);
                    e.Graphics.DrawString(txt, this.Font, Brushes.White, rf, sf);

                    Rectangle part2 = Rectangle.FromLTRB(r.Left + r.Width - (int)((1.0 - tag.Fraction) * r.Width), r.Top, r.Right, r.Bottom);

                    e.Graphics.SetClip(part2);
                    e.Graphics.DrawString(txt, this.Font, Brushes.Black, rf, sf);
                }
                else if (tag.Mode == ProgressTag.ViewMode.ProgressWalker)
                {
                    int partLocation1 = (int)(tag.Fraction * r.Width);
                    int partLocation2 = r.Width - (int)((0.75 - tag.Fraction) * r.Width);
                    Rectangle partRectangle = Rectangle.FromLTRB(r.Left + partLocation1, r.Top, r.Left + partLocation2, r.Bottom);
                    e.Graphics.FillRectangle(Brushes.Green, partRectangle);
                }
                else if (tag.Mode == ProgressTag.ViewMode.None)
                {
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }
        
        /*
        private void listViewTransfers_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.SubItem.BackColor = e.Item.BackColor;
            e.SubItem.ForeColor = e.Item.ForeColor;

            ProgressTag tag = e.SubItem.Tag as ProgressTag;

            Action<DrawListViewSubItemEventArgs, string> drawSubItemText = (x, text) => TextRenderer.DrawText(x.Graphics, text, x.SubItem.Font, x.Bounds, x.SubItem.ForeColor, x.SubItem.BackColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding);

            if (tag!=null)
            {
                //e.DrawBackground();

                var s1 = TextRenderer.MeasureText(e.Graphics, "100.0 %", e.SubItem.Font, e.Bounds.Size, TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding);

                Rectangle r = e.SubItem.Bounds;
                r.Inflate(-2, -2);
                r.Width = r.Width - s1.Width;
                e.Graphics.DrawRectangle(Pens.Black, r);

                r.Offset(1, 1);
                r.Width = r.Width - 1;
                r.Height = r.Height - 1;

                if(tag.Mode == ProgressTag.ViewMode.Percentage)
                {
                    r.Width = (int)(tag.Fraction * r.Width);
                    e.Graphics.FillRectangle(Brushes.Green, r);
                    drawSubItemText(e,string.Format("{0:0.0} %", 100.0 * tag.Fraction));
                }
                else if(tag.Mode == ProgressTag.ViewMode.ProgressWalker)
                {
                    int partLocation1 = (int)(tag.Fraction * r.Width);
                    int partLocation2 = r.Width - (int)((0.75 - tag.Fraction) * r.Width);
                    Rectangle partRectangle = Rectangle.FromLTRB(r.Left + partLocation1, r.Top, r.Left + partLocation2, r.Bottom);
                    e.Graphics.FillRectangle(Brushes.Green, partRectangle);
                    drawSubItemText(e, "... %");
                }
                else if (tag.Mode == ProgressTag.ViewMode.None)
                {
                    drawSubItemText(e, "... %");
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }
*/
        private void listViewTransfers_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }
    }

    public class TFTPLogEventArgs : EventArgs
    {
        public List<TFTPLogEntry> Entries { get; set; }
    }

    public class TransferUpdater : IDisposable
    {
        private ChannelFactory<ITFTPServiceContract> _pipeFactory = new ChannelFactory<ITFTPServiceContract>(
                        new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                        new EndpointAddress("net.pipe://localhost/JPMikkers/TFTPServer/Service"));

        private System.Threading.Timer _timer;
        private ITFTPServiceContract _pipeProxy;
        private EventHandler<TFTPLogEventArgs> _onUpdate;
        private readonly TimeSpan _updatePeriod;
        private readonly TimeSpan _retryPeriod;
        private volatile bool _disposed = false;

        public TransferUpdater(TimeSpan updatePeriod,TimeSpan retryPeriod, EventHandler<TFTPLogEventArgs> onUpdate)
        {
            _updatePeriod = updatePeriod;
            _retryPeriod = retryPeriod;
            _onUpdate = onUpdate;
            _timer = new System.Threading.Timer(OnTimer);
            _timer.Change(0, Timeout.Infinite);
        }

        public void Stop()
        {
        }

        private void OnTimer(object state)
        {
            if (!_disposed)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Before");

                    if (_pipeProxy == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Creating a channel");
                        _pipeProxy = _pipeFactory.CreateChannel();
                    }

                    var entries = _pipeProxy.GetLogEntries().OrderByDescending(x => x.StartTime).ToList();
                    _onUpdate(this, new TFTPLogEventArgs() { Entries = entries });
                    _timer.Change((int)_updatePeriod.TotalMilliseconds, 0);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Failed, retrying..");
                    _pipeProxy = null;
                    _timer.Change((int)_retryPeriod.TotalMilliseconds, 0);
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
