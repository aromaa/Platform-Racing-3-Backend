using Newtonsoft.Json;
using Platform_Racing_3_Common.User;
using Platform_Racing_3_Common.Utils;
using Platform_Racing_3_Server.Collections;
using Platform_Racing_3_Server.Core;
using Platform_Racing_3_Server.Game.Client;
using Platform_Racing_3_Server.Game.Communication.Messages.Incoming.Json;
using Platform_Racing_3_Server.Game.Communication.Messages.Outgoing;
using Platform_Racing_3_Server.Game.Communication.Messages.Outgoing.Json;
using Platform_Racing_3_Server.Game.Communication.Messages.Outgoing.Json.Rooms;
using Platform_Racing_3_Server.Game.User.Identifiers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform_Racing_3_Common.Redis;
using StackExchange.Redis;

namespace Platform_Racing_3_Server.Game.Chat
{
    internal class ChatRoom
    {
        private const uint MAX_RECENT_MESSAGES = 25;

        internal ChatRoomType Type { get; }

        internal uint CreatorUserId { get; }
        [JsonProperty("creator")]
        internal string CreatorUsername => this.CreatorUserId == 0 ? null : UserManager.TryGetUserDataByIdAsync(this.CreatorUserId).Result?.Username ?? "Unknown";

        [JsonProperty("roomName")]
        internal string Name { get; }

        internal string Pass { get; set; }
        [JsonProperty("note")]
        internal string Note { get; set; }

        private ClientSessionCollection Clients;
        private ConcurrentBag<IUserIdentifier> BannedClients;

        [JsonProperty("members")]
        internal uint MembersCount => this.Clients.Count;

        private ConcurrentQueue<ChatOutgoingMessage> RecentMessages;

        internal ChatRoom(ChatRoomType type, string name, string pass, string note, params ClientSession[] clientSessions) : this(type, 0, name, pass, note, clientSessions)
        {
        }

        internal ChatRoom(ChatRoomType type, uint creatorUserId, string name, string pass, string note, params ClientSession[] clientSessions)
        {
            this.Clients = new ClientSessionCollection(this.OnDisconnect);
            this.BannedClients = new ConcurrentBag<IUserIdentifier>();

            this.RecentMessages = new ConcurrentQueue<ChatOutgoingMessage>();

            this.CreatorUserId = creatorUserId;
            this.Type = type;
            this.Name = name;
            this.Pass = pass;
            this.Note = note;

            foreach(ClientSession session in clientSessions)
            {
                this.Join(session);
            }
        }

        internal ICollection<ClientSession> Members => this.Clients.Values;

        private void OnDisconnect(ClientSession session)
        {
            this.Leave(session, ChatRoonLeaveReason.Disconnected);
        }

        internal ChatRoomJoinStatus Join(ClientSession session, uint chatId = 0)
        {
            //First send the room vars
            session.SendPacket(new RoomVarsOutgoingMessage(chatId, this.Name, this.GetVars("creator", "note")));

            if (this.BannedClients.Any((i) => i.Matches(session.UserData.Id, session.SocketId, session.IPAddres)))
            {
                return ChatRoomJoinStatus.Banned; //L
            }

            if (this.Clients.Add(session))
            {
                foreach(ClientSession other in this.Clients.Values)
                {
                    other.TrackUserInRoom(this.Name, session.SocketId, session.UserData.Id, session.UserData.Username, session.UserData.GetVars("id"));
                }

                session.SendPackets(this.RecentMessages); //Sent recent messages

                return ChatRoomJoinStatus.Success;
            }
            else
            {
                return ChatRoomJoinStatus.Success;
            }
        }

        internal void Leave(ClientSession session, ChatRoonLeaveReason reason)
        {
            if (this.Clients.Remove(session))
            {
                foreach(ClientSession other in this.Clients.Values)
                {
                    if (reason == ChatRoonLeaveReason.Kicked || other != session)
                    {
                        other.UntrackUserInRoom(this.Name, session.SocketId);
                    }
                }

                if (this.Clients.Count <= 0)
                {
                    PlatformRacing3Server.ChatRoomManager.Die(this);
                }
            }
        }

        internal void HandleData(ClientSession session, JsonSendToRoomIncomingMessage.RoomMessageData data, bool sendToSelf = true)
        {
            if (this.Clients.Contains(session))
            {
                switch(data.Type)
                {
                    case "chat":
                        {
                            if (!session.IsGuest)
                            {
                                this.SendChatMessage(session, (string)data.Data["message"], sendToSelf);
                            }
                        }
                        break;
                }
            }
        }

        private void SendChatMessage(ClientSession session, string message, bool sendToSelf = true)
        {
            message = message.Trim();

            if (message.StartsWith('/'))
            {
                string[] args = message.Substring(1).Split(' ');

                if (!PlatformRacing3Server.CommandManager.Execte(session, args[0], args.AsSpan(start: 1, length: args.Length - 1)))
                {
                    session.SendPacket(new AlertOutgoingMessage("Unknown command; please type /help for a list of the available commands."));
                }
            }
            else if (message.Length > 0)
            {
                ChatOutgoingMessage packet = new ChatOutgoingMessage(this.Name, message, session.SocketId, session.UserData.Id, session.UserData.Username, session.UserData.NameColor);

                this.RecentMessages.Enqueue(packet);
                while (this.RecentMessages.Count > ChatRoom.MAX_RECENT_MESSAGES)
                {
                    this.RecentMessages.TryDequeue(out _);
                }

                if (sendToSelf)
                {
                    this.Clients.SendPacket(packet);
                }
                else
                {
                    this.Clients.SendPacket(packet, session);
                }

                if (this.Name == "chat-Home")
                {
                    PlatformRacing3Server.DiscordChatWebhook?.SendMessageAsync(text: $"`{message.Replace('`', '\'')}`", username: session.UserData.Username);
                }
            }
        }

        //Even tho the client has some funcitonality for this, its not properly supported, have some funcitonality for it tho
        internal void Ban(ClientSession session, ClientSession target)
        {
            if (session.IsLoggedIn && target.IsLoggedIn && !session.IsGuest && session.UserData.Id == this.CreatorUserId)
            {
                if (target.IsGuest)
                {
                    this.BannedClients.Add(new GuestIdentifier(target.SocketId, target.IPAddres));
                }
                else
                {
                    this.BannedClients.Add(new PlayerIdentifier(target.UserData.Id));
                }

                this.Leave(target, ChatRoonLeaveReason.Kicked); //BYE BYE!
            }
        }

        internal IReadOnlyDictionary<string, object> GetVars(params string[] vars) => JsonUtils.GetVars(this, vars);
        internal IReadOnlyDictionary<string, object> GetVars(HashSet<string> vars) => JsonUtils.GetVars(this, vars);
    }
}
