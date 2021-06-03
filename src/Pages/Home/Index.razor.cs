using AntDesign;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auth0app.Pages.Home
{
    [Authorize]
    public partial class Index
    {
        private readonly ListGridType _listGridType = new ListGridType
        {
            Gutter = 16,
            Xs = 1,
            Sm = 2,
            Md = 3,
            Lg = 3,
            Xl = 4,
            Xxl = 4,
        };
        [Inject]
        public IOptions<ApiSettings> Options { get; set; }

        [Inject]
        public ManagementClient ManagementClient { get; set; }

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

        protected override Task OnInitializedAsync()
        {
            IsLoading = true;
            Clients.Add(new Client());
            InvokeAsync(LoadApps);
            return base.OnInitializedAsync();
        }

        public int StartNo { get; set; } = 0;
        public int PageNum { get; set; } = 10;
        
        private bool _HasMoreData = false;
        public bool HasMoreData
        {
            get => _HasMoreData;
            set
            {
                if (_HasMoreData != value)
                {
                    _HasMoreData = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }
        public List<Client> Clients { get; set; } = new List<Client>();

        private async Task LoadApps()
        {
            try
            {
               var apps = await ManagementClient.GetAppsAsync(StartNo, PageNum);
               Clients.AddRange(apps);
               HasMoreData = apps.Paging.Total > Clients.Count - 1;
            }
            catch (System.Exception ex)
            {
                //TODO: add logic to handle exception
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnLoadMore()
        {
            if(!HasMoreData)
            {
                return;
            }
            StartNo++;
            await LoadApps();
        }
    }
}