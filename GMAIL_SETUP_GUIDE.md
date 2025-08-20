# 📧 Gmail SMTP Setup Guide for OTP Verification

## 🚨 The Problem
Your `appsettings.json` currently has placeholder values:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",    // ❌ Placeholder
  "SenderName": "PharmaFlow",
  "SmtpUsername": "your-email@gmail.com",   // ❌ Placeholder  
  "SmtpPassword": "your-app-password"       // ❌ Placeholder
}
```

## 🔧 Solution 1: Gmail SMTP (Recommended)

### Step 1: Enable 2-Factor Authentication
1. Go to your Google Account settings: https://myaccount.google.com/
2. Click on "Security" in the left sidebar
3. Under "Signing in to Google", click "2-Step Verification"
4. Follow the steps to enable 2FA if not already enabled

### Step 2: Generate App Password
1. Go to Google Account settings: https://myaccount.google.com/
2. Click "Security" → "2-Step Verification"
3. Scroll down and click "App passwords"
4. Select "Mail" and "Windows Computer" (or Other)
5. Name it "PharmaFlow" or "OTP Service"
6. Copy the 16-character password (e.g., `abcd efgh ijkl mnop`)

### Step 3: Update appsettings.json
Replace the EmailSettings with your real Gmail credentials:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "youractualemail@gmail.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "youractualemail@gmail.com",
    "SmtpPassword": "abcd efgh ijkl mnop"
  }
}
```

**⚠️ Important:** 
- Use your **actual Gmail address**
- Use the **16-character app password** (not your regular Gmail password)
- Remove spaces from the app password or keep them (both work)

## 🔧 Solution 2: Outlook/Hotmail SMTP

If you prefer Outlook/Hotmail:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SenderEmail": "youremail@outlook.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "youremail@outlook.com",
    "SmtpPassword": "your-outlook-password"
  }
}
```

## 🔧 Solution 3: Custom SMTP Provider

For production, consider professional email services:

### SendGrid (Recommended for Production)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "apikey",
    "SmtpPassword": "your-sendgrid-api-key"
  }
}
```

### Mailgun
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailgun.org",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com", 
    "SenderName": "PharmaFlow",
    "SmtpUsername": "postmaster@yourdomain.com",
    "SmtpPassword": "your-mailgun-password"
  }
}
```

## 🧪 Testing Your Configuration

### Method 1: Test Registration
1. Update your `appsettings.json` with real credentials
2. Restart your application
3. Try registering with a real email address
4. Check if you receive the OTP code

### Method 2: Add Logging for Debugging
Add this to your `appsettings.json` to see detailed email errors:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Pharmaflow7.Services.EmailService": "Debug",
      "Pharmaflow7.Services.OtpService": "Debug"
    }
  }
}
```

## 🚨 Common Issues & Solutions

### Issue 1: "Authentication failed"
**Solution:** Make sure you're using an App Password, not your regular Gmail password.

### Issue 2: "SMTP server requires a secure connection"
**Solution:** Ensure SmtpPort is 587 and EnableSsl is true in the EmailService.

### Issue 3: "The SMTP server requires a secure connection or the client was not authenticated"
**Solutions:**
- Enable 2-Factor Authentication on Gmail
- Use App Password instead of regular password
- Check if "Less secure app access" is enabled (not recommended)

### Issue 4: "Mailbox unavailable"
**Solution:** Make sure the sender email exists and is correctly spelled.

## 🔐 Security Best Practices

### For Development:
- Use App Passwords for Gmail
- Don't commit real credentials to Git
- Use environment variables or User Secrets

### For Production:
- Use professional email services (SendGrid, Mailgun, AWS SES)
- Store credentials in secure configuration (Azure Key Vault, AWS Secrets Manager)
- Use environment variables
- Enable email logging and monitoring

## 📝 Quick Fix Checklist

- [ ] Enable 2-Factor Authentication on Gmail
- [ ] Generate App Password for Gmail
- [ ] Replace placeholder values in appsettings.json
- [ ] Use your actual email address
- [ ] Use the 16-character App Password
- [ ] Restart the application
- [ ] Test registration with real email
- [ ] Check spam folder for OTP emails

## 🆘 If Still Not Working

1. **Check Application Logs:** Look for detailed error messages
2. **Test SMTP Connection:** Use a tool like Telnet or SMTP tester
3. **Try Different Email Provider:** Test with Outlook or Yahoo
4. **Contact Email Provider:** Some providers block SMTP access
5. **Use Professional Service:** Consider SendGrid or Mailgun for production

Remember: The most common issue is using placeholder credentials instead of real ones!