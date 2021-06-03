using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auth0app.Pages.Home
{
    [Authorize]
    public partial class Details
    {
        [Inject]
        public IOptions<ApiSettings> Options { get; set; }

        [Inject]
        public ManagementClient ManagementClient { get; set; }

        public Auth0.ManagementApi.Models.Client Client { get; set; }

        [Parameter]
        public string Id { get; set; }

        private bool _IsLoading = true;

        public bool IsLoading
        {
            get => _IsLoading;
            set
            {
                if (_IsLoading != value)
                {
                    _IsLoading = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        private bool _IsLoadingRules = true;

        public bool IsLoadingRules
        {
            get => _IsLoadingRules;
            set
            {
                if (_IsLoadingRules != value)
                {
                    _IsLoadingRules = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        public List<Rule> Rules { get; set; } = new List<Rule>();

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await InvokeAsync(LoadApp);
        }

        private async Task LoadApp()
        {
            IsLoading = true;
            try
            {
                Client = await ManagementClient.GetApplicationAsync(Id);
            }
            finally
            {
                IsLoading = false;
            }
            await LoadRules();
        }

        private async Task LoadRules()
        {
            if (Client == null)
            {
                return;
            }
            IsLoadingRules = true;
            try
            {
                var rules = await ManagementClient.GetApplicationRules(Client.Name);
                Rules.AddRange(rules);
            }
            finally
            {
                IsLoadingRules = false;
            }
        }
    }
}