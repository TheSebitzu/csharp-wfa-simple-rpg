using Engine;
using System;
using System.Windows.Forms;

namespace SimpleRPG
{
    public partial class TradingScreen : Form
    {
        private Player _currentPlayer;

        public TradingScreen(Player player)
        {
            _currentPlayer = player;
            InitializeComponent();

            // Style the dgv
            DataGridViewCellStyle rightAlignedCellStyle = new DataGridViewCellStyle();
            rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Inventory
            dgvMyItems.RowHeadersVisible = false;
            dgvMyItems.AutoGenerateColumns = false;

            // Hidden column with id
            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemId",
                Visible = false
            });
            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = "Description"
            });
            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Qty",
                Width = 30,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Quantity"
            });
            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Price"
            });
            dgvMyItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Sell 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = "ItemId"
            });

            // Bind inventory to dgvMyItems
            dgvMyItems.DataSource = _currentPlayer.Inventory;
            dgvMyItems.CellClick += dgvMyItems_CellClick;

            // Vendor inventory
            dgvVendorItems.RowHeadersVisible = false;
            dgvVendorItems.AutoGenerateColumns = false;
            
            // Hidden column with id
            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemId",
                Visible = false
            });
            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = "Description"
            });
            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Price"
            });
            dgvVendorItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Buy 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = "ItemId"
            });

            // Bind vendor inventory to dgv
            dgvVendorItems.DataSource = _currentPlayer.CurrentLocation.VendorWorkingHere.Inventory;
            dgvVendorItems.CellClick += dgvVendorItems_CellClick;

        }
        private void dgvMyItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // We have 5 elements 0-based
            // So index 4 is last column
            if (e.ColumnIndex == 4)
            {
                // Get the value of the item in the value column
                var itemId = dgvMyItems.Rows[e.RowIndex].Cells[0].Value;

                Item item = World.ItemByID(Convert.ToInt32(itemId));

                if (item.Price == World.UNSELLABLE_ITEM_PRICE)
                {
                    MessageBox.Show("You cannot sell the " + item.Name);
                }
                else
                {

                    _currentPlayer.RemoveItemFromInventory(item);
                    _currentPlayer.Gold += item.Price;
                }
            }
        }
        private void dgvVendorItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var itemId = dgvVendorItems.Rows[e.RowIndex].Cells[0].Value;

                Item item = World.ItemByID(Convert.ToInt32(itemId));

                if (_currentPlayer.Gold >= item.Price)
                {
                    _currentPlayer.AddItemToInventory(item);
                    _currentPlayer.Gold -= item.Price;
                }
                else
                {
                    MessageBox.Show("You do not have enough gold to buy the " + item.Name);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
