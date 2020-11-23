using Discord;
using Discord.Commands;
using Platform_Racing_3_Common.Customization;
using Platform_Racing_3_Common.Extensions;
using Platform_Racing_3_Common.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class MyPartsCommand : ModuleBase<SocketCommandContext>
    {
        [Command("pr3parts")]
        [Summary("Returns a list of obtainable PR3 parts and hats.")]
        public Task GetOnlinePlayersCount()
        {
            return DoAsync();

            async Task DoAsync()
            {
                uint userId = await UserManager.HasDiscordLinkage(this.Context.User.Id);
                if (userId == 0)
                {
                    await this.ReplyAsync("Well bruh, u ain't got ur Discord linkage");

                    return;
                }

                PlayerUserData userData = await UserManager.TryGetUserDataByIdAsync(userId);

                (int Count, int Max) hats = default;

                StringBuilder hatsWriter = new StringBuilder();
                foreach (Hat hat in Enum.GetValues(typeof(Hat)))
                {
                    if (hat == Hat.None || hat.IsStaffOnly())
                    {
                        continue;
                    }

                    hats.Max++;

                    hatsWriter.Append(hat.ToString());
                    hatsWriter.Append(" ");

                    if (userData.HasHat(hat))
                    {
                        hatsWriter.Append("✓");

                        hats.Count++;
                    }
                    else
                    {
                        hatsWriter.Append("✕");
                    }

                    hatsWriter.AppendLine();
                }

                (int Head, int Body, int Feet, int Max) parts = default;
                (int Head, int Body, int Feet, int Max) tournieParts = default;

                (StringBuilder Head, StringBuilder Body, StringBuilder Feet) partWriters = (new StringBuilder(), new StringBuilder(), new StringBuilder());
                (StringBuilder Head, StringBuilder Body, StringBuilder Feet) tourniePartWriters = (new StringBuilder(), new StringBuilder(), new StringBuilder());

                foreach (Part part in Enum.GetValues(typeof(Part)))
                {
                    if (part == Part.None || part.IsStaffOnly())
                    {
                        continue;
                    }

                    bool isTournie = part.IsTournamentPrize();

                    CountTo(ref !isTournie ? ref partWriters : ref tourniePartWriters, ref !isTournie ? ref parts : ref tournieParts);

                    void CountTo(ref (StringBuilder Head, StringBuilder Body, StringBuilder Feet) writers, ref (int Head, int Body, int Feet, int Max) partCounter)
                    {
                        partCounter.Max++;

                        AppendToAll(ref writers, part.ToString());
                        AppendToAll(ref writers, " ");

                        if (userData.HasHead(part))
                        {
                            writers.Head.Append("✓");

                            partCounter.Head++;
                        }
                        else
                        {
                            writers.Head.Append("✕");
                        }

                        if (userData.HasBody(part))
                        {
                            writers.Body.Append("✓");

                            partCounter.Body++;
                        }
                        else
                        {
                            writers.Body.Append("✕");
                        }

                        if (userData.HasFeet(part))
                        {
                            writers.Feet.Append("✓");

                            partCounter.Feet++;
                        }
                        else
                        {
                            writers.Feet.Append("✕");
                        }

                        BreakAll(ref writers);
                    }

                    void AppendToAll(ref (StringBuilder Head, StringBuilder Body, StringBuilder Feet) writers, string value)
                    {
                        writers.Head.Append(value);
                        writers.Body.Append(value);
                        writers.Feet.Append(value);
                    }

                    void BreakAll(ref (StringBuilder Head, StringBuilder Body, StringBuilder Feet) writers)
                    {
                        writers.Head.AppendLine();
                        writers.Body.AppendLine();
                        writers.Feet.AppendLine();
                    }
                }

                EmbedBuilder embed = new EmbedBuilder();

                embed.WithTitle("Parts List");
                embed.WithColor(Color.Blue);
                embed.WithAuthor(author = > { author.WithName(this.Context.User.Username + this.Context.User.Discriminator).WithIconUrl(this.Context.User.GetAvatarUrl()) });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Hats ({hats.Count}/{hats.Max})",
                    Value = hatsWriter.ToString()
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Heads ({parts.Head}/{parts.Max})",
                    Value = partWriters.Head.ToString(),

                    IsInline = true
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Bodies ({parts.Body}/{parts.Max})",
                    Value = partWriters.Body.ToString(),

                    IsInline = true
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Feets ({parts.Feet}/{parts.Max})",
                    Value = partWriters.Feet.ToString(),

                    IsInline = true
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Tournie Heads ({tournieParts.Head}/{tournieParts.Max})",
                    Value = tourniePartWriters.Head.ToString(),

                    IsInline = true
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Tournie Bodies ({tournieParts.Body}/{tournieParts.Max})",
                    Value = tourniePartWriters.Body.ToString(),

                    IsInline = true
                });

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"Tournie Feets ({tournieParts.Feet}/{tournieParts.Max})",
                    Value = tourniePartWriters.Feet.ToString(),

                    IsInline = true
                });

                await this.Context.User.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
