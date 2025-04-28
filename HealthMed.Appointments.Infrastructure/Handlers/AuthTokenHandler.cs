using Microsoft.AspNetCore.Http;

namespace HealthMed.Appointments.Infrastructure.Handlers
{
    // 1) Defina um handler concreto em um arquivo separado:
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;
        public AuthTokenHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _accessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Add("Authorization", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
