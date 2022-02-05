﻿using System.Threading.Tasks;
using Babylon.Investments.Domain.Abstractions.Dtos;
using Babylon.Investments.Shared.Notifications;

namespace Babylon.Investments.Domain.Abstractions.Services
{
    public interface IDeletableAsyncService<in TDto>
    {
        /// <summary>
        /// Deletes a previously created entity.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <returns>The result code of the operation.</returns>
        Task<Result> DeleteAsync(TDto entity);
    }
}