﻿using Newtonsoft.Json;
using Platform_Racing_3_Common.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Bot.Config
{
    internal class DiscordBotConfig : IDatabaseConfig, IRedisConfig
    {
        [JsonProperty("bot_token", Required = Required.Always)]
        internal string BotToken { get; set; }

        [JsonProperty("database_host", Required = Required.Always)]
        public string DatabaseHost { get; set; }
        [JsonProperty("database_port", Required = Required.Always)]
        public uint DatabasePort { get; set; }

        [JsonProperty("database_user", Required = Required.Always)]
        public string DatabaseUser { get; set; }
        [JsonProperty("database_pass", Required = Required.Always)]
        public string DatabasePass { get; set; }
        [JsonProperty("database_name", Required = Required.Always)]
        public string DatabaseName { get; set; }

        [JsonProperty("redis_host", Required = Required.Always)]
        public string RedisHost { get; set; }
        [JsonProperty("redis_port", Required = Required.Always)]
        public uint RedisPort { get; set; }
    }
}
