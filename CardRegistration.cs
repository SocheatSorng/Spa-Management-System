using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Spa_Management_System
{
    public partial class CardRegistration : Form
    {
        private ICardDataAccess _cardAccess;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public CardRegistration()
        {
            InitializeComponent();

            // Add the same dragging capability to the top panel
            //bunifuPanel2.MouseDown += (s, e) =>
            //{
            //    if (e.Button == MouseButtons.Left)
            //    {
            //        ReleaseCapture();
            //        SendMessage(Handle, 0xA1, 0x2, 0);
            //    }
            //};

            // Initialize using the proxy implementation with logging enabled
            _cardAccess = new CardProtectionProxy(useTransactionLogging: true);
            LoadCards();
            WireUpEvents();
        }

        // Load cards into the DataGridView
        private void LoadCards()
        {
            DataTable cardsTable = _cardAccess.GetAllCards();
            dgvCards.DataSource = cardsTable;
        }

        // Wire up event handlers
        private void WireUpEvents()
        {
            //btnRegisterSingle.Click += BtnRegisterSingle_Click;
            btnRegisterBatch.Click += BtnRegisterBatch_Click;
            btnSetDamaged.Click += BtnSetDamaged_Click;
            btnExitProgram.Click += BtnExitProgram_Click;
            dgvCards.CellClick += DgvCards_CellClick;
        }

        // Register a single card
        private void BtnRegisterSingle_Click(object sender, EventArgs e)
        {
            try
            {
                string cardId = txtCardId.Text.Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    MessageBox.Show("Please enter a card ID.");
                    return;
                }

                _cardAccess.RegisterCard(cardId);
                LoadCards();
                MessageBox.Show("Card registered successfully.");
                txtCardId.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering card: {ex.Message}");
            }
        }

        // Register a batch of cards
        private void BtnRegisterBatch_Click(object sender, EventArgs e)
        {
            try
            {
                string prefix = txtPrefix.Text.Trim();
                if (string.IsNullOrEmpty(prefix))
                {
                    MessageBox.Show("Please enter a prefix.");
                    return;
                }

                if (!int.TryParse(txtStartNumber.Text, out int startNumber) || startNumber < 0)
                {
                    MessageBox.Show("Please enter a valid starting number.");
                    return;
                }

                if (!int.TryParse(txtCount.Text, out int count) || count <= 0 || count > 1000)
                {
                    MessageBox.Show("Please enter a count between 1 and 1000.");
                    return;
                }

                _cardAccess.RegisterCardBatch(prefix, startNumber, count);
                LoadCards();
                MessageBox.Show($"{count} cards registered successfully.");
                txtPrefix.Clear();
                txtStartNumber.Clear();
                txtCount.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering cards: {ex.Message}");
            }
        }

        // Set a card as damaged
        private void BtnSetDamaged_Click(object sender, EventArgs e)
        {
            try
            {
                string cardId = txtCardId.Text.Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    MessageBox.Show("Please select a card or enter a card ID.");
                    return;
                }

                _cardAccess.SetCardAsDamaged(cardId);
                LoadCards();
                MessageBox.Show("Card marked as damaged.");
                txtCardId.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking card as damaged: {ex.Message}");
            }
        }

        // Display the clicked card's ID in the textbox
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
}
