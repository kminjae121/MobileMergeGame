using System;
using UnityEngine;

namespace _Code.Server
{
    public static class PlayerIdProvider
    {
        private const string LegacyPlayerIdKey = "CatBlast.PlayerId";
        private const string GuestPlayerIdKey = "CatBlast.GuestPlayerId";
        private const string AuthenticatedPlayerIdKey = "CatBlast.AuthenticatedPlayerId";

        public static string PlayerId
        {
            get
            {
                string authenticatedPlayerId = PlayerPrefs.GetString(AuthenticatedPlayerIdKey, string.Empty);
                return !string.IsNullOrWhiteSpace(authenticatedPlayerId) ? authenticatedPlayerId : GuestPlayerId;
            }
        }

        public static string GuestPlayerId
        {
            get
            {
                string guestPlayerId = PlayerPrefs.GetString(GuestPlayerIdKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(guestPlayerId))
                    return guestPlayerId;

                string legacyPlayerId = PlayerPrefs.GetString(LegacyPlayerIdKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(legacyPlayerId))
                {
                    SetGuestPlayerId(legacyPlayerId);
                    return legacyPlayerId;
                }

                guestPlayerId = CreateGuestPlayerId();
                SetGuestPlayerId(guestPlayerId);
                return guestPlayerId;
            }
        }

        public static bool IsSignedIn => !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(AuthenticatedPlayerIdKey, string.Empty));

        public static void SetPlayerId(string playerId)
        {
            SetAuthenticatedPlayerId(playerId);
        }

        public static void SetAuthenticatedPlayerId(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return;

            PlayerPrefs.SetString(AuthenticatedPlayerIdKey, playerId);
            PlayerPrefs.Save();
        }

        public static void ClearAuthenticatedPlayerId()
        {
            PlayerPrefs.DeleteKey(AuthenticatedPlayerIdKey);
            PlayerPrefs.Save();
        }

        private static void SetGuestPlayerId(string playerId)
        {
            PlayerPrefs.SetString(GuestPlayerIdKey, playerId);
            PlayerPrefs.Save();
        }

        private static string CreateGuestPlayerId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (!string.IsNullOrWhiteSpace(deviceId) &&
                !string.Equals(deviceId, SystemInfo.unsupportedIdentifier, StringComparison.Ordinal))
                return $"guest:{deviceId}";

            return $"guest:{Guid.NewGuid():N}";
        }
    }
}
