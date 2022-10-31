// =====================================================================================================================
// == LICENSE:       Copyright (c) 2022 Kevin De Coninck
// ==
// ==                Permission is hereby granted, free of charge, to any person
// ==                obtaining a copy of this software and associated documentation
// ==                files (the "Software"), to deal in the Software without
// ==                restriction, including without limitation the rights to use,
// ==                copy, modify, merge, publish, distribute, sublicense, and/or sell
// ==                copies of the Software, and to permit persons to whom the
// ==                Software is furnished to do so, subject to the following
// ==                conditions:
// ==
// ==                The above copyright notice and this permission notice shall be
// ==                included in all copies or substantial portions of the Software.
// ==
// ==                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// ==                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// ==                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// ==                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// ==                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// ==                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// ==                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// ==                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.Hosting.Jobs.Tests.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using FluentAssertions;

using Kwality.Hosting.Jobs.Abstractions;
using Kwality.Hosting.Jobs.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

using static Kwality.Hosting.Jobs.Tests.Properties.Traits.TraitTypes;
using static Kwality.Hosting.Jobs.Tests.Properties.Traits.TraitValues;

[Trait(Functionality, DI)]
public sealed class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "An `IHostedService` is registered when registering a `Job`.")]
    internal void AddJobAddsAnIHostedServiceInstance()
    {
        // ARRANGE.
        var serviceCollection = new ServiceCollection();

        // ACT.
        using ServiceProvider serviceProvider = serviceCollection
            .AddJob<TestableJob, TestableLock>()
            .BuildServiceProvider();

        // ASSERT.
        _ = serviceProvider.GetService<IHostedService>()
            .Should()
            .BeOfType<TestableJob>();
    }

    [Fact(DisplayName = "An `ILock` is registered when registering a `Job`.")]
    internal void AddJobAddsAnILockInstance()
    {
        // ARRANGE.
        var serviceCollection = new ServiceCollection();

        // ACT.
        using ServiceProvider serviceProvider = serviceCollection
            .AddJob<TestableJob, TestableLock>()
            .BuildServiceProvider();

        // ASSERT.
        _ = serviceProvider.GetService<ILock>()
            .Should()
            .BeOfType<TestableLock>();
    }

    [SuppressMessage("", "CA1812", Justification = "This class is used for testing.")]
    private sealed class TestableJob : Job
    {
        public TestableJob(ILock @lock)
            : base(@lock)
        {
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [SuppressMessage("", "CA1812", Justification = "This class is used for testing.")]
    private sealed class TestableLock : ILock
    {
        public bool IsLocked()
        {
            throw new NotImplementedException();
        }

        public void Lock()
        {
            throw new NotImplementedException();
        }

        public void Unlock()
        {
            throw new NotImplementedException();
        }
    }
}
