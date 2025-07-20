using Microsoft.AspNetCore.Components;
using Saigor.Services;
using Saigor.Utils;
using Saigor.Pages.Shared;

namespace Saigor.Shared.Components
{
    /// <summary>
    /// Base class for CRUD pages that provides common functionality.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class CrudPageBase<T> : BasePage where T : class
    {
        [Inject] protected ICrudService<T> CrudService { get; set; } = default!;

        protected List<T> Items { get; set; } = new();
        protected T Item { get; set; } = default!;
        protected new bool IsLoading { get; set; } = false;
        protected new string SearchString { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        protected virtual async Task LoadDataAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                Items = await CrudService.GetAllAsync();
            }, $"Erro ao carregar {typeof(T).Name}");
        }

        protected virtual async Task RefreshDataAsync()
        {
            await LoadDataAsync();
            ShowSuccess($"{typeof(T).Name} atualizados com sucesso!");
        }

        protected virtual async Task DeleteItemAsync(int id)
        {
            var success = await ExecuteWithLoadingAsync(
                async () => await CrudService.DeleteByIdAsync(id),
                $"Erro ao excluir {typeof(T).Name}");

            if (success)
            {
                ShowSuccess($"{typeof(T).Name} excluído com sucesso!");
                await LoadDataAsync();
            }
        }

        protected virtual async Task SaveItemAsync(T item, bool isNew = false)
        {
            var success = await ExecuteWithLoadingAsync(
                async () => isNew ? await CrudService.CreateAsync(item) : await CrudService.UpdateAsync(item),
                $"Erro ao salvar {typeof(T).Name}");

            if (success)
            {
                ShowSuccess($"{typeof(T).Name} salvo com sucesso!");
                NavigateTo(GetListPageUrl());
            }
        }

        protected virtual string GetListPageUrl() => $"/{typeof(T).Name.ToLower()}s";
        protected virtual string GetCreatePageUrl() => $"/{typeof(T).Name.ToLower()}s/create";
        protected virtual string GetEditPageUrl(int id) => $"/{typeof(T).Name.ToLower()}s/edit/{id}";
        protected virtual string GetDeletePageUrl(int id) => $"/{typeof(T).Name.ToLower()}s/delete/{id}";

        protected virtual void NavigateToCreate() => NavigateTo(GetCreatePageUrl());
        protected virtual void NavigateToEdit(int id) => NavigateTo(GetEditPageUrl(id));
        protected virtual void NavigateToDelete(int id) => NavigateTo(GetDeletePageUrl(id));
        protected virtual void NavigateToList() => NavigateTo(GetListPageUrl());
    }
} 