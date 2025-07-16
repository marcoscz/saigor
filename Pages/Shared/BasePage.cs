using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Saigor.Pages.Shared
{
    public class BasePage : ComponentBase
    {
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        protected bool IsLoading { get; set; }
        protected string SearchString { get; set; } = string.Empty;

        protected void ShowSuccess(string message) => Snackbar.Add(message, Severity.Success);
        protected void ShowError(string message) => Snackbar.Add(message, Severity.Error);
        protected void ShowWarning(string message) => Snackbar.Add(message, Severity.Warning);
        protected void ShowInfo(string message) => Snackbar.Add(message, Severity.Info);

        protected void NavigateTo(string uri) => NavigationManager.NavigateTo(uri);

        protected async Task ExecuteWithLoadingAsync(Func<Task> action, string? errorMessage = null)
        {
            IsLoading = true;
            StateHasChanged();
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                ShowError(errorMessage ?? $"Ocorreu um erro: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> action, string? errorMessage = null)
        {
            IsLoading = true;
            StateHasChanged();
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                ShowError(errorMessage ?? $"Ocorreu um erro: {ex.Message}");
                return default!;
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
    }
} 