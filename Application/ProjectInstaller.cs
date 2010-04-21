﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Diagnostics;


namespace TFTPServerApp
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            System.Diagnostics.Trace.WriteLine("Creating TFTP service log");

            try
            {
                if (!EventLog.SourceExists(Program.CustomEventSource))
                {
                    EventLog.CreateEventSource(Program.CustomEventSource, Program.CustomEventLog);
                }
                // write something to the event log, or else the EventLog component in the UI
                // won't fire the updating events. I know, it sucks.
                EventLog tmp = new EventLog(Program.CustomEventLog, ".", Program.CustomEventSource);
                tmp.WriteEntry("Installation complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Exception: {0}", ex));
            }

            Context.Parameters["assemblypath"] += "\" \"/service";
            base.OnBeforeInstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            Context.Parameters["assemblypath"] += "\" \"/service";
            base.OnBeforeUninstall(savedState);
        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            System.Diagnostics.Trace.WriteLine("Removing TFTP service log");

            try
            {
                if (EventLog.SourceExists(Program.CustomEventSource))
                {
                    EventLog.DeleteEventSource(Program.CustomEventSource);
                }

                if(EventLog.Exists(Program.CustomEventLog))
                {
                    EventLog.Delete(Program.CustomEventLog);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Exception: {0}", ex));
            }

            base.OnAfterUninstall(savedState);
        }
    }
}