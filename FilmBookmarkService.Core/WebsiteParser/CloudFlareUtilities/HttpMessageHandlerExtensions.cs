﻿using System.Net.Http;

namespace FilmBookmarkService.Core.CloudFlareUtilities
{
    internal static class HttpMessageHandlerExtensions
    {
        public static HttpMessageHandler GetMostInnerHandler(this HttpMessageHandler self)
        {
            var delegatingHandler = self as DelegatingHandler;
            return delegatingHandler == null ? self : delegatingHandler.InnerHandler.GetMostInnerHandler();
        }
    }
}