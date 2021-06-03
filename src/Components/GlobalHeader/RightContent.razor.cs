using AntDesign;
using AntDesign.ProLayout;
using Auth0app.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth0app.Components
{
    public partial class RightContent
    {
        private CurrentUser _currentUser = new CurrentUser();      

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    
        [Inject] protected NavigationManager NavigationManager { get; set; }

        [Inject] protected MessageService MessageService { get; set; }
        [Inject]
        public IOptions<ApiSettings> Options { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            SetClassMap();
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var nickName = user.Claims.FirstOrDefault(x => x.Type == "nickname")?.Value;
            var pic = user.Claims.FirstOrDefault(x => x.Type == "picture")?.Value;
            _currentUser = new CurrentUser
            {
                Name = nickName,
                Avatar = pic
            };
        }

        protected void SetClassMap()
        {
            ClassMapper
                .Clear()
                .Add("right");
        }

        public void HandleSelectUser(MenuItem item)
        {
            switch (item.Key)
            {
                case "center":
                    NavigationManager.NavigateTo($"{Options.Value.ConsoleBaseUrl}/profile",true);
                    break;

                case "logout":
                    NavigationManager.NavigateTo("/Account/Logout", true);
                    break;
            }
        }
    }
}