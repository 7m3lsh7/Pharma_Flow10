# 🚨 Email Verification Code Issue - Troubleshooting Guide

## The Problem
You're getting this message: **"Registration successful, but we couldn't send the verification code. Please contact support."**

This means the user registration worked, but the email sending failed.

## 🔍 Root Cause
Your `appsettings.json` has placeholder email credentials instead of real ones:

```json
"EmailSettings": {
  "SenderEmail": "your-email@gmail.com",     // ❌ This is a placeholder
  "SmtpUsername": "your-email@gmail.com",    // ❌ This is a placeholder  
  "SmtpPassword": "your-app-password"        // ❌ This is a placeholder
}
```

## 🔧 IMMEDIATE FIX - Choose One Option:

### Option 1: Gmail Setup (Easiest)

1. **Enable 2-Factor Authentication on your Gmail account**
   - Go to https://myaccount.google.com/security
   - Enable "2-Step Verification"

2. **Generate App Password**
   - Go to https://myaccount.google.com/apppasswords
   - Select "Mail" and "Other (Custom name)"
   - Name it "PharmaFlow"
   - Copy the 16-character password (like: `abcd efgh ijkl mnop`)

3. **Update appsettings.json**
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "YOUREMAIL@gmail.com",
       "SenderName": "PharmaFlow",
       "SmtpUsername": "YOUREMAIL@gmail.com",
       "SmtpPassword": "abcd efgh ijkl mnop"
     }
   }
   ```

4. **Restart your application**

### Option 2: Outlook/Hotmail Setup

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SenderEmail": "YOUREMAIL@outlook.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "YOUREMAIL@outlook.com",
    "SmtpPassword": "YOUR_OUTLOOK_PASSWORD"
  }
}
```

### Option 3: Yahoo Mail Setup

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "SenderEmail": "YOUREMAIL@yahoo.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "YOUREMAIL@yahoo.com",
    "SmtpPassword": "YOUR_YAHOO_APP_PASSWORD"
  }
}
```

## 🧪 Testing Your Fix

### Method 1: Use Test Endpoints
I've created test endpoints for you. After updating your email settings:

1. **Test basic email sending:**
   ```
   GET http://localhost:5000/api/testemail/send-test?email=YOUR_EMAIL@gmail.com
   ```

2. **Test OTP generation:**
   ```
   GET http://localhost:5000/api/testemail/send-otp?email=YOUR_EMAIL@gmail.com
   ```

3. **Check configuration:**
   ```
   GET http://localhost:5000/api/testemail/test-config
   ```

### Method 2: Test Registration
1. Update your email settings
2. Restart the application
3. Try registering with a real email address
4. Check your inbox (and spam folder)

## 🔍 Debugging Steps

### Step 1: Check Configuration
Visit: `http://localhost:5000/api/testemail/test-config`

This will show you:
- Current email configuration
- Whether you have placeholder values
- Configuration issues

### Step 2: Check Application Logs
Look for error messages in your application logs. Common errors:

- **"Authentication failed"** → Wrong password or need App Password
- **"SMTP server requires secure connection"** → Port or SSL issue
- **"Mailbox unavailable"** → Wrong email address
- **"Connection refused"** → Wrong SMTP server or port

### Step 3: Test Simple Email
Visit: `http://localhost:5000/api/testemail/send-test?email=YOUR_EMAIL@gmail.com`

This will attempt to send a basic test email and show detailed error messages.

## 🚨 Common Issues & Solutions

### Issue: "Authentication failed"
**Solutions:**
- For Gmail: Use App Password, not regular password
- Enable 2-Factor Authentication first
- Make sure email and username match

### Issue: "Connection timeout"
**Solutions:**
- Check your internet connection
- Verify SMTP server address
- Try different port (465 for SSL, 587 for TLS)

### Issue: "SMTP server requires secure connection"
**Solutions:**
- Use port 587 (TLS) or 465 (SSL)
- Ensure EnableSsl is true in EmailService

### Issue: Still using placeholder values
**Solutions:**
- Replace ALL placeholder text with real values
- Make sure no "your-email" or "your-password" remains
- Use your actual email address

## 🔐 Security Notes

### For Development:
- Use App Passwords for Gmail (more secure)
- Don't commit real credentials to version control
- Consider using User Secrets or environment variables

### For Production:
- Use professional email services (SendGrid, Mailgun, AWS SES)
- Store credentials securely (Key Vault, environment variables)
- Monitor email delivery and bounces

## 📝 Quick Checklist

- [ ] Replace placeholder email settings with real credentials
- [ ] For Gmail: Enable 2FA and generate App Password
- [ ] Use your actual email address (not "your-email@gmail.com")
- [ ] Use App Password (not regular Gmail password)
- [ ] Restart the application after changes
- [ ] Test with the test endpoints
- [ ] Check spam folder for emails
- [ ] Verify SMTP server and port are correct

## 🆘 If Still Not Working

1. **Try a different email provider** (Outlook instead of Gmail)
2. **Check your email provider's SMTP settings**
3. **Look at application logs** for detailed error messages
4. **Test SMTP connection** with external tools
5. **Consider using a professional email service** like SendGrid

## 📧 Professional Email Services (Recommended for Production)

### SendGrid (Free tier available)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "PharmaFlow",
    "SmtpUsername": "apikey",
    "SmtpPassword": "YOUR_SENDGRID_API_KEY"
  }
}
```

### Mailgun (Free tier available)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailgun.org",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "PharmaFlow", 
    "SmtpUsername": "postmaster@yourdomain.com",
    "SmtpPassword": "YOUR_MAILGUN_PASSWORD"
  }
}
```

Remember: The most important step is replacing the placeholder values with real email credentials!