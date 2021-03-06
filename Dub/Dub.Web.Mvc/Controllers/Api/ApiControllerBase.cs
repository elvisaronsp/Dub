﻿// -----------------------------------------------------------------------
// <copyright file="ApiControllerBase.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Dub.Web.Mvc.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NETCORE
    using System.Web;
    using System.Web.Http;
#endif
    using Dub.Web.Core;
    using Dub.Web.Mvc.Models;
#if !NETCORE
    using Microsoft.Owin;
#endif
#if NETCORE
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
#else
    using Microsoft.AspNet.Identity;
#endif
#if !NETCORE
    using Controller = System.Web.Http.ApiController;
    using IActionResult = System.Web.Http.IHttpActionResult;
#endif

    /// <summary>
    /// Base controller for all API controllers.
    /// </summary>
    public class ApiControllerBase : Controller
    {
#if !NETCORE
        /// <summary>
        /// Gets OWIN context.
        /// </summary>
        protected IOwinContext OwinContext
        {
            get
            {
                return HttpContext.Current.GetOwinContext();
            }
        }

        /// <summary>
        /// Get OWIN variable from request.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <returns>Element of the given type from the request context.</returns>
        protected internal T GetOwinValiable<T>()
        {
            return (T)this.OwinContext.Request.Environment["AspNet.Identity.Owin:" + typeof(T).AssemblyQualifiedName];
        }
#endif

        /// <summary>
        /// Returns  simple API status code.
        /// </summary>
        /// <param name="code">API code to return.</param>
        /// <returns>Action result which represent specific API code.</returns>
        protected IActionResult StatusCode(ApiStatusCode code)
        {
            return this.Ok(new ApiStatusResponse() { Code = code });
        }

        /// <summary>
        /// Returns API status code which contains information about errors.
        /// </summary>
        /// <param name="code">API code to return.</param>
        /// <param name="errors">Sequence of errors.</param>
        /// <returns>Action result which represent specific API code.</returns>
        protected IActionResult ErrorCode(ApiStatusCode code, IEnumerable<string> errors)
        {
            return this.Ok(new ApiErrorStatusResponse() 
            { 
                Code = code, 
                Errors = errors.ToArray() 
            });
        }

#if NETCORE
        /// <summary>
        /// Returns API status code which contains information about errors.
        /// </summary>
        /// <param name="code">API code to return.</param>
        /// <param name="errors">Sequence of errors.</param>
        /// <returns>Action result which represent specific API code.</returns>
        protected IActionResult ErrorCode(ApiStatusCode code, IEnumerable<IdentityError> errors)
        {
            return this.Ok(new ApiErrorStatusResponse()
            {
                Code = code,
                Errors = errors.Select(_ => _.Description).ToArray()
            });
        }
#endif

        /// <summary>
        /// Performs filtering of the data collection.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection to filter.</typeparam>
        /// <param name="collection">Sequence of elements to filter.</param>
        /// <param name="filter">Filter which apply to collection.</param>
        /// <param name="sortBy">Sorting function.</param>
        /// <param name="sortAscending">Sorting direction.</param>
        /// <param name="startRow">Row from which start filtering.</param>
        /// <param name="pageSize">Count of rows in the page.</param>
        /// <returns>Collection with specific filtering rules applied.</returns>
        protected IEnumerable<T> Filter<T>(
            IQueryable<T> collection, 
            ICollectionFilter<T> filter, 
            Func<T, IComparable> sortBy, 
            bool sortAscending, 
            int startRow, 
            int pageSize)
        {
            collection = filter.Filter(collection);
            if (sortBy == null)
            {
                return collection;
            }

            // Perform sorting
            var sortedCollection = sortAscending
                ? collection.OrderBy(sortBy)
                : collection.OrderByDescending(sortBy);
            
            // Perform filtering.
            var pagedCollection = sortedCollection.Skip(startRow).Take(pageSize);
            return pagedCollection;
        }
    }
}
