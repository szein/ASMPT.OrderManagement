using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Radzen;

public class CustomHttpHandler : DelegatingHandler
{
    private readonly NotificationService _notificationService; // Replace with your notification library service

    public CustomHttpHandler(NotificationService notificationService)
    {
         _notificationService = notificationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            _notificationService.Notify(NotificationSeverity.Warning, "Access Denied: You do not have permission to perform this action.");
        }

        return response;
    }
}