using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test2
{
    public partial class MainForm : Form
    {
        static private string connectString { set; get; }
        static private int amountOfColumns { set; get; } = 8;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            widthOfColumns(dataGridView1, -10);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            GetConnectionStrings(label1);
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();

                List<CheckBox> checkBoxes = new List<CheckBox>() { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
                int counter = 0;
                string checkedColumns = String.Empty;
                int amountOfCheckedColumns = 2;

                //визначаємо видимі колонки відносно вибраних чекбоксів
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (checkBoxes[counter].Checked && column.Name == dataGridView1.Columns[counter].Name)
                    {
                        column.Visible = true;
                        checkedColumns += dataGridView1.Columns[counter].Name + ", ";
                        amountOfCheckedColumns++;
                    }
                    else
                    {
                        column.Visible = false;
                    }
                    counter++;
                    if (counter == dataGridView1.ColumnCount - 2) { break; }
                }

                //формуємо запит відносно вибраних чекбоксів
                string query = makeQuery(checkedColumns);

                SqlCommand command = new SqlCommand(query, connection);
                List<string[]> data = readData(command);

                //додаємо в датагрід
                foreach (string[] s in data)
                {
                    dataGridView1.Rows.Add(s);

                }
                //коригуємо ширину
                widthOfColumns(dataGridView1, 0);
            }
        }

        static void GetConnectionStrings(Label label1)
        {
            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    connectString = cs.ConnectionString;
                }
            }
        }

        private void widthOfColumns(DataGridView dataGridView, int x) // x = -10
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.Width = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.DisplayedCells, true) + x;
            }
        }

        private string makeQuery(string checkedColumns)
        {
            string query = String.Empty;
            string template = "SELECT SUM(Amount), SUM(Price) from TestTable2.dbo.Shipment group by ";
            if (checkedColumns != String.Empty)
            {
                query = template.Substring(0, 7) + checkedColumns + template.Substring(6, template.Length - 6)
                    + checkedColumns.Substring(0, checkedColumns.Length - 2);
            }
            else
            {
                query = template.Replace("group by", "");
            }
            return query;
        }

        private List<string[]> readData(SqlCommand command)
        {
            List<string[]> data = new List<string[]>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    data.Add(new string[amountOfColumns]);
                    int k = 0;
                    int coef = 0;
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column.Visible == true)
                        {
                            data[data.Count - 1][k] = reader[coef].ToString();
                            coef++;
                        }
                        k++;
                    }
                }
            }
            return data;
        }

    }
}
