﻿using System;
using Ivas.Transactions.Domain.Dtos;
using Ivas.Transactions.Domain.Enums;
using Ivas.Transactions.Shared.Notifications;
using Ivas.Transactions.Shared.Specifications.Interfaces;

namespace Ivas.Transactions.Domain.Rules
{
    public class IsGuidValid : IResultedSpecification<string>
    {
        public Result IsSatisfiedBy(string stringToEvaluate)
        {
            var expression = 
                !string.IsNullOrWhiteSpace(stringToEvaluate) && 
                Guid.TryParse(stringToEvaluate, out var guidOutput);

            return !expression
                ? Result.Failure(ErrorCodesEnum.TransactionIdProvidedNotValid)
                : Result.Ok();
        }
    }
}