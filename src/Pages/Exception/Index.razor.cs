using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth0app.Pages.Exception
{
    public partial class Index
    {
        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        protected override async Task OnInitializedAsync()
        {            
            await base.OnInitializedAsync();
            var exceptionHandlerPathFeature =HttpContextAccessor.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
        }
    }
}
