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
namespace Kwality.Hosting.Jobs.Tests;

using System.Diagnostics.CodeAnalysis;

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.Hosting.Jobs.Abstractions;

using Moq;

using Xunit;

using static Kwality.Hosting.Jobs.Tests.Properties.Traits.TraitTypes;
using static Kwality.Hosting.Jobs.Tests.Properties.Traits.TraitValues;

[Trait(Functionality, Execution)]
public sealed class JobTests
{
    [Theory(DisplayName = "The `Job` is NOT executed when the `ILock` is \"Locked\".")]
    [AutoDomainData]
    internal async Task TheJobIsNotExecutedWhenTheLockIsLocked([Frozen] Mock<ILock> lockMock,
        [Frozen] Mock<Job> jobMock)
    {
        // VALIDATION.
        ILock @lock = lockMock?.Object ?? throw new ArgumentNullException(nameof(lockMock));
        Job job = jobMock?.Object ?? throw new ArgumentNullException(nameof(jobMock));

        // MOCK SETUP.
        _ = lockMock.Setup(x => x.IsLocked())
            .Returns(true);

        // ACT.
        await job.StartAsync(new CancellationToken())
            .ConfigureAwait(false);

        // ASSERT.
        jobMock.Verify(job => job.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "The `Job` is executed when the `ILock` is \"NOT Locked\".")]
    [AutoDomainData]
    internal async Task TheJobIsExecutedWhenTheLockIsNotLocked([Frozen] Mock<ILock> lockMock,
        [Frozen] Mock<Job> jobMock)
    {
        // VALIDATION.
        ILock @lock = lockMock?.Object ?? throw new ArgumentNullException(nameof(lockMock));
        Job job = jobMock?.Object ?? throw new ArgumentNullException(nameof(jobMock));

        // MOCK SETUP.
        _ = lockMock.Setup(x => x.IsLocked())
            .Returns(false);

        // ACT.
        await job.StartAsync(new CancellationToken())
            .ConfigureAwait(false);

        // ASSERT.
        jobMock.Verify(job => job.ExecuteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "The `ILock` is \"Locked\" before the `Job` is executed.")]
    internal async Task TheLockIsLockedBeforeTheJobIsExecuted()
    {
        // ARRANGE.
        var @lock = new TestableLock();
        var job = new TestableJob(@lock);

        // ACT.
        await job.StartAsync(new CancellationToken())
            .ConfigureAwait(false);

        // ASSERT.
        _ = job.IsLockedBeforeJobExecution
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "The `ILock` is \"Unlocked\" once the `Job` is executed.")]
    internal async Task TheLockIsUnlockedOnceTheJobIsExecuted()
    {
        // ARRANGE.
        var @lock = new TestableLock();
        var job = new TestableJob(@lock);

        // ACT.
        await job.StartAsync(new CancellationToken())
            .ConfigureAwait(false);

        // ASSERT.
        _ = @lock.IsLocked()
            .Should()
            .BeFalse();
    }

    [Theory(DisplayName = "The `ILock` is \"Unlocked\" when the `Job` is stopped.")]
    [AutoDomainData]
    internal async Task TheLockIsUnlockedWhenTheJobIsStopped([Frozen] Mock<ILock> lockMock, [Frozen] Mock<Job> jobMock)
    {
        // VALIDATION.
        ILock @lock = lockMock?.Object ?? throw new ArgumentNullException(nameof(lockMock));
        Job job = jobMock?.Object ?? throw new ArgumentNullException(nameof(jobMock));

        // ACT.
        await job.StopAsync(new CancellationToken())
            .ConfigureAwait(false);

        // ASSERT.
        lockMock.Verify(@lock => @lock.Unlock(), Times.Once);
    }

    [SuppressMessage("", "CA1031", Justification = "Exception is NOT required here.")]
    [Theory(DisplayName = "The `ILock` is \"Unlocked\" when an exception is raised during the execution of the `Job`.")]
    [AutoDomainData]
    internal async Task TheLockIsUnlockedWhenAnExceptionIsRaisedDuringTheExecutionOfTheJob(
        [Frozen] Mock<ILock> lockMock,
        [Frozen] Mock<Job> jobMock)
    {
        // VALIDATION.
        ILock @lock = lockMock?.Object ?? throw new ArgumentNullException(nameof(lockMock));
        Job job = jobMock?.Object ?? throw new ArgumentNullException(nameof(jobMock));

        // MOCK SETUP.
        _ = jobMock.Setup(@mock => mock.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Throws<ArgumentOutOfRangeException>();

        // ACT.
        try
        {
            await job.StartAsync(new CancellationToken())
                .ConfigureAwait(false);
        }
        catch
        {
            // Suppression of CA1031: In scope of this test, it's NOT required to inspect the value of the exception.
        }

        // ASSERT.
        lockMock.Verify(@lock => @lock.Unlock(), Times.Once);
    }

    [Theory(DisplayName = "The exception that is raised during the execution of the `Job`, is throwed.")]
    [AutoDomainData]
    internal async Task TheExceptionThatIsRaisedDuringTheExecutionOfTheJobIsThrowed(
        [Frozen] Mock<ILock> lockMock,
        [Frozen] Mock<Job> jobMock,
        Exception exception)
    {
        // VALIDATION.
        ILock @lock = lockMock?.Object ?? throw new ArgumentNullException(nameof(lockMock));
        Job job = jobMock?.Object ?? throw new ArgumentNullException(nameof(jobMock));

        // MOCK SETUP.
        _ = jobMock.Setup(@mock => mock.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Throws(exception);

        // ACT / ASSERT.
        Func<Task> act = async () => await job.StartAsync(new CancellationToken())
                .ConfigureAwait(false);

        _ = await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage(exception.Message)
            .ConfigureAwait(false);
    }

    internal sealed class TestableJob : Job
    {
        private readonly ILock @lock;

        public TestableJob(ILock @lock)
            : base(@lock)
        {
            this.@lock = @lock;
        }

        public bool IsLockedBeforeJobExecution
        {
            get; set;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.IsLockedBeforeJobExecution = this.@lock.IsLocked();

            return Task.CompletedTask;
        }
    }

    internal sealed class TestableLock : ILock
    {
        private bool isLockedFlag;

        public bool IsLocked()
        {
            return this.isLockedFlag;
        }

        public void Lock()
        {
            this.isLockedFlag = true;
        }

        public void Unlock()
        {
            this.isLockedFlag = false;
        }
    }

    private sealed class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(CreateFixture)
        {
        }

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            _ = fixture.Customize(new AutoMoqCustomization());

            fixture.Customize<Mock<Job>>(
                composer => composer.FromFactory(
                    () => new Mock<Job>(fixture.Create<ILock>()))
                .OmitAutoProperties());

            return fixture;
        }
    }
}
