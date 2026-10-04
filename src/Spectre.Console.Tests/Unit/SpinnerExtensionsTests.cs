namespace Spectre.Console.Tests.Unit;

public sealed class SpinnerExtensionsTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, -1)]
    [InlineData(true, -1)]
    public async Task Should_Complete_When_The_Spinner_Stops(bool returnsResult, int intervalMilliseconds)
    {
        for (var iteration = 0; iteration < 1000; iteration++)
        {
            // Given
            var console = new TestConsole();
            var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var spinner = new CompletingSpinner(completion, intervalMilliseconds);

            // When
            if (returnsResult)
            {
                var result = await completion.Task.Spinner(spinner, ansiConsole: console)
                    .WaitAsync(TimeSpan.FromSeconds(10));

                // Then
                result.ShouldBe(42);
            }
            else
            {
                await ((Task)completion.Task).Spinner(spinner, ansiConsole: console)
                    .WaitAsync(TimeSpan.FromSeconds(10));
            }
        }
    }

    [Fact]
    public async Task Should_Preserve_The_Task_Exception()
    {
        var exception = new InvalidOperationException("Task failed");
        var console = new TestConsole();

        var actual = await Should.ThrowAsync<InvalidOperationException>(
            () => Task.FromException(exception).Spinner(ansiConsole: console));

        actual.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task Should_Preserve_Task_Cancellation()
    {
        var console = new TestConsole();
        var token = new System.Threading.CancellationToken(true);

        var exception = await Should.ThrowAsync<TaskCanceledException>(
            () => Task.FromCanceled(token).Spinner(ansiConsole: console));

        exception.CancellationToken.ShouldBe(token);
    }

    private sealed class CompletingSpinner(TaskCompletionSource<int> completion, int intervalMilliseconds) : Spinner
    {
        public override TimeSpan Interval
        {
            get
            {
                completion.TrySetResult(42);
                return TimeSpan.FromMilliseconds(intervalMilliseconds);
            }
        }

        public override bool IsUnicode => false;

        public override IReadOnlyList<string> Frames => new[] { "." };
    }
}