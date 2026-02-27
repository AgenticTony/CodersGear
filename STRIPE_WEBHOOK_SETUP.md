# Stripe Webhook Setup Guide

This guide explains how to set up Stripe webhooks for the CodersGear application.

## Why Webhooks Are Important

Webhooks provide **reliable payment confirmation** that doesn't depend on:
- User's browser not closing
- Network interruptions
- Redirect failures

When a payment completes, Stripe sends a webhook to your server directly.

## Setup Steps

### 1. Deploy Your Application (Required for Stripe)

Stripe webhooks need a **publicly accessible URL**. Options:

**Option A: Deploy to production/hosting**
- Azure App Service
- AWS
- Google Cloud
- Heroku
- Any web host

**Option B: Use a tunnel for local development**
```bash
# Install ngrok: https://ngrok.com/download
ngrok http 5000  # Or whatever port your app uses
```

Your tunnel URL will be something like: `https://abc123.ngrok.io`

### 2. Create Webhook in Stripe Dashboard

1. Go to [Stripe Dashboard → Webhooks](https://dashboard.stripe.com/webhooks)
2. Click **"Add endpoint"**
3. Enter your webhook URL:
   ```
   https://yourdomain.com/api/Stripewebhook
   ```
   Or for ngrok:
   ```
   https://abc123.ngrok.io/api/Stripewebhook
   ```

### 3. Select Events to Listen To

Enable the following events:

| Event | Description |
|-------|-------------|
| `checkout.session.completed` | Payment succeeded |
| `checkout.session.async_payment_succeeded` | Async payment (like bank transfer) succeeded |
| `checkout.session.async_payment_failed` | Async payment failed |

### 4. Copy Webhook Secret

After creating the webhook:
1. Click on the webhook endpoint
2. Click **"Reveal"** next to "Signing secret"
3. Copy the secret (starts with `whsec_`)

### 5. Update appsettings.json

Replace the placeholder WebhookSecret:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_YOUR_ACTUAL_WEBHOOK_SECRET_HERE"
  }
}
```

### 6. Test the Webhook

1. Run your application
2. Create a test checkout
3. Complete payment in Stripe test mode
4. Check your logs for webhook events

**Test Card Number:** `4242 4242 4242 4242`
**Any future date** for expiration
**Any 3 digits** for CVC

### 7. Verify Webhook Processing

After successful payment:
1. Check `OrderHeaders` table in database
2. Verify `PaymentStatus` = "Approved"
3. Verify `PaymentIntentId` is populated
4. Verify `SessionId` is populated
5. Verify `ShoppingCarts` are cleared for that user

## Security Notes

⚠️ **CRITICAL:** Never expose your webhook secret or secret key:
- Don't commit them to Git
- Don't log them
- Don't include them in client-side code

The webhook signature verification in `StripeWebhookController` ensures:
- Requests are genuinely from Stripe
- Data hasn't been tampered with
- No replay attacks

## Troubleshooting

### Webhook not firing?
- Check the URL is publicly accessible
- Check Stripe Dashboard webhook logs for errors
- Verify firewall/port settings

### Signature verification failing?
- Ensure WebhookSecret matches exactly (no extra spaces)
- Check you're using the correct webhook (test vs live mode)

### Order not updating?
- Check application logs for errors
- Verify `order_id` is in metadata during checkout creation
- Check database connectivity

### Want to test locally?
Use [Stripe CLI](https://stripe.com/docs/stripe-cli):
```bash
stripe listen --forward-to localhost:5000/api/Stripewebhook
```

This will forward webhook events to your local machine!

## Production Checklist

- [ ] Deploy application to public URL
- [ ] Create webhook in Stripe Dashboard (LIVE mode)
- [ ] Copy LIVE mode webhook secret
- [ ] Update appsettings.Production.json
- [ ] Test with real payment (small amount)
- [ ] Verify webhook logs show 200 OK responses
- [ ] Monitor for webhook failures in Stripe Dashboard

## Additional Resources

- [Stripe Webhooks Documentation](https://stripe.com/docs/webhooks)
- [Stripe Webhooks Best Practices](https://stripe.com/docs/webhooks/best-practices)
- [Webhook Security Guide](https://stripe.com/docs/webhooks/signatures)
