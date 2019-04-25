﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Miunie.Core;
using Miunie.Core.Logging;

namespace Miunie.Discord
{
    public class Impersonation : IDiscordImpersonation
    {
        private readonly IDiscord _discord;
        private readonly ILogWriter _logger;

        public Impersonation(IDiscord discord, ILogWriter logger)
        {
            _discord = discord;
            _logger = logger;
        }

        public IEnumerable<GuildView> GetAvailableGuilds()
            =>_discord.Client?.Guilds.Select(g => new GuildView
            {
                Id = g.Value.Id,
                IconUrl = g.Value.IconUrl,
                Name = g.Value.Name
            });

        public async Task<IEnumerable<TextChannelView>> GetAvailableTextChannelsAsync(ulong guildId)
        {
            var guild = await _discord.Client.GetGuildAsync(guildId);
            var textChannels = guild.Channels.Where(c => c.Type == ChannelType.Text);
            var result = new List<TextChannelView>();
            foreach (var channel in textChannels)
            {
                try
                {
                    result.Add(new TextChannelView
                    {
                        Id = channel.Id,
                        Name = $"# {channel.Name}",
                        Messages = await GetMessagesFrom(channel)
                    });
                }
                catch (Exception)
                {
                    _logger.Log($"Miunie cannot read from the '{channel.Name}' channel.");
                }
            }

            return result;
        }

        public async Task<IEnumerable<MessageView>> GetMessagesFrom(DiscordChannel channel)
        {
            var msgs = await channel.GetMessagesAsync(10);
            return msgs.Select(m => new MessageView
            {
                AuthorAvatarUrl = m.Author.AvatarUrl,
                AuthorName = m.Author.Username,
                Content = m.Content,
                TimeStamp = m.CreationTimestamp.ToLocalTime()
            });
        }
    }
}
