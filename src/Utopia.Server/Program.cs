// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Server;

public static class Program
{
    private const string Art =
        """
                                      .-'''-.
                             '   _    \
                           /   /` '.   \_________   _...._      .--.
                          .   |     \  '\        |.'      '-.   |__|
                       .| |   '      |  '\        .'```'.    '. .--.
                     .' |_\    \     / /  \      |       \     \|  |    __
           _    _  .'     |`.   ` ..' /    |     |        |    ||  | .:--.'.
          | '  / |'--.  .-'   '-...-'`     |      \      /    . |  |/ |   \ |
         .' | .' |   |  |                  |     |\`'-.-'   .'  |  |`" __ | |
         /  | /  |   |  |                  |     | '-....-'`    |__| .'.''| |
        |   `'.  |   |  '.'               .'     '.                 / /   | |_
        '   .'|  '/  |   /              '-----------'               \ \._,\ '/
         `-'  `--'   `'-'                                            `--'  `"
        """;

    public static void Main(string[] args)
    {
        Console.WriteLine(Art);
        var opt = Launcher.ParseOptions(args);
        Launcher.Launch(opt, out _).Wait();
    }
}
