namespace AngeloBreuerApi;

using System.Collections.Immutable;
using System.Threading;
using Microsoft.Extensions.Options;

internal sealed class DataService
{
    private readonly GitHubClient _githubClient;
    private readonly IOptions<AppConfiguration> _options;
    private TaskCompletionSource<IImmutableDictionary<string, ProjectProperties>>? _projectInformationTaskCompletionSource;
    private IImmutableDictionary<string, ProjectProperties>? _data;
    private DateTimeOffset _lastTimeFetched;

    public DataService(GitHubClient githubClient, IOptions<AppConfiguration> options)
    {
        _githubClient = githubClient ?? throw new ArgumentNullException(nameof(githubClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask<IImmutableDictionary<string, ProjectProperties>> FetchAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_data is null && _projectInformationTaskCompletionSource is not null && !_projectInformationTaskCompletionSource!.Task.IsCompleted)
        {
            return await _projectInformationTaskCompletionSource.Task.ConfigureAwait(false);
        }

        // GitHub allows up to 60 requests/hour
        if (DateTimeOffset.UtcNow - _lastTimeFetched > TimeSpan.FromMinutes(5))
        {
            _projectInformationTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, ProjectProperties>();

                foreach (var projectName in _options.Value.Projects)
                {
                    var projectProperties = await _githubClient
                        .FetchAsync(projectName, cancellationToken)
                        .ConfigureAwait(false);

                    dictionaryBuilder.Add(projectName, projectProperties);
                }

                _lastTimeFetched = DateTimeOffset.UtcNow;
                _data = dictionaryBuilder.ToImmutable();
                _projectInformationTaskCompletionSource.SetResult(_data);
            }
            catch (Exception exception)
            {
                _projectInformationTaskCompletionSource.SetException(exception);
                throw;
            }
        }

        return _data!;
    }
}
