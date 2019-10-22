// <copyright file="Storage.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    /// <summary>
    /// Local storage handling methods.
    /// </summary>
    public static class Storage
    {
        /// <summary>
        /// Sets a value in localstorage.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="key">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task SetItem(IJSRuntime jsRuntime, string key, object value)
        {
            await SetItem(jsRuntime, key, value, 1800).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets a value in localstorage.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="key">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresInSeconds">The value expiration time in seconds.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task SetItem(IJSRuntime jsRuntime, string key, object value, int expiresInSeconds)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            string serializedValue = JsonSerializer.Serialize(value);

            await jsRuntime.InvokeAsync<object>("localStorage.setItem", key, serializedValue).ConfigureAwait(false);
            await jsRuntime.InvokeAsync<object>("localStorage.setItem", $"{key}:exp", expiresInSeconds).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a value from the localstorage.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="key">The identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task RemoveItem(IJSRuntime jsRuntime, string key)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            await jsRuntime.InvokeAsync<object>("localStorage.removeItem", key).ConfigureAwait(false);
            await jsRuntime.InvokeAsync<object>("localStorage.removeItem", $"{key}:exp").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an item from localstorage.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="key">The identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<T> GetItemAsync<T>(IJSRuntime jsRuntime, string key)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            string result = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key).ConfigureAwait(false);

            return JsonSerializer.Deserialize<T>(string.IsNullOrEmpty(result) ? "{}" : result);
        }
    }
}
