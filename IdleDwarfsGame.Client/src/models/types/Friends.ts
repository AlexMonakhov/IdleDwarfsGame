// Друзья и сессии (IdleDwarfsGame.Server/Controllers/Responses/FriendResponses.cs, SessionResponses.cs)

export interface FriendInfo {
    userId: string;
    username: string;
    // Есть открытая вкладка с игрой
    isOnline: boolean;
    // null — ни разу не заходил с тех пор, как появились сессии
    lastSeenAt: string | null;
    friendsSince: string;
}

export interface FriendRequestInfo {
    // Входящая заявка — отправитель, исходящая — получатель
    userId: string;
    username: string;
    createdAt: string;
}

export interface FriendsOverview {
    serverTime: string;
    maxFriends: number;
    friends: FriendInfo[];
    incoming: FriendRequestInfo[];
    outgoing: FriendRequestInfo[];
}

// Accepted — у игрока уже была встречная заявка, и вы сразу стали друзьями
export type FriendRequestOutcome = 'Sent' | 'Accepted';

export interface SendFriendRequestResult {
    outcome: FriendRequestOutcome;
}

export interface SessionStarted {
    sessionId: string;
    heartbeatSeconds: number;
}

export interface SessionHeartbeat {
    incomingFriendRequests: number;
}
