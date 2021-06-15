﻿using Platform_Racing_3_Server.Game.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Platform_Racing_3_Server.Game.Chat
{
    internal class ChatRoomManager
    {
        private ConcurrentDictionary<string, ChatRoom> ChatRooms;

        internal ChatRoomManager()
        {
            this.ChatRooms = new ConcurrentDictionary<string, ChatRoom>();
            this.TryCreate(ChatRoomType.System, 1, "chat-Home", "", "Join our offical Discord server! https://discord.gg/xYTvAGP", out _); //User id 1 is isokissa3
        }

        internal ICollection<ChatRoom> Rooms => this.ChatRooms.Values;

        //Dont really like these methods tho I can't come up with better names and funcitonaly
        internal bool TryCreate(ChatRoomType type, string name, string pass, string note, out ChatRoom room) => this.ChatRooms.TryAdd(name, room = new ChatRoom(type, name, pass, note));
        internal bool TryCreate(ChatRoomType type, uint creatorUserId, string name, string pass, string note, out ChatRoom room) => this.ChatRooms.TryAdd(name, room = new ChatRoom(type, creatorUserId, name, pass, note));

        internal bool TryGet(string name, out ChatRoom chatRoom) => this.ChatRooms.TryGetValue(name, out chatRoom);

        internal ChatRoom JoinOrCreate(ClientSession session, string name, string pass, string note, out bool status, uint chatId = 0)
        {
            //Complexity added by concurrency

            while (true)
            {
                if (!this.ChatRooms.TryGetValue(name, out ChatRoom chat)) //Dont use GetOrAdd here to reduce the amount of allocations we do
                {
                    if (!session.IsGuest)
                    {
                        chat = new ChatRoom(ChatRoomType.UserCreated, session.UserData.Id, name, pass, note); //Create chat room with the user already listed in as member

                        status = chat.Join(session, chatId); //Initial join on create should NEVER fail

                        if (!this.ChatRooms.TryAdd(name, chat))
                        {
                            chat.Leave(session); //Leave so we dont hold reference to the chat even tho GC will take care of it

                            continue; //Failed to add the newly created room, check for already existing room
                        }
                    }
                    else
                    {
                        status = false;

                        return null; //We are guest, we can not create rooms
                    }
                }
                else
                {
                    //Password functionality is not properly implemented in the client but keep this anyway
                    status = (string.IsNullOrWhiteSpace(chat.Pass) || chat.Pass == pass) ? chat.Join(session, chatId) : false;
                }

                //If we failed to join to the room return and dont try again
                if (!status)
                {
                    return chat;
                }
                else if (this.ChatRooms.GetOrAdd(name, chat) == chat) //If it was succesfully joined then verify that the the room that we just joined in is listed in globally or add the room if the room is missing
                {
                    return chat;
                }
                else //Everything failed, leave so we dont hold reference to the chat even tho GC will take care of it and try again
                {
                    chat.Leave(session);
                }
            }
        }

        internal void Leave(ClientSession session, string name)
        {
            if (this.ChatRooms.TryGetValue(name, out ChatRoom chat))
            {
                chat.Leave(session);
            }
        }

        internal void Die(ChatRoom chatRoom)
        {
            if (chatRoom.Type != ChatRoomType.System)
            {
                this.ChatRooms.TryRemove(chatRoom.Name, out _);
            }
        }
    }
}
