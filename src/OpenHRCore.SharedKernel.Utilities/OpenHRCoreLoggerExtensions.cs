using Microsoft.Extensions.Logging;
using System.Reflection;

namespace OpenHRCore.SharedKernel.Utilities
{
    /// <summary>
    /// Provides extension methods for ILogger to log messages with layer-specific prefixes.
    /// </summary>
    public static class OpenHRCoreLoggerExtensions
    {
        /// <summary>
        /// Logs an informational message with the layer-specific prefix.
        /// </summary>
        /// <param name="logger">The logger instance to extend.</param>
        /// <param name="message">The informational message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        public static void LogLayerInfo(this ILogger logger, string message, params object?[] args)
        {
            string layerName = Assembly.GetCallingAssembly()?.GetName().Name ?? string.Empty;

            logger.LogInformation($"[{layerName}] {message}", args);
        }

        /// <summary>
        /// Logs a warning message with the layer-specific prefix.
        /// </summary>
        /// <param name="logger">The logger instance to extend.</param>
        /// <param name="message">The warning message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        public static void LogLayerWarning(this ILogger logger, string message, params object?[] args)
        {
            string layerName = Assembly.GetCallingAssembly()?.GetName().Name ?? string.Empty;

            logger.LogWarning($"[{layerName}] {message}", args);
        }

        /// <summary>
        /// Logs an error message with the layer-specific prefix, including an exception.
        /// </summary>
        /// <param name="logger">The logger instance to extend.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The error message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        public static void LogLayerError(this ILogger logger, Exception? exception, string message, params object?[] args)
        {
            string layerName = Assembly.GetCallingAssembly()?.GetName().Name ?? string.Empty;

            logger.LogError(exception, $"[{layerName}] {message}", args);
        }
    }
}
