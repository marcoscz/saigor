using System.Linq.Expressions;

namespace Saigor.Utils
{
    /// <summary>
    /// Generic filter helper for common filtering operations.
    /// </summary>
    public static class GenericFilterHelper
    {
        /// <summary>
        /// Filters a collection by a search string using multiple properties.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The collection to filter</param>
        /// <param name="searchString">The search string</param>
        /// <param name="propertySelectors">Lambda expressions for properties to search in</param>
        /// <returns>Filtered collection</returns>
        public static IEnumerable<T> FilterBySearchString<T>(
            IEnumerable<T> items, 
            string searchString, 
            params Expression<Func<T, object>>[] propertySelectors)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return items;

            var searchLower = searchString.ToLowerInvariant();
            
            return items.Where(item =>
            {
                foreach (var selector in propertySelectors)
                {
                    var value = selector.Compile()(item)?.ToString()?.ToLowerInvariant();
                    if (value?.Contains(searchLower) == true)
                        return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Filters a collection by a specific property value.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The collection to filter</param>
        /// <param name="propertySelector">Lambda expression for the property</param>
        /// <param name="value">The value to filter by</param>
        /// <returns>Filtered collection</returns>
        public static IEnumerable<T> FilterByProperty<T, TValue>(
            IEnumerable<T> items,
            Expression<Func<T, TValue>> propertySelector,
            TValue value)
        {
            if (value == null)
                return items;

            return items.Where(item =>
            {
                var propertyValue = propertySelector.Compile()(item);
                return EqualityComparer<TValue>.Default.Equals(propertyValue, value);
            });
        }

        /// <summary>
        /// Filters a collection by date range.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The collection to filter</param>
        /// <param name="propertySelector">Lambda expression for the date property</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Filtered collection</returns>
        public static IEnumerable<T> FilterByDateRange<T>(
            IEnumerable<T> items,
            Expression<Func<T, DateTime>> propertySelector,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            return items.Where(item =>
            {
                var date = propertySelector.Compile()(item);
                
                if (startDate.HasValue && date < startDate.Value)
                    return false;
                    
                if (endDate.HasValue && date > endDate.Value)
                    return false;
                    
                return true;
            });
        }

        /// <summary>
        /// Filters a collection by status.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The collection to filter</param>
        /// <param name="propertySelector">Lambda expression for the status property</param>
        /// <param name="status">The status to filter by</param>
        /// <returns>Filtered collection</returns>
        public static IEnumerable<T> FilterByStatus<T>(
            IEnumerable<T> items,
            Expression<Func<T, string>> propertySelector,
            string? status = null)
        {
            if (string.IsNullOrWhiteSpace(status))
                return items;

            return items.Where(item =>
            {
                var itemStatus = propertySelector.Compile()(item);
                return string.Equals(itemStatus, status, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// Sorts a collection by a property.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The collection to sort</param>
        /// <param name="propertySelector">Lambda expression for the property to sort by</param>
        /// <param name="ascending">Whether to sort in ascending order</param>
        /// <returns>Sorted collection</returns>
        public static IEnumerable<T> SortByProperty<T, TValue>(
            IEnumerable<T> items,
            Expression<Func<T, TValue>> propertySelector,
            bool ascending = true)
        {
            return ascending 
                ? items.OrderBy(propertySelector.Compile())
                : items.OrderByDescending(propertySelector.Compile());
        }
    }
} 