using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AesEncryptionAppWinForms
{
    /// <summary>
    /// Main WinForms UI for AES file encryption/decryption.
    /// Manages key/IV lifecycle, file selection, and basic key storage.
    /// </summary>
    public partial class MainForm : Form
    {
        // NOTE: For AES-256 use 32-byte key; IV must be 16 bytes (AES block size).
        // SECURITY NOTE: Reusing the same IV across multiple encryptions with the same key is NOT recommended.
        // Consider generating a fresh random IV per file and storing it alongside the ciphertext.
        private byte[] Key = new byte[32];  // Key buffer (32 bytes for AES-256)
        private byte[] IV = new byte[16];  // IV buffer (16 bytes for AES block size)

        /// <summary>
        /// Initializes the form and generates a fresh key and IV on startup.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            RegenerateKeyAndIV();  // Generate key and IV at application start
        }

        /// <summary>
        /// Click handler for the "Encrypt" button. Prompts user to pick an input file and writes an "encrypted_" copy.
        /// </summary>
        private void EncryptButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Allow selecting any file type.
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inputFile = openFileDialog.FileName;
                string outputFile = Path.Combine(Path.GetDirectoryName(inputFile), "encrypted_" + Path.GetFileName(inputFile));

                try
                {
                    EncryptFile(inputFile, outputFile);
                    OutputTextBox.Text = $"File encrypted successfully: {outputFile}";
                }
                catch (Exception ex)
                {
                    OutputTextBox.Text = $"Error while encrypting file: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Click handler for the "Decrypt" button. Prompts user to pick an input file and writes a "decrypted_" copy.
        /// </summary>
        private void DecryptButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Allow selecting any file type.
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inputFile = openFileDialog.FileName;
                string outputFile = Path.Combine(Path.GetDirectoryName(inputFile), "decrypted_" + Path.GetFileName(inputFile));

                try
                {
                    DecryptFile(inputFile, outputFile);
                    OutputTextBox.Text = $"File decrypted successfully: {outputFile}";
                }
                catch (Exception ex)
                {
                    OutputTextBox.Text = $"Error while decrypting file: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Click handler for the "Regenerate Key/IV" button. Creates a new random key and IV.
        /// </summary>
        private void RegenerateKeyIVButton_Click(object sender, EventArgs e)
        {
            RegenerateKeyAndIV();  // Generate a new Key and IV
            OutputTextBox.Text = $"Encryption settings regenerated.\nNew Key: {Convert.ToBase64String(Key)}\nNew IV: {Convert.ToBase64String(IV)}";
            UpdateKeyIVTextBox();  // Update UI with the new Key and IV
        }

        /// <summary>
        /// Click handler for the "Save Key/IV" button. Derives a key from a user password and writes Key+IV to an encrypted .key file.
        /// </summary>
        private void SaveKeyIVButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Key File|*.key"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string password = PromptPassword("Enter a password to protect the key:");
                if (password != null)
                {
                    try
                    {
                        SaveKeyAndIV(saveFileDialog.FileName, password);
                        OutputTextBox.Text = $"Key and IV saved successfully to: {saveFileDialog.FileName}";
                    }
                    catch (Exception ex)
                    {
                        OutputTextBox.Text = $"Error while saving key/IV: {ex.Message}";
                    }
                }
            }
        }

        /// <summary>
        /// Click handler for the "Load Key/IV" button. Reads and decrypts the .key file using a user password.
        /// </summary>
        private void LoadKeyIVButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Key File|*.key"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string password = PromptPassword("Enter the password to recover the key:");
                if (password != null)
                {
                    try
                    {
                        LoadKeyAndIV(openFileDialog.FileName, password);
                        OutputTextBox.Text = $"Key and IV loaded successfully from: {openFileDialog.FileName}";
                        UpdateKeyIVTextBox();  // Update UI with the recovered Key and IV
                    }
                    catch (Exception ex)
                    {
                        OutputTextBox.Text = $"Error while loading key/IV: {ex.Message}";
                    }
                }
            }
        }

        /// <summary>
        /// Generates fresh random bytes for Key and IV.
        /// </summary>
        private void RegenerateKeyAndIV()
        {
            // RNGCryptoServiceProvider is legacy but still secure; RandomNumberGenerator.Create() is the modern alternative.
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(Key);  // Fill key with secure random bytes
                rng.GetBytes(IV);   // Fill IV with secure random bytes
            }
        }

        /// <summary>
        /// Updates the UI textbox to display the current Key and IV in Base64.
        /// </summary>
        private void UpdateKeyIVTextBox()
        {
            KeyIVTextBox.Text = $"Key: {Convert.ToBase64String(Key)}\nIV: {Convert.ToBase64String(IV)}";
        }

        /// <summary>
        /// Encrypts a file using the current in-memory Key and IV, writing ciphertext to outputFile.
        /// </summary>
        /// <param name="inputFile">Path to the plaintext input file.</param>
        /// <param name="outputFile">Path where the encrypted file will be written.</param>
        private void EncryptFile(string inputFile, string outputFile)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    // Default: CBC + PKCS7 in .NET
                    aes.Key = Key;
                    aes.IV = IV;

                    using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (FileStream fsEncrypted = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        // Create encryptor and wrap output stream with CryptoStream (write mode).
                        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                        using (CryptoStream csEncrypt = new CryptoStream(fsEncrypted, encryptor, CryptoStreamMode.Write))
                        {
                            // Stream copy for large files; CryptoStream handles chunking + padding.
                            fsInput.CopyTo(csEncrypt);
                        }
                    }
                }

                MessageBox.Show("Encryption completed successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while encrypting the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Decrypts a file using the current in-memory Key and IV, writing plaintext to outputFile.
        /// </summary>
        /// <param name="inputFile">Path to the encrypted input file.</param>
        /// <param name="outputFile">Path where the decrypted file will be written.</param>
        private void DecryptFile(string inputFile, string outputFile)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    // Must match the mode/padding/parameters used for encryption.
                    aes.Key = Key;
                    aes.IV = IV;

                    using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (FileStream fsDecrypted = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        // Create decryptor and wrap input stream with CryptoStream (read mode).
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                        using (CryptoStream csDecrypt = new CryptoStream(fsInput, decryptor, CryptoStreamMode.Read))
                        {
                            csDecrypt.CopyTo(fsDecrypted);
                        }
                    }
                }

                MessageBox.Show("Decryption completed successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while decrypting the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Encrypts and saves the current in-memory Key and IV to an external ".key" file using a password-derived key.
        /// </summary>
        /// <param name="filePath">Destination path for the .key file.</param>
        /// <param name="password">User-supplied password to protect the stored Key/IV.</param>
        private void SaveKeyAndIV(string filePath, string password)
        {
            using (Aes aes = Aes.Create())
            {
                // Generate random salt and derive encryption key from password using PBKDF2.
                byte[] salt = GenerateSalt();
                // NOTE: 1000 iterations is minimal; consider increasing (e.g., 100k+) in production.
                var key = new Rfc2898DeriveBytes(password, salt, 1000);

                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    // Store salt in clear at the beginning of the file (needed for later derivation).
                    fs.Write(salt, 0, salt.Length);

                    using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Write Key then IV to the encrypted stream.
                        cs.Write(Key, 0, Key.Length);
                        cs.Write(IV, 0, IV.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Loads and decrypts the Key and IV from a ".key" file using the provided password.
        /// </summary>
        /// <param name="filePath">Path to the .key file.</param>
        /// <param name="password">Password used to derive the decryption key.</param>
        private void LoadKeyAndIV(string filePath, string password)
        {
            using (Aes aes = Aes.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Read the salt (stored in clear).
                byte[] salt = new byte[16];
                int readSalt = fs.Read(salt, 0, salt.Length);
                if (readSalt != salt.Length)
                    throw new InvalidOperationException("Invalid key file: salt is missing or truncated.");

                // Re-derive the same key/IV from password and salt.
                var key = new Rfc2898DeriveBytes(password, salt, 1000);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                using (CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    // Read back Key and IV in the same order they were written.
                    int readKey = cs.Read(Key, 0, Key.Length);
                    if (readKey != Key.Length)
                        throw new InvalidOperationException("Invalid key file: key length mismatch.");

                    int readIv = cs.Read(IV, 0, IV.Length);
                    if (readIv != IV.Length)
                        throw new InvalidOperationException("Invalid key file: IV length mismatch.");
                }
            }
        }

        /// <summary>
        /// Creates a new 16-byte random salt for PBKDF2.
        /// </summary>
        /// <returns>Random salt bytes.</returns>
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Prompts the user for a password using a small modal dialog.
        /// </summary>
        /// <param name="prompt">Prompt text to display to the user.</param>
        /// <returns>The entered password, or null if the dialog was canceled.</returns>
        private string PromptPassword(string prompt)
        {
            Form promptForm = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Password",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label promptLabel = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240, UseSystemPasswordChar = true };
            Button confirmation = new Button() { Text = "OK", Left = 110, Width = 80, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { promptForm.Close(); };
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.Controls.Add(promptLabel);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
