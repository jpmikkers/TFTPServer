using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodePlex.JPMikkers.TFTP;


namespace TFTPServerApp
{
    public partial class FormAlternatives : Form
    {
        private List<ConfigurationAlternative> m_Configuration = new List<ConfigurationAlternative>();

        public BindingList<ConfigurationAlternative> Configuration
        {
            get
            {
                BindingList<ConfigurationAlternative> result = new BindingList<ConfigurationAlternative>();
                foreach(var t in m_Configuration)
                {
                    result.Add(t.Clone());
                }
                return result;
            }
            set
            {
                m_Configuration.Clear();
                foreach (var t in value)
                {
                    m_Configuration.Add(t.Clone());
                }
                Bind();
                comboBoxSelectAlternative.SelectedIndex = Math.Min(0, m_Configuration.Count - 1);
            }
        }

        public FormAlternatives()
        {
            InitializeComponent();
        }

        private void Bind()
        {
            if (m_Configuration.Count > 0)
            {
                comboBoxSelectAlternative.Items.Clear();
                comboBoxSelectAlternative.Enabled = true;
                buttonDeleteAlternative.Enabled = true;
                groupBox1.Enabled = true;

                for (int t = 0; t < m_Configuration.Count; t++)
                {
                    comboBoxSelectAlternative.Items.Add(string.Format("Alternative {0}", t+1));
                }
            }
            else
            {
                comboBoxSelectAlternative.Items.Clear();
                comboBoxSelectAlternative.Enabled = false;
                buttonDeleteAlternative.Enabled = false;
                groupBox1.Enabled = false;
            }
        }

        private void BindDetails()
        {
            textBoxFilter.DataBindings.Clear();
            textBoxWindowSize.DataBindings.Clear();

            if (comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < m_Configuration.Count)
            {
                ConfigurationAlternative alternative = m_Configuration[comboBoxSelectAlternative.SelectedIndex];
                groupBox1.Text = string.Format("Alternative {0}", comboBoxSelectAlternative.SelectedIndex + 1);
                comboBoxFilterMode.SelectedIndex = alternative.IsRegularExpression ? 1 : 0;
                groupBox1.Enabled = true;

                var bs = new BindingSource(alternative, null);
                textBoxFilter.DataBindings.Add("Text", bs, "Filter");
                textBoxWindowSize.DataBindings.Add("Text", bs, "WindowSize");
            }
            else
            {
                groupBox1.Text = "";
                comboBoxFilterMode.SelectedIndex = -1;
                groupBox1.Enabled = false;
            }
        }

        private void buttonNewAlternative_Click(object sender, EventArgs e)
        {
            m_Configuration.Add(new ConfigurationAlternative());
            Bind();
            comboBoxSelectAlternative.SelectedIndex = m_Configuration.Count - 1;
        }

        private void comboBoxSelectAlternative_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindDetails();
        }

        private void buttonDeleteAlternative_Click(object sender, EventArgs e)
        {
            int index = comboBoxSelectAlternative.SelectedIndex;

            if (index >= 0 && index < m_Configuration.Count)
            {
                m_Configuration.RemoveAt(index);
                Bind();
                comboBoxSelectAlternative.SelectedIndex = Math.Min(index,m_Configuration.Count-1);
            }
        }

        private void comboBoxFilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < m_Configuration.Count)
            {
                ConfigurationAlternative alternative = m_Configuration[comboBoxSelectAlternative.SelectedIndex];
                alternative.IsRegularExpression = (comboBoxFilterMode.SelectedIndex == 0) ? false : true;
            }
        }

        void textBoxWindowSize_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(textBoxWindowSize, "");
        }

        private void textBoxWindowSize_Validating(object sender, CancelEventArgs e)
        {
            ushort value;

            e.Cancel = true;
            if (ushort.TryParse(textBoxWindowSize.Text, out value))
            {
                if (value > 0 && value <= 32) e.Cancel = false;
            }

            if (e.Cancel)
            {
                this.errorProvider1.SetError(textBoxWindowSize, "value must be between 1 and 32 (default: 1)");
            }
        }

        private void textBoxTestString_TextChanged(object sender, EventArgs e)
        {
            TestIt();
        }

        private void TestIt()
        {
            try
            {

                if (comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < m_Configuration.Count)
                {
                    ConfigurationAlternative alternative = m_Configuration[comboBoxSelectAlternative.SelectedIndex];
                    TFTPServer.ConfigurationAlternative filter = alternative.IsRegularExpression ? TFTPServer.ConfigurationAlternative.CreateRegex(alternative.Filter) : TFTPServer.ConfigurationAlternative.CreateWildcard(alternative.Filter);
                    if (filter.Match(textBoxTestString.Text))
                    {
                        buttonMatchResult.Text = "Match!";
                        buttonMatchResult.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        buttonMatchResult.Text = "Does not match";
                        buttonMatchResult.BackColor = Color.Red;
                    }
                }
                else
                {
                    buttonMatchResult.Text = "";
                    buttonMatchResult.BackColor = SystemColors.Control;
                }
            }
            catch (Exception e)
            {
                buttonMatchResult.Text = "Invalid filter";
                buttonMatchResult.BackColor = Color.Red;
            }
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            TestIt();
        }

        private void textBoxFilter_Validated(object sender, EventArgs e)
        {
            TestIt();
        }
    }
}
