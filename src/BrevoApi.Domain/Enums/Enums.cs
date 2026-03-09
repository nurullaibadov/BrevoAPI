namespace BrevoApi.Domain.Enums;

public enum ContactStatus
{
    Active = 1,
    Unsubscribed = 2,
    Blacklisted = 3,
    Bounced = 4
}

public enum TemplateStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum CampaignStatus
{
    Draft = 1,
    Scheduled = 2,
    Sending = 3,
    Sent = 4,
    Paused = 5,
    Cancelled = 6
}

public enum EmailLogStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Opened = 4,
    Clicked = 5,
    Bounced = 6,
    Failed = 7,
    Unsubscribed = 8
}

public enum EmailType
{
    Transactional = 1,
    Campaign = 2,
    System = 3
}
