﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using log4net;
using Platform_Racing_3_Common.Database;
using Platform_Racing_3_Common.Server;
using Platform_Racing_3_Common.User;
using Platform_Racing_3_Server.Core;
using Platform_Racing_3_Server.Game.Client;
using Platform_Racing_3_Server.Game.Communication.Messages.Incoming.Json;
using Platform_Racing_3_Server.Game.Communication.Messages.Outgoing;
using Platform_Racing_3_Server.Game.Communication.Messages.Outgoing.Json;

namespace Platform_Racing_3_Server.Game.Communication.Messages.Incoming
{
    internal class GuestLoginIncomingMessage : IMessageIncomingJson
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Handle(ClientSession session, JsonPacket message)
        {
            if (session.UpgradeClientStatus(ClientStatus.LoggedIn))
            {
                DatabaseConnection.NewAsyncConnection((dbConnection) => dbConnection.ReadDataAsync($"INSERT INTO base.users_logins(user_id, ip, data, type) VALUES(0, {session.IPAddres}, '', 'server') RETURNING id").ContinueWith((task) =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        DbDataReader reader = task.Result;
                        if (reader?.Read() ?? false)
                        {
                            uint id = (uint)(int)reader["id"];

                            session.UserData = new GuestUserData(id);
                            session.SendPacket(new LoginSuccessOutgoingMessage(session.SocketId, session.UserData.Id, session.UserData.Username, session.UserData.Permissions, session.UserData.GetVars("*")));

                            if (PlatformRacing3Server.ServerManager.TryGetServer(PlatformRacing3Server.ServerConfig.ServerId, out ServerDetails server))
                            {
                                session.UserData.SetServer(server);
                            }

                            PlatformRacing3Server.ClientManager.Add(session);
                        }
                        else
                        {
                            session.SendPacket(new LoginErrorOutgoingMessage("User data was not found"));
                        }
                    }
                    else if (task.IsFaulted)
                    {
                        GuestLoginIncomingMessage.Logger.Error($"Failed to insert login", task.Exception);

                        session.SendPacket(new LoginErrorOutgoingMessage("Critical error"));
                    }
                }));
            }
        }
    }
}
