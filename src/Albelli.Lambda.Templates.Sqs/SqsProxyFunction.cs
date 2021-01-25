﻿using System.Collections.Generic;
using Albelli.Lambda.Templates.Core;
using Amazon.Lambda.SQSEvents;

namespace Albelli.Lambda.Templates.Sqs
{
    public class SqsProxyFunction<TEntity, TStartup> : LambdaProxyFunction<TEntity, SQSEvent, SQSEvent.SQSMessage, TStartup> where TStartup : class
    {
        protected override IEnumerable<SQSEvent.SQSMessage> GetItems(SQSEvent collection) => collection.Records;
        protected override string GetJson(SQSEvent.SQSMessage item) => item.Body;
    }
}