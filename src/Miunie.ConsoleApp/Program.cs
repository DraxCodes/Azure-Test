﻿using Microsoft.Extensions.DependencyInjection;
using Miunie.ConsoleApp.Configuration;
using Miunie.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Miunie.ConsoleApp
{
    internal static class Program
    {
        private static MiunieBot _miunie;
        private static ConfigManager _configManager;
        private static ConfigurationFileEditor _editor;

        private static async Task Main(string[] args)
        {
            _miunie = ActivatorUtilities.CreateInstance<MiunieBot>(InversionOfControl.Provider);

            if (args.Contains("-headless")) { await RunHeadless(args); }

            _configManager = InversionOfControl.Provider.GetRequiredService<ConfigManager>();
            _editor = InversionOfControl.Provider.GetRequiredService<ConfigurationFileEditor>();
            _miunie.MiunieDiscord.ConnectionChanged += MiunieOnConnectionStateChanged;
            HandleInput();
        }

        private static async Task RunHeadless(string[] args)
        {
            if (!args.Any(arg => arg.StartsWith("-token=")))
            {
                Console.WriteLine(ConsoleStrings.HEADLESS_REQUIRES_TOKEN);
                Environment.Exit(0);
            }

            var token = args
                .First(arg => arg.StartsWith("-token="))
                .Substring(7);

            _miunie.BotConfiguration.DiscordToken = token;
            await _miunie.StartAsync();
        }

        private static void MiunieOnConnectionStateChanged(object sender, EventArgs e)
        {
            DrawMiunieState();
        }

        private static void HandleInput()
        {
            while (true)
            {
                DisplayMenu();
                DrawMiunieState();
                Console.Write(ConsoleStrings.PLEASE_ENTER_MENU_NUMBER);
                var success = int.TryParse(Console.ReadLine(), out var choice);

                if (!success)
                {
                    Console.WriteLine(ConsoleStrings.ERROR_CHOICE_NOT_A_NUMBER);
                }

                switch (choice)
                {
                    case 1:
                    {
                        string token;
                        do
                        {
                            Console.Clear();
                            Console.Write(ConsoleStrings.ENTER_TOKEN);
                            token = Console.ReadLine();

                            Console.Clear();
                            Console.WriteLine(ConsoleStrings.IS_TOKEN_CORRECT, token);
                            Console.WriteLine(ConsoleStrings.YES_NO_PROMPT);
                        } while (Console.ReadKey().Key != ConsoleKey.Y);

                        _editor.WriteSetting("DiscordToken", token);
                        _editor.Save();
                        _miunie.BotConfiguration.DiscordToken = token;
                        break;
                    }
                    case 2:
                    {
                        if (_miunie.MiunieDiscord.ConnectionState == ConnectionState.CONNECTED)
                        {
                            _miunie.Stop();
                        }
                        else if(_miunie.MiunieDiscord.ConnectionState == ConnectionState.DISCONNECTED)
                        {
                            _miunie.BotConfiguration.DiscordToken = _configManager.GetValueFor("DiscordToken");
                            _ = _miunie.StartAsync();
                        }
                        break;
                    }
                    case 3:
                    {
                        Environment.Exit(0);
                        break;
                    }
                    default:
                    {
                        Console.WriteLine(ConsoleStrings.UNKNOWN_OPTION_SELECTED);
                        AnyKeyToContinue();
                        break;
                    }
                }
            }
        }

        public static void DisplayMenu()
        {
            Console.Clear();
            PrintHeader();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(ConsoleStrings.MAIN_MENU_OPTIONS);
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(ConsoleStrings.MIUNIE_ASCII_HEADER);
            Console.ResetColor();
        }

        private static void AnyKeyToContinue()
        {
            Console.WriteLine(ConsoleStrings.ANY_KEY_TO_CONTINUE);
            Console.ReadKey();
        }

        private static void DrawMiunieState()
        {
            var prevCursorLeft = Console.CursorLeft;
            var prevCursorTop = Console.CursorTop;

            var msg = _miunie.MiunieDiscord.ConnectionState == ConnectionState.CONNECTED 
                ? ConsoleStrings.BOT_IS_RUNNING 
                : ConsoleStrings.BOT_IS_NOT_RUNNING;

            var left = Math.Clamp(Console.WindowWidth - msg.Length, 0, Console.WindowWidth);

            Console.SetCursorPosition(left, 0);
            ClearCurrentConsoleLine();
            Console.Write(msg);
            Console.SetCursorPosition(prevCursorLeft, prevCursorTop);
        }

        public static void ClearCurrentConsoleLine()
        {
            var currentTop = Console.CursorTop;
            var currentLeft = Console.CursorLeft;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(currentLeft, currentTop);
        }
    }
}
