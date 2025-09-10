using System;
using FluentEmail.Core;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace KDC.Main.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
    Task SendEmailAsync<T>(string email, string subject, string template, T model);
    Task SendLocalizedEmailAsync(string userId, string email, string templateName, string htmlMessage, string subject, object model);
}

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly IEmailLocalizationService _localizationService;
    private readonly ITemplateEngine _templateEngine;

    public EmailService(IFluentEmail fluentEmail, IEmailLocalizationService localizationService, ITemplateEngine templateEngine)
    {
        _fluentEmail = fluentEmail;
        _localizationService = localizationService;
        _templateEngine = templateEngine;
    }

    /// <summary>
    /// Sends an email with a html template (Using Hangfire)
    /// </summary>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="htmlMessage"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Send mail using hangfire queue for retry
        BackgroundJob.Enqueue(
                    () => ExecuteAsync(email, subject, htmlMessage));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends an email using a given template (Using Hangfire)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="template"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task SendEmailAsync<T>(string email, string subject, string template, T model)
    {
        // Send mail using hangfire queue for retry
        BackgroundJob.Enqueue(
                    () => ExecuteAsync(email, subject, template, model));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends an email with a html template (Without Hangfire)
    /// </summary>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="htmlMessage"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task ExecuteAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(email))
            throw new Exception($"{nameof(email)} must not be null or empty");

        if (string.IsNullOrEmpty(subject))
            throw new Exception($"{nameof(subject)} must not be null or empty");

        if (string.IsNullOrEmpty(htmlMessage))
            throw new Exception($"{nameof(htmlMessage)} must not be null or empty");

        await _fluentEmail
                        .To(email)
                        .Subject(subject)
                        .Body(htmlMessage, isHtml:true)
                        .SendAsync();
    }

    /// <summary>
    /// Sends an email using a given template (Without Hangfire)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="template"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task ExecuteAsync<T>(string email, string subject, string template, T model)
    {
        if (string.IsNullOrEmpty(email))
            throw new Exception($"{nameof(email)} must not be null or empty");

        if (string.IsNullOrEmpty(subject))
            throw new Exception($"{nameof(subject)} must not be null or empty");

        if (string.IsNullOrEmpty(template))
            throw new Exception($"{nameof(template)} must not be null or empty");

        if (model == null)
            throw new Exception($"{nameof(model)} must not be null");

        await _fluentEmail
                .To(email)
                .Subject(subject)
                .UsingTemplate(template, model)
                .SendAsync();
    }

    public async Task SendLocalizedEmailAsync(string userId, string email, string templateName, string htmlMessage, string subject, object model)
    {
        var culture = await _localizationService.GetUserPreferredCulture(userId);
        
        using (new CultureScope(culture))
        {
            var emailContent = _templateEngine.RenderTemplateAsync(templateName, culture, model);
            await _fluentEmail
                .To(email)
                .Subject(emailContent.Subject)
                .Body(emailContent.Body)
                .SendAsync();
        }
    }
}
