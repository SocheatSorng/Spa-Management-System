using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace Spa_Management_System
{
    public partial class CardRegistration : Form
    {
        private readonly CardDAO _dao;
        private DataTable _cardsTable;

        public CardRegistration()
        {
            InitializeComponent();
            _dao = new CardDAO();
            LoadData();
            SetupEventHandlers();
        }

        private void LoadData()
        {
            try
            {
                _cardsTable = _dao.GetAllCards();
                dgvCards.DataSource = _cardsTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cards: " + ex.Message);
            }
        }

        private void SetupEventHandlers()
        {
            btnRegister.Click += BtnRegister_Click;
            btnRegisterBatch.Click += BtnRegisterBatch_Click;
            btnSetDamaged.Click += BtnSetDamaged_Click;
            btnClear.Click += BtnClear_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvCards.CellClick += DgvCards_CellClick;
            btnExitProgram.Click += BtnExitProgram_Click;
        }

        private void ClearFields()
        {
            txtCardId.Clear();
            txtPrefix.Clear();
            txtStartNumber.Clear();
            txtCount.Clear();
            txtCardId.Focus();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                string cardId = txtCardId.Text.Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    MessageBox.Show("Please enter a card ID.");
                    return;
                }

                _dao.RegisterCard(cardId);
                LoadData();
                ClearFields();
                MessageBox.Show("Card registered successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering card: " + ex.Message);
            }
        }

        private void BtnRegisterBatch_Click(object sender, EventArgs e)
        {
            try
            {
                string prefix = txtPrefix.Text.Trim();
                if (!int.TryParse(txtStartNumber.Text, out int startNumber))
                {
                    MessageBox.Show("Please enter a valid start number.");
                    return;
                }
                if (!int.TryParse(txtCount.Text, out int count))
                {
                    MessageBox.Show("Please enter a valid count.");
                    return;
                }

                _dao.RegisterCardBatch(prefix, startNumber, count);
                LoadData();
                ClearFields();
                MessageBox.Show($"Successfully registered {count} cards.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering cards: " + ex.Message);
            }
        }

        private void BtnSetDamaged_Click(object sender, EventArgs e)
        {
            try
            {
                string cardId = txtCardId.Text.Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    MessageBox.Show("Please select a card to mark as damaged.");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    "Are you sure you want to mark this card as damaged?",
                    "Confirm Action",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _dao.SetCardAsDamaged(cardId);
                    LoadData();
                    ClearFields();
                    MessageBox.Show("Card marked as damaged successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error marking card as damaged: " + ex.Message);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchValue = txtSearch.Text.Trim();
            if (_cardsTable != null)
            {
                _cardsTable.DefaultView.RowFilter = string.Format("CardId LIKE '%{0}%'", searchValue);
            }
        }

        private void DgvCards_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCards.Rows[e.RowIndex];
                txtCardId.Text = row.Cells["CardId"].Value.ToString();
            }
        }

        private void BtnExitProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class CardDAO
    {
        private readonly SqlConnectionManager _connectionManager;

        public CardDAO()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable GetAllCards()
        {
            string query = "SELECT CardId, Status, LastUsed, CreatedDate FROM tbCard ORDER BY CreatedDate DESC";
            return _connectionManager.ExecuteQuery(query);
        }

        public void RegisterCard(string cardId)
        {
            string query = "EXEC sp_RegisterCard @CardId";
            SqlParameter param = new SqlParameter("@CardId", cardId);
            _connectionManager.ExecuteNonQuery(query, param);
        }

        public void RegisterCardBatch(string prefix, int startNumber, int count)
        {
            string query = "EXEC sp_RegisterCardBatch @Prefix, @StartNumber, @Count";
            SqlParameter[] parameters = {
                new SqlParameter("@Prefix", prefix),
                new SqlParameter("@StartNumber", startNumber),
                new SqlParameter("@Count", count)
            };
            _connectionManager.ExecuteNonQuery(query, parameters);
        }

        public void SetCardAsDamaged(string cardId)
        {
            string query = "EXEC sp_SetCardAsDamaged @CardId";
            SqlParameter param = new SqlParameter("@CardId", cardId);
            _connectionManager.ExecuteNonQuery(query, param);
        }
    }
}
