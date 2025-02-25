using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using CleanArchitecture.Blazor.Server.UI.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace CleanArchitecture.Blazor.Server.UI.Components.Identity;
public class UserLoginState : ComponentBase, IDisposable
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject]
    public IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;
    [Inject]
    public IStringLocalizer<SharedResource> L { get; set; } = null!;

    public void Dispose()
    {
        Client.LoginEvent -= _client_Login;
        Client.LogoutEvent -= _client_Logout;
        AuthenticationStateProvider.AuthenticationStateChanged -= _authenticationStateProvider_AuthenticationStateChanged;
        GC.SuppressFinalize(this);
    }
    [Inject]
    private HubClient Client { get; set; } = default!;
    [Inject]
    private IUsersStateContainer UsersStateContainer { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        Client.LoginEvent += _client_Login;
        Client.LogoutEvent += _client_Logout;
        await Client.StartAsync().ConfigureAwait(false);
        AuthenticationStateProvider.AuthenticationStateChanged += _authenticationStateProvider_AuthenticationStateChanged;
        var state = await AuthState;
        if (state.User.Identity?.IsAuthenticated ?? false)
        {
            var userId = state.User.GetUserId();
            SetProfile(userId!);
        }
    }

    private void _client_Login(object? sender, UserStateChangeEventArgs args)
    {
        InvokeAsync(() =>
        {
            Snackbar.Add(string.Format(L["{0} has logged in."], args.UserName), Severity.Info);
            UsersStateContainer.AddOrUpdate(args.ConnectionId, args.UserName);
        });
    }

    private void _client_Logout(object? sender, UserStateChangeEventArgs args)
    {
        InvokeAsync(() =>
        {
            Snackbar.Add(string.Format(L["{0} has logged out."], args.UserName), Severity.Normal);
            UsersStateContainer.Remove(args.ConnectionId);

        });
    }
    private void _authenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> authenticationState)
    {
        InvokeAsync(async () =>
        {
            var state = await authenticationState;
            if (state.User.Identity?.IsAuthenticated ?? false)
            {
                var userId = state.User.GetUserId();
                SetProfile(userId!);
            }
        });
    }
    private void SetProfile(string userId)
    {
        Dispatcher.Dispatch(new FetchUserDtoAction() { UserId = userId });
    }
}