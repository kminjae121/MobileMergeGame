using System;
using UnityEngine;

namespace _Code.Server
{
    public static class PlayerIdProvider
    {
        private const string PlayerIdKey = "CatBlast.PlayerId";

        public static string PlayerId
        {
            get
            {
                string deviceId = SystemInfo.deviceUniqueIdentifier;
                if (!string.IsNullOrWhiteSpace(deviceId) &&
                    !string.Equals(deviceId, SystemInfo.unsupportedIdentifier, StringComparison.Ordinal))
                {
                    SetPlayerId(deviceId);
                    return deviceId;
                }

                string playerId = PlayerPrefs.GetString(PlayerIdKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(playerId))
                    return playerId;

                playerId = Guid.NewGuid().ToString("N");
                SetPlayerId(playerId);
                return playerId;
            }
        }

        public static void SetPlayerId(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return;

            PlayerPrefs.SetString(PlayerIdKey, playerId);
            PlayerPrefs.Save();
        }
    }
}
