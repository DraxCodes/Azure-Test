﻿using DSharpPlus;

namespace Miunie.Discord
{
    public interface IDiscord
    {
        DiscordClient Client { get; }
        void Initialize();
        void DisposeOfClient();
    }
}
