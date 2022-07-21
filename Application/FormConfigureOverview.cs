using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodePlex.JPMikkers.TFTP;
using System.Reflection;
using System.IO;

namespace TFTPServerApp
{
    public partial class FormConfigureOverview : Form
    {
        private string _configurationPath;
        private TFTPServerConfigurationList _configurationList;

        public FormConfigureOverview(string configurationPath)
        {
            InitializeComponent();
            _configurationPath = configurationPath;
            _configurationList = TFTPServerConfigurationList.Read(_configurationPath);
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _configurationList;
            UpdateButtonStatus();
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dataGridView1.Rows[e.RowIndex].DataBoundItem != null) &&
                (dataGridView1.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
            {
                e.Value = BindProperty(dataGridView1.Rows[e.RowIndex].DataBoundItem,
                       dataGridView1.Columns[e.ColumnIndex].DataPropertyName);
            }
            // modify row selection color to LightBlue:
            e.CellStyle.SelectionBackColor = Color.LightBlue;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        }

        private string BindProperty(object property, string propertyName)
        {
            string retValue = "";

            if (propertyName.Contains("."))
            {
                PropertyInfo[] arrayProperties;
                string leftPropertyName;

                leftPropertyName = propertyName.Substring(0, propertyName.IndexOf("."));
                arrayProperties = property.GetType().GetProperties();

                foreach (PropertyInfo propertyInfo in arrayProperties)
                {
                    if (propertyInfo.Name == leftPropertyName)
                    {
                        retValue = BindProperty(
                          propertyInfo.GetValue(property, null),
                          propertyName.Substring(propertyName.IndexOf(".") + 1));
                        break;
                    }
                }
            }
            else
            {
                Type propertyType;
                PropertyInfo propertyInfo;

                propertyType = property.GetType();
                propertyInfo = propertyType.GetProperty(propertyName);
                retValue = propertyInfo.GetValue(property, null).ToString();
            }

            return retValue;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            TFTPServerConfiguration result = EditConfiguration(new TFTPServerConfiguration());
            if (result != null)
            {
                _configurationList.Add(result);
                SelectRow(_configurationList.Count - 1);
                UpdateButtonStatus();
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var rowsToRemove = new List<int>( dataGridView1.SelectedRows.Cast<DataGridViewRow>().OrderByDescending(x => x.Index).Select(x => x.Index) );
            int currentIndex = dataGridView1.CurrentRow.Index;

            foreach (int x in rowsToRemove)
            {
                if (currentIndex == x && currentIndex>0)
                {
                    currentIndex--;
                    SelectRow(currentIndex);
                }
                
                _configurationList.RemoveAt(x);
            }

            UpdateButtonStatus();
        }

        private void UpdateButtonStatus()
        {
            buttonRemove.Enabled = dataGridView1.Rows.Count > 0;
            buttonEdit.Enabled = dataGridView1.SelectedRows.Count > 0;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStatus();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            EditConfiguration(dataGridView1.CurrentRow.Index);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            EditConfiguration(e.RowIndex);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _configurationList.Write(_configurationPath);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex] == ColumnRootPath)
            {
                System.Diagnostics.Process.Start(_configurationList[e.RowIndex].RootPath);
            }
        }

        private void SelectRow(int index)
        {
            if (index >= 0 && index < dataGridView1.Rows.Count)
            {
                dataGridView1.ClearSelection();
                DataGridViewRow rowToSelect = dataGridView1.Rows[index];
                rowToSelect.Selected = true;
                rowToSelect.Cells[0].Selected = true;
                dataGridView1.CurrentCell = rowToSelect.Cells[0];
            }
        }

        private void EditConfiguration(int index)
        {
            if (index >= 0 && index < _configurationList.Count)
            {
                TFTPServerConfiguration result = EditConfiguration(_configurationList[index]);
                if (result != null)
                {
                    _configurationList.Insert(index,result);
                    _configurationList.RemoveAt(index + 1);
                }
            }
        }

        private TFTPServerConfiguration EditConfiguration(TFTPServerConfiguration input)
        {
            FormSettings f = new FormSettings();
            f.Configuration = input;

            DialogResult dialogResult = f.ShowDialog(this);
            while(dialogResult == DialogResult.OK && 
                _configurationList.Any(x => (x!=input && x.EndPoint.Address == f.Configuration.EndPoint.Address && x.EndPoint.Port == f.Configuration.EndPoint.Port)))
            {
                MessageBox.Show($"There already is a configuration for address {f.Configuration.EndPoint.Address}, port {f.Configuration.EndPoint.Port}.\r\nPlease select another endpoint.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dialogResult = f.ShowDialog(this);
            }
            return dialogResult == DialogResult.OK ? f.Configuration : null;
        }
    }
}
