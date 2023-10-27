using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Unite.Commands.Web.Configuration.Options;

namespace Unite.Commands.Web.Controllers;

[Route("api/run")]
public class CommandController : Controller
{
    private const string _src_path_key = "{src}"; // All entries for {src} will be replaced with the value of _options.SourcePath
    private const string _data_path_key = "{data}"; // All entries for {data} will be replaced with the value of _options.DataPath
    private const string _process_key = "{proc}"; // All entries for {proc} will be replaced with the value of the key parameter
    private static int _process_number = 0;

    private readonly CommandOptions _options;
    private readonly ILogger _logger;


    public CommandController(
        CommandOptions options, 
        ILogger<CommandController> logger)
    {
        _options = options;
        _logger = logger;
    }


    // Process current directory: _options.SourcePath
    // sh script.sh -i input.txt -o output.txt 
    // sh script.sh -i {data}/{proc}_input.txt -o {data}/{proc}_output.txt
    [HttpPost]
    public async Task<IActionResult> Run(string key)
    {
        if (_options.Limit.HasValue && _process_number >= _options.Limit)
            return StatusCode(501, "Processes limit exceeded");

        var stopwatch = new Stopwatch();
        var command = PrepareCommand(_options.Command, key);
        var arguments = PrepareArguments(_options.Arguments, key);
        var process = PrepareProcess(command, arguments);

        try
        {
            _process_number++;
            _logger.LogInformation("Starting process '{key}' ({number}/{limit})", key ?? "-", _process_number.ToString(), _options.Limit?.ToString() ?? "-");

            stopwatch.Start();
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var errors = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            stopwatch.Stop();

            if (process.ExitCode > 0)
            {
                _logger.LogError("Process finished with exit code {code}", process.ExitCode);
                throw new Exception(errors);
            }
            else
            {
                _logger.LogInformation("Process finished in {seconds}s", Math.Round(stopwatch.Elapsed.TotalSeconds, 2));
                return Ok(output);
            }
        }
        catch (Exception exception)
        {
            LogError(exception);

            return StatusCode(500);
        }
        finally
        {
            stopwatch.Reset();
            process.Dispose();
            _process_number --;
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
        process.StartInfo.RedirectStandardError = true;

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
