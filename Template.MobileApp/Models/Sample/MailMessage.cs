namespace Template.MobileApp.Models.Sample;

public sealed class MailMessage
{
    public DateTime DateTime { get; set; }

    public ImageSource Image { get; set; } = default!;

    public string From { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;
}
