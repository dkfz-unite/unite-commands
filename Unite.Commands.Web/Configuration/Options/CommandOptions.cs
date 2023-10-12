namespace Unite.Commands.Web.Configuration.Options;

public class CommandOptions
{
    /// <summary>
    /// Command to execute (e.g. 'sh' or 'command.sh').
    /// Command always source directory as working directory. 
    /// </summary>
    public string Command
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_COMMAND");
            
            if (string.IsNullOrWhiteSpace(option))
                throw new Exception("'UNITE_COMMAND' environment variable has to be set");

            return option.Trim();
        }
    }

    /// <summary>
    /// Command arguments (e.g. '{src}/script.sh -i {data}/{proc}_data.tsv} -o {data}/{proc}_result.tsv').
    /// {src} will be replaced with the source directory path.
    /// {data} will be replaced with the data directory path.
    /// {proc} will be replaced with the process key.
    /// </summary>
    public string Arguments
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_COMMAND_ARGUMENTS");

            return option?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// Path to the source directory.
    /// </summary> 
    public string SourcePath
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_SOURCE_PATH");

            return option?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// Path to the data directory.
    /// </summary>
    public string DataPath
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_DATA_PATH");

            return option?.Trim() ?? string.Empty;
        }
    }

    /// <summary>
    /// Maximum amount of processes to run at the same time.
    /// </summary>
    public int? Limit
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_PROCESS_LIMIT");

            if (string.IsNullOrWhiteSpace(option))
                return null;

            if (!int.TryParse(option, out var value))
                throw new Exception("'UNITE_PROCESS_LIMIT' environment variable has to be an integer");

            if (value <= 0)
                throw new Exception("'UNITE_PROCESS_LIMIT' environment variable has to be greater than 0");

            return value;
        }
    }
}
