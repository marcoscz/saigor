using Microsoft.Extensions.Options;
using Saigor.Configuration;

namespace Saigor.Services;

/// <summary>
/// Serviço para notificações do sistema
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ICacheService _cacheService;
    private readonly AppSettings _appSettings;
    private readonly List<Notification> _notifications;

    public NotificationService(
        ILogger<NotificationService> logger,
        ICacheService cacheService,
        IOptions<AppSettings> appSettings)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(appSettings);
        _logger = logger;
        _cacheService = cacheService;
        _appSettings = appSettings.Value;
        _notifications = new List<Notification>();
    }

    /// <summary>
    /// Adiciona uma notificação
    /// </summary>
    public Task AddNotificationAsync(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        try
        {
            notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            _notifications.Add(notification);

            // Salvar no cache para persistência
            var cacheKey = $"notification_{notification.Id}";
            _cacheService.Set(cacheKey, notification, TimeSpan.FromDays(7));

            _logger.LogInformation("Notificação adicionada: {Type} - {Message}", 
                notification.Type, notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar notificação");
            throw;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adiciona notificação de job falhou
    /// </summary>
    public Task AddJobFailedNotificationAsync(string jobName, string error)
    {
        var notification = new Notification
        {
            Type = NotificationType.Error,
            Title = "Job Falhou",
            Message = $"O job '{jobName}' falhou durante a execução: {error}",
            Category = NotificationCategory.Job,
            Priority = NotificationPriority.High
        };

        return AddNotificationAsync(notification);
    }

    /// <summary>
    /// Adiciona notificação de job completado
    /// </summary>
    public Task AddJobCompletedNotificationAsync(string jobName)
    {
        var notification = new Notification
        {
            Type = NotificationType.Success,
            Title = "Job Completado",
            Message = $"O job '{jobName}' foi executado com sucesso",
            Category = NotificationCategory.Job,
            Priority = NotificationPriority.Low
        };

        return AddNotificationAsync(notification);
    }

    /// <summary>
    /// Adiciona notificação de sistema
    /// </summary>
    public Task AddSystemNotificationAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        var notification = new Notification
        {
            Type = type,
            Title = title,
            Message = message,
            Category = NotificationCategory.System,
            Priority = type == NotificationType.Error ? NotificationPriority.High : NotificationPriority.Medium
        };

        return AddNotificationAsync(notification);
    }

    /// <summary>
    /// Obtém todas as notificações
    /// </summary>
    public Task<IEnumerable<Notification>> GetNotificationsAsync(bool includeRead = false)
    {
        try
        {
            var notifications = _notifications.AsEnumerable();

            if (!includeRead)
            {
                notifications = notifications.Where(n => !n.IsRead);
            }

            var result = notifications.OrderByDescending(n => n.CreatedAt);
            return Task.FromResult<IEnumerable<Notification>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações");
            return Task.FromResult<IEnumerable<Notification>>(new List<Notification>());
        }
    }

    /// <summary>
    /// Obtém notificações por categoria
    /// </summary>
    public Task<IEnumerable<Notification>> GetNotificationsByCategoryAsync(NotificationCategory category)
    {
        try
        {
            var result = _notifications
                .Where(n => n.Category == category)
                .OrderByDescending(n => n.CreatedAt);
            return Task.FromResult<IEnumerable<Notification>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações por categoria {Category}", category);
            return Task.FromResult<IEnumerable<Notification>>(new List<Notification>());
        }
    }

    /// <summary>
    /// Obtém notificações por tipo
    /// </summary>
    public Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type)
    {
        try
        {
            var result = _notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt);
            return Task.FromResult<IEnumerable<Notification>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações por tipo {Type}", type);
            return Task.FromResult<IEnumerable<Notification>>(new List<Notification>());
        }
    }

    /// <summary>
    /// Marca notificação como lida
    /// </summary>
    public Task MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                // Atualizar no cache
                var cacheKey = $"notification_{notificationId}";
                _cacheService.Set(cacheKey, notification, TimeSpan.FromDays(7));

                _logger.LogDebug("Notificação marcada como lida: {Id}", notificationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação como lida {Id}", notificationId);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marca todas as notificações como lidas
    /// </summary>
    public Task MarkAllAsReadAsync()
    {
        try
        {
            var unreadNotifications = _notifications.Where(n => !n.IsRead);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                // Atualizar no cache
                var cacheKey = $"notification_{notification.Id}";
                _cacheService.Set(cacheKey, notification, TimeSpan.FromDays(7));
            }

            _logger.LogInformation("Todas as notificações marcadas como lidas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar todas as notificações como lidas");
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove uma notificação
    /// </summary>
    public Task RemoveNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                _notifications.Remove(notification);

                // Remover do cache
                var cacheKey = $"notification_{notificationId}";
                _cacheService.Remove(cacheKey);

                _logger.LogDebug("Notificação removida: {Id}", notificationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover notificação {Id}", notificationId);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove notificações antigas
    /// </summary>
    public Task<int> CleanOldNotificationsAsync(int daysToKeep = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldNotifications = _notifications
                .Where(n => n.CreatedAt < cutoffDate)
                .ToList();

            foreach (var notification in oldNotifications)
            {
                _notifications.Remove(notification);

                // Remover do cache
                var cacheKey = $"notification_{notification.Id}";
                _cacheService.Remove(cacheKey);
            }

            _logger.LogInformation("Notificações antigas removidas: {Count}", oldNotifications.Count);
            return Task.FromResult(oldNotifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar notificações antigas");
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Obtém estatísticas das notificações
    /// </summary>
    public Task<NotificationStatistics> GetNotificationStatisticsAsync()
    {
        try
        {
            var totalNotifications = _notifications.Count;
            var unreadNotifications = _notifications.Count(n => !n.IsRead);
            var errorNotifications = _notifications.Count(n => n.Type == NotificationType.Error);
            var recentNotifications = _notifications.Count(n => n.CreatedAt >= DateTime.UtcNow.AddHours(-24));

            var statistics = new NotificationStatistics
            {
                TotalNotifications = totalNotifications,
                UnreadNotifications = unreadNotifications,
                ErrorNotifications = errorNotifications,
                RecentNotifications = recentNotifications
            };

            _logger.LogDebug("Estatísticas das notificações obtidas: Total={Total}, Não lidas={Unread}", 
                totalNotifications, unreadNotifications);
            return Task.FromResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas das notificações");
            return Task.FromResult(new NotificationStatistics());
        }
    }
}

/// <summary>
/// Interface para serviço de notificações
/// </summary>
public interface INotificationService
{
    Task AddNotificationAsync(Notification notification);
    Task AddJobFailedNotificationAsync(string jobName, string error);
    Task AddJobCompletedNotificationAsync(string jobName);
    Task AddSystemNotificationAsync(string title, string message, NotificationType type = NotificationType.Info);
    Task<IEnumerable<Notification>> GetNotificationsAsync(bool includeRead = false);
    Task<IEnumerable<Notification>> GetNotificationsByCategoryAsync(NotificationCategory category);
    Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync();
    Task RemoveNotificationAsync(Guid notificationId);
    Task<int> CleanOldNotificationsAsync(int daysToKeep = 30);
    Task<NotificationStatistics> GetNotificationStatisticsAsync();
}

/// <summary>
/// Notificação do sistema
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

/// <summary>
/// Tipo de notificação
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Categoria de notificação
/// </summary>
public enum NotificationCategory
{
    System,
    Job,
    Security,
    Performance
}

/// <summary>
/// Prioridade da notificação
/// </summary>
public enum NotificationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Estatísticas das notificações
/// </summary>
public class NotificationStatistics
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int ErrorNotifications { get; set; }
    public int RecentNotifications { get; set; }
    
    public double ReadRate => TotalNotifications > 0 ? 
        (double)(TotalNotifications - UnreadNotifications) / TotalNotifications * 100 : 0;
} 