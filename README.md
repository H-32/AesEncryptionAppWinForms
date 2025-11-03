# 🔐 AES Encryption App – by Hussein Ahmed Kazem (C# WinForms)

A simple and secure AES file encryption tool built with **C# WinForms**.  
It allows you to **encrypt / decrypt any file**, and manage your **Key & IV** easily — with options to **save or load** them using password-protected `.key` files.

---

## 🧩 Features

- 🔒 AES-256 encryption & decryption for any file type  
- 🔑 Regenerate random Key & IV at any time  
- 💾 Save and load Key & IV with password protection (PBKDF2 + AES)  
- 🧠 Clean UI with real-time status output  
- 💡 Built with security in mind (RNGCryptoServiceProvider / RandomNumberGenerator)

---

## 🖼️ User Interface

| Action | Description |
|--------|--------------|
| **Encrypt File** | Select a file and create an encrypted copy with prefix `encrypted_` |
| **Decrypt File** | Select an encrypted file to restore it |
| **Regenerate Key & IV** | Creates a fresh random key and IV |
| **Save Key & IV** | Stores current key and IV in a `.key` file with password |
| **Load Key & IV** | Loads the saved key and IV from a `.key` file |

---

## ⚙️ How It Works

1. The app generates a **256-bit AES key** and **128-bit IV** on startup.  
2. Files are encrypted using AES-CBC mode with PKCS7 padding.  
3. When saving keys:
   - A password is used to derive an encryption key with PBKDF2.
   - Key & IV are then encrypted and written to `.key` file.
4. When loading keys:
   - The password decrypts the `.key` file and restores Key & IV.

> ⚠️ **Note:** Each encryption operation should ideally use a unique IV for best security.

---

## 🧰 Requirements

- Windows 10 / 11  
- .NET Framework 4.7.2 or later (or .NET 6+ if retargeted)
- Visual Studio 2019 / 2022

---

## 🧑‍💻 Installation & Run

1. Clone the repository:
   ```bash
   git clone https://github.com/H-32/AesEncryptionAppWinForms.git
   ```
2. Open the solution file (`.sln`) in Visual Studio.
3. Build and run the project (`Ctrl + F5`).
4. Start encrypting/decrypting files directly from the UI.

---

## 🛡️ Security Notes

- Keys and IVs are generated using secure cryptographic RNG.
- PBKDF2 (Rfc2898DeriveBytes) is used for password-based encryption of key files.
- Do not reuse the same IV for multiple files.
- Test certificates are optional and not included in this repo.

---

## 📦 Build / Publish (Optional)

To publish a portable executable:
1. Go to **Build → Publish → Folder / ClickOnce**.
2. Uncheck signing if you don't have a code-signing certificate.
3. Run the published executable from `bin\x64\Release\app.publish\AES Encryption App.exe`.

---

## 📜 License

This project is open-source under the **MIT License** — you can freely use, modify, and distribute it.

---

## 👤 Author

**Hussein Ahmed Kazem**  
GitHub: [H-32](https://github.com/H-32)  
📧 your.email@example.com  
🕹️ Developed for educational and research purposes.
