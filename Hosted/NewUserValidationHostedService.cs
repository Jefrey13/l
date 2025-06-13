using CustomerService.API.Hubs;

namespace CustomerService.API.Hosted
{
    public class NewUserValidationHostedService: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UserHub _userHub;
        private readonly ILogger<NewUserValidationHostedService> _logger;
        public NewUserValidationHostedService(IServiceScopeFactory scopeFactory, UserHub userHub, 
            ILogger<NewUserValidationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _userHub = userHub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {

        }
    }
}
