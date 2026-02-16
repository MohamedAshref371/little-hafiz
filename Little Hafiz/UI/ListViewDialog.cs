using System;
using System.Drawing;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class ListViewDialog : Form
    {
        public int SelectedIndex { get; private set; } = -1;

        public ListViewDialog(string title, string[] data, bool clickable = true)
        {
            if (data is null) Close();
            InitializeComponent();

            if (data.Length >= 18)
            {
                ClientSize = new Size(ClientSize.Width + 20, ClientSize.Height);
                listView1.ClientSize = new Size(listView1.ClientSize.Width + 20, listView1.ClientSize.Height);
            }

            listView1.Columns.Add(title, listView1.ClientSize.Width - 1, HorizontalAlignment.Center);

            ListViewItem item;
            for (int i = 0; i < data.Length; i++)
                listView1.Items.Add(new ListViewItem(data[i]));

            if (clickable) listView1.DoubleClick += (s, e) => ConfirmSelection();

            if (data.Length <= 1) return;
            item = new ListViewItem("عدد الأسطر :   " + data.Length.ToString()) { BackColor = Color.FromArgb(220, 255, 220) };
            listView1.Items.Add(item);
            listView1.ItemSelectionChanged += (s, e) =>
            {
                if (e.Item == item && e.IsSelected)
                    e.Item.Selected = false;
            };
        }

        public ListViewDialog(string title, FieldData[] data, bool clickable = true)
        {
            if (data is null) Close();
            InitializeComponent();

            if (data.Length >= 18)
            {
                ClientSize = new Size(ClientSize.Width + 20, ClientSize.Height);
                listView1.ClientSize = new Size(listView1.ClientSize.Width + 20, listView1.ClientSize.Height);
            }

            listView1.Columns.Add(title, 320, HorizontalAlignment.Center);
            listView1.Columns.Add("التكرار", listView1.ClientSize.Width - 321, HorizontalAlignment.Center);

            ListViewItem item; int total = 0;
            for (int i = 0; i < data.Length; i++)
            {
                item = new ListViewItem(data[i].Text);
                total += data[i].Count;
                item.SubItems.Add(data[i].Count.ToString());
                listView1.Items.Add(item);
            }

            if (clickable) listView1.DoubleClick += (s, e) => ConfirmSelection();

            if (data.Length <= 1) return;
            item = new ListViewItem() { BackColor = Color.FromArgb(220, 255, 220) };
            item.SubItems.Add(total.ToString());
            listView1.Items.Add(item);
            listView1.ItemSelectionChanged += (s, e) =>
            {
                if (e.Item == item && e.IsSelected)
                    e.Item.Selected = false;
            };
        }

        private void ConfirmSelection()
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                SelectedIndex = listView1.SelectedIndices[0];
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string query = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                listView1.SelectedItems.Clear();
                return;
            }

            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
                else
                    item.Selected = false;
            }
        }

    }
}
