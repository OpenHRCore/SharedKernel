namespace OpenHRCore.SharedKernel.Application
{
    /// <summary>
    /// Represents a generic service response for OpenHRCore operations.
    /// </summary>
    /// <typeparam name="TData">The type of data contained in the response.</typeparam>
    public class OpenHRCoreServiceResponse<TData> where TData : class
    {
        /// <summary>
        /// Gets or sets the data returned by the service operation.
        /// </summary>
        public TData? Data { get; set; }

        /// <summary>
        /// Gets or sets the error message in case of a failure.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the general message for the response.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Creates a successful response with the provided data and message.
        /// </summary>
        /// <param name="data">The data to be included in the response.</param>
        /// <param name="message">The success message.</param>
        /// <returns>A new instance of OpenHRCoreServiceResponse indicating success.</returns>
        public static OpenHRCoreServiceResponse<TData> CreateSuccess(TData data, string message) =>
            new OpenHRCoreServiceResponse<TData>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };

        /// <summary>
        /// Creates a failure response with the provided error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <returns>A new instance of OpenHRCoreServiceResponse indicating failure.</returns>
        public static OpenHRCoreServiceResponse<TData> CreateFailure(string errorMessage) =>
            new OpenHRCoreServiceResponse<TData>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };

        /// <summary>
        /// Creates a failure response with the provided exception and error message.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <returns>A new instance of OpenHRCoreServiceResponse indicating failure with exception details.</returns>
        public static OpenHRCoreServiceResponse<TData> CreateFailure(Exception exception, string errorMessage) =>
            new OpenHRCoreServiceResponse<TData>
            {
                IsSuccess = false,
                ErrorMessage = exception.Message,
                Message = errorMessage
            };
    }

    /// <summary>
    /// Represents a paginated service response for OpenHRCore operations.
    /// </summary>
    /// <typeparam name="TData">The type of data contained in the response.</typeparam>
    public class OpenHRCorePaginatedResponse<TData> : OpenHRCoreServiceResponse<TData> where TData : class
    {
        /// <summary>
        /// Gets or sets the total number of records.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the search criteria dictionary.
        /// </summary>
        public Dictionary<string, string> SearchCriteria { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a value indicating whether the sorting is ascending.
        /// </summary>
        public bool IsAscending { get; set; } = true;

        /// <summary>
        /// Gets or sets the property name to sort by.
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// Creates a successful paginated response with the provided data and pagination details.
        /// </summary>
        /// <param name="data">The data to be included in the response.</param>
        /// <param name="totalRecords">The total number of records.</param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="searchCriteria">The search criteria dictionary.</param>
        /// <param name="orderBy">The property name to sort by.</param>
        /// <param name="isAscending">Whether to sort in ascending order.</param>
        /// <param name="message">The success message.</param>
        /// <returns>A new instance of OpenHRCorePaginatedResponse with pagination details.</returns>
        public static OpenHRCorePaginatedResponse<TData> CreateSuccess(
            TData data,
            int totalRecords,
            int currentPage,
            int pageSize,
            Dictionary<string, string>? searchCriteria,
            string? orderBy,
            bool isAscending,
            string message) =>
            new OpenHRCorePaginatedResponse<TData>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                TotalRecords = totalRecords,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SearchCriteria = searchCriteria ?? new Dictionary<string, string>(),
                OrderBy = orderBy,
                IsAscending = isAscending
            };

        /// <summary>
        /// Creates a failure response with the provided error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <returns>A new instance of OpenHRCorePaginatedResponse indicating failure.</returns>
        public static new OpenHRCorePaginatedResponse<TData> CreateFailure(string errorMessage) =>
            new OpenHRCorePaginatedResponse<TData>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };

        /// <summary>
        /// Creates a failure response with the provided exception and error message.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <returns>A new instance of OpenHRCorePaginatedResponse indicating failure with exception details.</returns>
        public static new OpenHRCorePaginatedResponse<TData> CreateFailure(Exception exception, string errorMessage) =>
            new OpenHRCorePaginatedResponse<TData>
            {
                IsSuccess = false,
                ErrorMessage = exception.Message,
                Message = errorMessage
            };
    }
}
