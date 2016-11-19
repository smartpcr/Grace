﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl.InstanceStrategies
{
    public class FuncWithInjectionContextInstanceExportStrategy<T> : BaseInstanceExportStrategy
    {
        private readonly Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, T> _func;

        public FuncWithInjectionContextInstanceExportStrategy(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, T> func, IInjectionScope injectionScope) : 
            base(typeof(T), injectionScope)
        {
            _func = func;
        }

        protected override IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request,
            ICompiledLifestyle lifestyle)
        {
            request.RequireInjectionContext();

            var staticContext = request.GetStaticInjectionContext();

            Expression expressionStatement =
                Expression.Call(Expression.Constant(_func.Target),
                                _func.GetMethodInfo(),
                                request.Constants.ScopeParameter,
                                Expression.Constant(staticContext),
                                request.Constants.InjectionContextParameter);

            expressionStatement = ApplyNullCheckAndAddDisposal(scope, request, expressionStatement);

            var expressionResult = request.Services.Compiler.CreateNewResult(request, expressionStatement);

            if (lifestyle != null)
            {
                expressionResult = lifestyle.ProvideLifestlyExpression(scope, request, expressionResult);
            }

            return expressionResult;
        }
    }
}
