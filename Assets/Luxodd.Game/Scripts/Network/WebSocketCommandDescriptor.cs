using System;

namespace Luxodd.Game.Scripts.Network
{
    public enum CommandRequestType
    {
        GetProfileRequest,
        GetUserBalanceRequest,
        AddBalanceRequest,
        ChargeUserBalanceRequest,
        GameStatsRequest,
        HealthStatusCheckRequest,
        //statistics
        LevelBeginRequest,
        LevelEndRequest,
        GetUserBestScoreRequest,
        GetUserRecentGamesRequest,
        LeaderboardRequest,
        GetUserDataRequest,
        SetUserDataRequest,
        GetGameSessionInfoRequest
    }

    public enum CommandResponseType
    {
        GetProfileResponse,
        GetUserBalanceResponse,
        AddBalanceResponse,
        ChargeUserBalanceResponse,
        GameStatsResponse,
        HealthStatusCheckResponse,
        //statistics
        LevelBeginResponse,
        LevelEndResponse,
        GetUserBestScoreResponse,
        GetUserRecentGamesResponse,
        LeaderboardResponse,
        GetUserDataResponse,
        SetUserDataResponse,
        GetGameSessionInfoResponse
    }

    public enum PayloadParameterTypes
    {
        Int,
        Float,
        String,
    }

    public static class CommandTypeExtensions
    {
        public static CommandRequestType ToCommandRequestType(this CommandResponseType type)
        {
            return type switch
            {
                CommandResponseType.GetProfileResponse => CommandRequestType.GetProfileRequest,
                CommandResponseType.GetUserBalanceResponse => CommandRequestType.GetUserBalanceRequest,
                CommandResponseType.AddBalanceResponse => CommandRequestType.AddBalanceRequest,
                CommandResponseType.ChargeUserBalanceResponse => CommandRequestType.ChargeUserBalanceRequest,
                CommandResponseType.GameStatsResponse => CommandRequestType.GameStatsRequest,
                CommandResponseType.HealthStatusCheckResponse => CommandRequestType.HealthStatusCheckRequest,
                CommandResponseType.LevelBeginResponse => CommandRequestType.LevelBeginRequest,
                CommandResponseType.LevelEndResponse => CommandRequestType.LevelEndRequest,
                CommandResponseType.GetUserBestScoreResponse => CommandRequestType.GetUserBestScoreRequest,
                CommandResponseType.GetUserRecentGamesResponse => CommandRequestType.GetUserRecentGamesRequest,
                CommandResponseType.LeaderboardResponse => CommandRequestType.LeaderboardRequest,
                CommandResponseType.GetUserDataResponse => CommandRequestType.GetUserDataRequest,
                CommandResponseType.SetUserDataResponse => CommandRequestType.SetUserDataRequest,
                CommandResponseType.GetGameSessionInfoResponse => CommandRequestType.GetGameSessionInfoRequest,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static CommandResponseType ToCommandResponseType(this CommandRequestType type)
        {
            return type switch
            {
                CommandRequestType.GetProfileRequest => CommandResponseType.GetProfileResponse,
                CommandRequestType.GetUserBalanceRequest => CommandResponseType.GetUserBalanceResponse,
                CommandRequestType.AddBalanceRequest => CommandResponseType.AddBalanceResponse,
                CommandRequestType.ChargeUserBalanceRequest => CommandResponseType.ChargeUserBalanceResponse,
                CommandRequestType.GameStatsRequest => CommandResponseType.GameStatsResponse,
                CommandRequestType.HealthStatusCheckRequest => CommandResponseType.HealthStatusCheckResponse,
                CommandRequestType.LevelBeginRequest => CommandResponseType.LevelBeginResponse,
                CommandRequestType.LevelEndRequest => CommandResponseType.LevelEndResponse,
                CommandRequestType.GetUserBestScoreRequest => CommandResponseType.GetUserBestScoreResponse,
                CommandRequestType.GetUserRecentGamesRequest => CommandResponseType.GetUserRecentGamesResponse,
                CommandRequestType.LeaderboardRequest => CommandResponseType.LeaderboardResponse,
                CommandRequestType.GetUserDataRequest => CommandResponseType.GetUserDataResponse,
                CommandRequestType.SetUserDataRequest => CommandResponseType.SetUserDataResponse,
                CommandRequestType.GetGameSessionInfoRequest => CommandResponseType.GetGameSessionInfoResponse,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
