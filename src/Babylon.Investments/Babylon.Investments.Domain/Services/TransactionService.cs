﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Babylon.Networking.Interfaces.Brokers;
using Babylon.Investments.Domain.Abstractions.Services;
using Babylon.Investments.Domain.Contracts.Repositories;
using Babylon.Investments.Domain.Dtos;
using Babylon.Investments.Domain.Objects;
using Babylon.Investments.Domain.Requests;
using Babylon.Investments.Domain.Responses;
using Babylon.Investments.Domain.Validators;
using Babylon.Investments.Shared.Exceptions.Custom;
using Babylon.Investments.Shared.Notifications;
using Microsoft.Extensions.Logging;

namespace Babylon.Investments.Domain.Services
{
    public interface ITransactionService :
        ICreatableAsyncService<TransactionPostDto>, 
        IDeletableAsyncService<TransactionDeleteDto>
    {
        Task<IEnumerable<TransactionGetResponse>> GetByClientAndUserAsync(string clientIdentifier, string userId);

        Task<TransactionGetResponse> GetSingleAsync(string clientIdentifier, string transactionId);
    }
    
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionValidator _transactionValidator;

        private readonly ITransactionRepository _transactionRepository;

        private readonly IFinancialsBroker _financialsBroker;
        
        private readonly IMapper _mapper;

        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionValidator transactionValidator,
            ITransactionRepository transactionRepository,
            IFinancialsBroker financialsBroker,
            IMapper mapper,
            ILogger<TransactionService> logger)
        {
            _transactionValidator = transactionValidator 
                                    ?? throw new ArgumentNullException(nameof(transactionValidator));
            
            _transactionRepository = transactionRepository 
                                     ?? throw new ArgumentNullException(nameof(transactionRepository));

            _financialsBroker = financialsBroker ?? throw new ArgumentNullException(nameof(financialsBroker));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> CreateAsync(TransactionPostDto dto)
        {
            _logger.LogInformation("Investmentservice - Called method CreateAsync");
            
            var validationResult = _transactionValidator.Validate(dto);

            _logger.LogInformation($"Validation Result: { JsonSerializer.Serialize(validationResult) }");
            
            if (validationResult.IsFailure)
            {
                throw new BabylonException(
                    string.Join(", ", validationResult.Errors.Select(x => x.Message)));
            }
            
            var domainObject = new TransactionCreate(dto, _financialsBroker);

            await _transactionRepository.Insert(domainObject);
            
            return Result.Ok(domainObject.TransactionId);
        }

        public async Task<Result> DeleteAsync(TransactionDeleteDto entity)
        {
            _logger.LogInformation("Investmentservice - Called method DeleteAsync");
            
            var isEntityValid = _transactionValidator.ValidateDelete(entity);
            
            _logger.LogInformation($"Validation Result: { JsonSerializer.Serialize(isEntityValid) }");
            
            if (isEntityValid.IsFailure)
            {
                throw new BabylonException(
                    string.Join(
                        ", ", 
                        isEntityValid.Errors.Select(x => x.Message))
                    );
            }

            var transactionToDelete =
                await _transactionRepository.GetByIdAsync(entity.ClientIdentifier, entity.TransactionId);

            _logger.LogInformation($"Transaction to delete: { JsonSerializer.Serialize(transactionToDelete) }");
            
            if (transactionToDelete == null)
            {
                throw new BabylonException($"Transaction specified does not exist. Please provide a valid transaction");
            }
            
            _logger.LogInformation("Deleting transaction from DynamoDB table..");
            
            await _transactionRepository.Delete(transactionToDelete);

            return Result.Ok(transactionToDelete.TransactionId);
        }

        public async Task<IEnumerable<TransactionGetResponse>> GetByClientAndUserAsync(string clientIdentifier, string userId)
        {
            var clientInvestments =
                (await _transactionRepository.GetByClientAsync(clientIdentifier)).OrderByDescending(x => x.Date);

            var userInvestments = clientInvestments.Where(x => x.UserId.Equals(userId));
            
            return _mapper.Map<IEnumerable<Transaction>, IEnumerable<TransactionGetResponse>>(userInvestments);
        }

        public async Task<TransactionGetResponse> GetSingleAsync(string clientIdentifier, string transactionId)
        {
            var transactionToGet =
                await _transactionRepository.GetByIdAsync(clientIdentifier, transactionId);

            return _mapper.Map<Transaction, TransactionGetResponse>(transactionToGet);
        }
    }
}