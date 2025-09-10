# Sending email

## SMTP4Dev

This project comes with a local smtp server. This server accepts any email adresses and makes mails available through a webmail application.

> See [Getting started](./getting-started.md) to launch it.

### Using the web interface

- The web interface is hosted at: [http://localhost:6010](http://localhost:6010).
- Use the credentials provided by the [smtp4dev.env](../.env/smtp4dev.env) file.

### SMTP Connection

The server listens on `localhost` at port `25`. SSL is not configured, so send without. You will need to authenticate. Use the same credentials as for the web interface.

## EmailSender Service

Dependency Injection provides an email sender. It is availble by injecting `KDC.Main.Services.IEmailService`.

```c#
public class EmailExample
{
    private readonly IEmailService _emailService;

    public EmailExample(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendSingleEmail()
    {
        var template = "Dear {{Name}}, You are totally {{Compliment}}.";

        await _emailService.SendEmailAsync("test@test.test", "test email", template, new { Name = "Luke", Compliment = "Awesome" });
    }
}
```
