using System;
using System.IO;
using S14001ASpeak.Audio;
using S14001ASpeak.Cli;
using S14001ASpeak.Speech;

namespace S14001ASpeak;

/// <summary>
/// Entry point and composition root.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        CommandLineParseResult parsed = CommandLineParser.Parse(args);

        if (parsed.Options is null)
        {
            Console.Error.WriteLine(parsed.Error);
            Console.Error.WriteLine(UsageText.HelpHint);
            return (int)ExitCode.InvalidArguments;
        }

        if (parsed.Options.ShowHelp)
        {
            Console.Out.Write(UsageText.Text);
            return (int)ExitCode.Success;
        }

        return (int)Speak(parsed.Options);
    }

    private static ExitCode Speak(CommandLineOptions options)
    {
        try
        {
            CreatePipeline(options).Speak(options.Text, options.Voice);
            return ExitCode.Success;
        }
        catch (SpeechSourceException exception)
        {
            return Fail(exception, ExitCode.SpeechSourceFailed);
        }
        catch (NoSpeechException exception)
        {
            return Fail(exception, ExitCode.NothingToSay);
        }
        catch (Exception exception) when (exception is AudioPlaybackException or IOException or UnauthorizedAccessException)
        {
            return Fail(exception, ExitCode.OutputFailed);
        }
    }

    private static SpeechPipeline CreatePipeline(CommandLineOptions options)
    {
        IAudioOutput output = options.OutputFile is null
            ? new SpeakerOutput()
            : FileOutputFactory.Create(options.OutputFile);

        return new SpeechPipeline(new WindowsSpeechSource(), new S14001ASpeechRenderer(), output);
    }

    private static ExitCode Fail(Exception exception, ExitCode exitCode)
    {
        Console.Error.WriteLine(exception.Message);
        return exitCode;
    }
}
