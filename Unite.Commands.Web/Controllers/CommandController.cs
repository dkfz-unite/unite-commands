using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Unite.Commands.Web.Configuration.Options;

namespace Unite.Commands.Web.Controllers;

[Route("api/run")]
public class CommandController : Controller
{
    private const string _src_path_key = "{src}"; // all entries for {src} will be replaced with the value of _options.SourcePath
    private const string _data_path_key = "{data}"; // all entries for {data} will be replaced with the value of _options.DataPath
    private const string _process_key = "{proc}"; // all entries for {proc} will be replaced with the value of the key parameter
    private static int _process_limit = 0;

    private readonly CommandOptions _options;
    private readonly ILogger _logger;


    public CommandController(
        CommandOptions options, 
        ILogger<CommandController> logger)
    {
        _options = options;
        _logger = logger;
    }


    // Process current directory is _options.SourcePath
    // Rscript script.R data.tsv metadata.tsv result.tsv
    // Rscript script.R {data}/{proc}_data.tsv {data}/{proc}_metadata.tsv {data}/{proc}_result.tsv
    [HttpPost] 
    public async Task<IActionResult> Run(string key)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (_options.Limit > 0 && _process_limit >= _options.Limit)
                return StatusCode(501, "Processes limit exceeded");

            var command = PrepareCommand(_options.Command, key);

            var arguments = PrepareArguments(_options.Arguments, key);

            var process = PrepareProcess(command, arguments);

            _logger.LogInformation($"Running command: {command} {arguments}");

            _process_limit++;
            
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            process.Dispose();

            return Ok(output);
        }
        catch (Exception exception)
        {
            LogError(exception);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                return StatusCode(500, exception.Message);
            else 
                return StatusCode(500, "Internal server error");
        }
        finally
        {
            stopwatch.Stop();

            _process_limit--;

            _logger.LogInformation($"Process finished in {stopwatch.ElapsedMilliseconds} ms");
        }
    }


    private string PrepareCommand(string command, string key)
    {
        if (!string.IsNullOrEmpty(_options.SourcePath))
            command = command.Replace(_src_path_key, $"{_options.SourcePath}");

        if (!string.IsNullOrEmpty(_options.DataPath))
            command = command.Replace(_data_path_key, $"{_options.DataPath}");

        if (!string.IsNullOrEmpty(key))
            command = command.Replace(_process_key, $"{key}");

        return command;
    }

    private string PrepareArguments(string arguments, string key)
    {
        if (!string.IsNullOrEmpty(_options.SourcePath))
            arguments = arguments.Replace(_src_path_key, $"{_options.SourcePath}");

        if (!string.IsNullOrEmpty(_options.DataPath))
            arguments = arguments.Replace(_data_path_key, $"{_options.DataPath}");

        if (!string.IsNullOrEmpty(key))
            arguments = arguments.Replace(_process_key, $"{key}");

        return arguments;
    }

    private Process PrepareProcess(string command, string arguments)
    {
        var process = new Process();

        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        if (!string.IsNullOrEmpty(_options.SourcePath))
            process.StartInfo.WorkingDirectory = _options.SourcePath;

        return process;
    }

    private void LogError(Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        if (exception.InnerException != null)
            LogError(exception.InnerException);
    }
}
