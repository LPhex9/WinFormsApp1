# 🗝️ Nexus Password Vault Pro

Nexus Password Vault este o aplicație desktop securizată, dezvoltată în **C# .NET (WinForms)**, concepută pentru a stoca, genera și gestiona parolele într-un mod eficient și privat. Aplicația utilizează standarde industriale de criptare pentru a asigura că datele tale rămân locale și protejate.

![Nexus Vault Splash](https://img.shields.io/badge/Security-AES--256-brightgreen)
![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)

---

## 🚀 Caracteristici Principale

- **Seif Criptat (Vault):** Stocare securizată pentru titlu, utilizator, parolă, URL și note.
- **Generator de Parole:** Creează parole complexe cu lungime customizabilă și caractere speciale.
- **Securitate Master:** Accesul este permis doar pe baza unei parole Master unice.
- **Analiză Securitate:** Indicator vizual în timp real pentru puterea parolei (Weak, Fair, Strong).
- **Auto-Lock:** Aplicația se blochează automat după 5 minute de inactivitate pentru a preveni accesul fizic neautorizat.
- **Clipboard Clear:** Ștergere automată a clipboard-ului după copierea unei parole.
- **Import/Export:** Suport pentru migrarea datelor prin fișiere CSV.

---

## 🛡️ Arhitectura de Securitate

Securitatea este prioritatea zero a acestui proiect. Nexus Vault implementează următoarele concepte:

1.  **Criptare Simetrică AES-256:** Toate datele din seif sunt criptate folosind algoritmul AES pe 256 biți în mod CBC.
2.  **Derivarea Cheii (PBKDF2):** Parola Master nu este stocată niciodată. În schimb, folosim **PBKDF2 cu SHA-256** și un număr de **600.000 de iterații** pentru a genera cheia de criptare.
3.  **Săruri și IV-uri unice:** Fiecare salvare generează un "Salt" și un "IV" (Vector de Inițializare) aleatorii pentru a preveni atacurile prin tabele de pre-calcul (Rainbow Tables).
4.  **Zero-Knowledge:** Datele sunt stocate exclusiv local pe computerul tău (`vault.nexus`). Nicio informație nu părăsește dispozitivul.

---

## 📂 Structura Proiectului

Proiectul respectă principiul separării responsabilităților:

- **/Models:** Conține structurile de date (`Credential`, `AppSettings`, `PasswordHistory`).
- **/Services:** Logica de business (`VaultManager` pentru fișiere, `PasswordService` pentru generator).
- **/Forms:** Interfețele grafice (`LoginForm`, `CredentialEditForm`, `SettingsForm`).
- **CryptoHelper.cs:** Motorul central de criptare și decriptare.

---

## 🛠️ Instalare și Rulare

### Cerințe:
- Visual Studio 2022 (sau mai nou)
- .NET SDK (6.0, 7.0 sau 8.0)

### Pași:
1. Clonează repository-ul:
   ```bash
   git clone https://github.com/LPhex9/WinFormsApp1.git
