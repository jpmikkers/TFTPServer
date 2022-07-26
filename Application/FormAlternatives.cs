using CodePlex.JPMikkers.TFTP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace TFTPServerApp
{
    public partial class FormAlternatives : Form
    {
        private readonly List<ConfigurationAlternative> _configuration = new List<ConfigurationAlternative>();

        public BindingList<ConfigurationAlternative> Configuration
        {
            get
            {
                BindingList<ConfigurationAlternative> result = new BindingList<ConfigurationAlternative>();
                foreach(var t in _configuration)
                {
                    result.Add(t.Clone());
                }
                return result;
            }
            set
            {
                _configuration.Clear();
                foreach(var t in value)
                {
                    _configuration.Add(t.Clone());
                }
                Bind();
                comboBoxSelectAlternative.SelectedIndex = Math.Min(0, _configuration.Count - 1);
            }
        }

        public FormAlternatives()
        {
            InitializeComponent();
        }

        private void Bind()
        {
            if(_configuration.Count > 0)
            {
                comboBoxSelectAlternative.Items.Clear();
                comboBoxSelectAlternative.Enabled = true;
                buttonDeleteAlternative.Enabled = true;
                groupBox1.Enabled = true;

                for(int t = 0; t < _configuration.Count; t++)
                {
                    comboBoxSelectAlternative.Items.Add($"Alternative {t + 1}");
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

            if(comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < _configuration.Count)
            {
                ConfigurationAlternative alternative = _configuration[comboBoxSelectAlternative.SelectedIndex];
                groupBox1.Text = $"Alternative {comboBoxSelectAlternative.SelectedIndex + 1}";
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
            _configuration.Add(new ConfigurationAlternative());
            Bind();
            comboBoxSelectAlternative.SelectedIndex = _configuration.Count - 1;
        }

        private void comboBoxSelectAlternative_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindDetails();
        }

        private void buttonDeleteAlternative_Click(object sender, EventArgs e)
        {
            int index = comboBoxSelectAlternative.SelectedIndex;

            if(index >= 0 && index < _configuration.Count)
            {
                _configuration.RemoveAt(index);
                Bind();
                comboBoxSelectAlternative.SelectedIndex = Math.Min(index, _configuration.Count - 1);
            }
        }

        private void comboBoxFilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < _configuration.Count)
            {
                ConfigurationAlternative alternative = _configuration[comboBoxSelectAlternative.SelectedIndex];
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
            if(ushort.TryParse(textBoxWindowSize.Text, out value))
            {
                if(value > 0 && value <= 32) e.Cancel = false;
            }

            if(e.Cancel)
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

                if(comboBoxSelectAlternative.SelectedIndex >= 0 && comboBoxSelectAlternative.SelectedIndex < _configuration.Count)
                {
                    ConfigurationAlternative alternative = _configuration[comboBoxSelectAlternative.SelectedIndex];
                    TFTPServer.ConfigurationAlternative filter = alternative.IsRegularExpression ? TFTPServer.ConfigurationAlternative.CreateRegex(alternative.Filter) : TFTPServer.ConfigurationAlternative.CreateWildcard(alternative.Filter);
                    if(filter.Match(textBoxTestString.Text))
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
            catch
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
