﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient {
    public interface IPlayer {
        int Handle { get; }
        int ServerId { get; }
        Ped Character { get; }
        bool IsLocal { get; }
        bool IsActive { get; }
        string Name { get; }
    }

    public interface IPlayerList : IEnumerable<IPlayer> {
        void RequestPlayerList();

        void ReceivedPlayerList(IList<object> players);

        Task WaitRequested();
    }

    public class NativePlayer : IPlayer {
        readonly Player player;

        public NativePlayer(Player player)
        {
            this.player = player;
        }

        public int Handle => player.Handle;
        public int ServerId => player.ServerId;
        public Ped Character => player.Character;
        public bool IsLocal => player == Game.Player;
        public bool IsActive => NetworkIsPlayerActive(player.Handle);
        public string Name => player.Name;
    }

    public class NativePlayerList : IPlayerList {
        readonly PlayerList playerList;

        public NativePlayerList(PlayerList playerList)
        {
            this.playerList = playerList;
        }

        public IEnumerator<IPlayer> GetEnumerator()
        {
            foreach (Player player in playerList) {
                yield return new NativePlayer(player);
            }
        }

        public void RequestPlayerList()
        {
            // we are a local-only player list
        }

        public void ReceivedPlayerList(IList<object> players)
        {
        }

        public Task WaitRequested()
        {
            // we instantly complete, as we always have all players
            return Task.FromResult(0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (Player player in playerList) {
                yield return new NativePlayer(player);
            }
        }
    }

    public class InfinityPlayer : IPlayer {
        public InfinityPlayer(int serverId, string name)
        {
            ServerId = serverId;
            Name = name;
        }

        public int Handle => GetPlayerFromServerId(ServerId);

        public int ServerId { get; }

        public Ped Character {
            get {
                if (Handle >= 0) {
                    int ped = GetPlayerPed(Handle);

                    if (ped > 0) {
                        return new Ped(ped);
                    }
                }

                return null;
            }
        }

        public bool IsLocal => ServerId == GetPlayerServerId(PlayerId());
        public bool IsActive => Handle != -1 && NetworkIsPlayerActive(Handle);

        public string Name { get; }
    }

    public class InfinityPlayerList : IPlayerList {
        readonly PlayerList playerList;
        readonly Dictionary<int, InfinityPlayer> remotePlayerList;

        int updatingPlayerList;

        public InfinityPlayerList(PlayerList playerList)
        {
            this.playerList = playerList;
            remotePlayerList = new Dictionary<int, InfinityPlayer>();
        }

        public IEnumerator<IPlayer> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        public void RequestPlayerList()
        {
            updatingPlayerList++;
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerList");
        }

        public void ReceivedPlayerList(IList<object> players)
        {
            remotePlayerList.Clear();

            foreach (object playerPair in players) {
                if (playerPair is IDictionary<string, object> playerDict) {
                    if (playerDict.TryGetValue("n", out object nameObj) && playerDict.TryGetValue("s", out object serverIdObj)) {
                        if (nameObj is string name) {
                            int serverId = Convert.ToInt32(serverIdObj);

                            remotePlayerList[serverId] = new InfinityPlayer(serverId, name);
                        }
                    }
                }
            }

            updatingPlayerList--;
        }

        public async Task WaitRequested()
        {
            while (updatingPlayerList > 0) {
                await BaseScript.Delay(0);
            }
        }

        IEnumerator<IPlayer> GetEnumeratorInternal()
        {
            HashSet<int> nearPlayers = new HashSet<int>();

            foreach (Player player in playerList) {
                yield return new NativePlayer(player);
                nearPlayers.Add(player.ServerId);
            }

            foreach (KeyValuePair<int, InfinityPlayer> player in remotePlayerList) {
                if (!nearPlayers.Contains(player.Value.ServerId)) {
                    yield return player.Value;
                }
            }
        }
    }
}