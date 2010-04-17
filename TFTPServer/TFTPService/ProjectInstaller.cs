using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace TFTPService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            //this.serviceProcessInstaller1.
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            Context.Parameters["assemblypath"] += "\" \"/service";
            base.OnBeforeInstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            Context.Parameters["assemblypath"] += "\" \"/service";
            base.OnBeforeUninstall(savedState);
        }
    }
}
